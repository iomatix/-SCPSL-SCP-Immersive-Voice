namespace SCP_Immersive_Voice.Decoders
{
    using LabApi.Features.Audio;
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.VoiceProfiles;
    using System;
    using VoiceChat.Codec;
    using VoiceChat.Codec.Enums;
    using VoiceChat.Networking;

    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;

    /// <summary>
    /// Handles Opus decoding/encoding and applies the SCP voice DSP pipeline
    /// on float PCM audio. Fully float-native, zero-copy where possible,
    /// minimal allocations, real-time safe.
    /// </summary>
    public static class ScpVoiceDecoder
    {
        // Shared Opus decoder: Opus → float PCM
        private static readonly OpusDecoder _decoder = new OpusDecoder();

        // Shared Opus encoder: float PCM → Opus
        private static readonly OpusEncoder _encoder = new OpusEncoder(OpusApplicationType.Voip); // Audio?


        private static readonly int SampleRate = VoiceChat.VoiceChatSettings.SampleRate;
        private static readonly int FrameSize = VoiceChat.VoiceChatSettings.PacketSizePerChannel;
        private static readonly float[] _floatBuffer = new float[FrameSize];


        /// <summary>
        /// Decodes an incoming VoiceMessage from Opus to float[] PCM (-1..1).
        /// Returns an empty array on invalid or empty data.
        /// </summary>
        public static float[] Decode(VoiceMessage msg)
        {
            try
            {
                if (msg.Data == null || msg.DataLength <= 0)
                    return Array.Empty<float>();
            }
            catch
            {
                return Array.Empty<float>();
            }

            int samples = _decoder.Decode(msg.Data, msg.DataLength, _floatBuffer);

            if (samples <= 0 || samples > FrameSize)
                return Array.Empty<float>();

            // Copy only valid samples (decoder uses shared buffer)
            float[] output = new float[samples];
            Array.Copy(_floatBuffer, output, samples);
            return output;
        }

        /// <summary>
        /// Encodes float PCM (-1..1) to Opus using a shared encoder.
        /// </summary>
        public static byte[] EncodeToOpus(float[] pcm)
        {
            if (pcm == null || pcm.Length == 0)
                return Array.Empty<byte>();

            byte[] encoded = new byte[AudioTransmitter.MaxEncodedSize];
            int len = _encoder.Encode(pcm, encoded, pcm.Length);

            if (len <= 0)
                return Array.Empty<byte>();

            if (len == encoded.Length)
                return encoded;

            byte[] trimmed = new byte[len];
            Buffer.BlockCopy(encoded, 0, trimmed, 0, len);
            return trimmed;
        }

        /// <summary>
        /// Applies the SCP voice DSP pipeline. Fully float-native.
        /// <para>
        /// Decode → AGC(pre-DSP) → DSP (pitch, formant, reverb…) → OutputGain(preset) → Limiter → return pcm
        /// </para>
        /// </summary>
        public static float[] ApplyEffects(float[] pcm, Player scp)
        {
            if (pcm == null || pcm.Length == 0 || scp == null)
                return pcm ?? Array.Empty<float>();

            bool isDynamicScp = scp.Role == PlayerRoles.RoleTypeId.Scp096 ||
                                scp.Role == PlayerRoles.RoleTypeId.Scp106 ||
                                scp.Role == PlayerRoles.RoleTypeId.Scp939 ||
                                scp.Role == PlayerRoles.RoleTypeId.Scp3114;

            // 1. Resolve preset
            var activePreset = ScpVoiceProfiles.GetPreset(scp);
            if (activePreset == null || !activePreset.Enable)
                return pcm;

            // 2. AGC (pre‑DSP)
            pcm = ApplyAgc(pcm, targetPeak: 0.7f, maxGain: 3f);

            // 3. DSP Pipeline Forensic Processing
            var pipeline = ScpVoiceProfiles.GetPipelineFor(scp, activePreset);
            if (pipeline != null)
            {
                //  FORENSIC BLOCK: Process effects one-by-one to pinpoint the exact NaN source
                 for (int i = 0; i < pipeline.Effects.Count; i++)
                {
                    var effect = pipeline.Effects[i];

                    if (effect == null) continue;

                    try
                    {
                        effect.Process(pcm, pcm.Length);

                        // Run an unallocated check for NaN or Infinity anomalies
                        for (int ii = 0; ii < pcm.Length; ii++)
                        {
                            if (float.IsNaN(pcm[ii]) || float.IsInfinity(pcm[ii]))
                            {
                                LabApi.Features.Console.Logger.Error($"[DSP-CRASH] Effect '{effect.GetType().Name}' generated an invalid float value ({pcm[ii]}) at sample index {ii}! This effect is silencing the pipeline.");

                                // Defensive safeguard: Force-purge the tainted array back to zero to prevent downstream crash waves
                                Array.Clear(pcm, 0, pcm.Length);
                                return pcm;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LabApi.Features.Console.Logger.Error($"[DSP-EXCEPTION] Exception thrown by '{effect.GetType().Name}': {ex.Message}");
                    }
                }
            }

            // 4. OutputGain
            ApplyOutputGain(pcm, activePreset.OutputGain);

            // 5. Limiter
            ApplyLimiter(pcm, threshold: 0.98f);

            return pcm;
        }


        /// <summary>
        /// Returns true if the frame is considered silent based on amplitude threshold.
        /// </summary>
        public static bool IsSilent(float[] pcm, float threshold = 0.01f)
        {
            if (pcm == null || pcm.Length == 0)
                return true;

            float absThr = Math.Abs(threshold);

            for (int i = 0; i < pcm.Length; i++)
            {
                if (Math.Abs(pcm[i]) > absThr)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Applies a studio-grade stateless soft-limiter to prevent digital clipping.
        /// Smoothly compresses signals exceeding 0.8f using a soft-knee saturation curve.
        /// </summary>
        private static void ApplyLimiter(float[] pcm, float threshold = 0.98f)
        {
            float t = Math.Abs(threshold);

            for (int i = 0; i < pcm.Length; i++)
            {
                float v = pcm[i];
                float absV = Math.Abs(v);

                // If sample enters the hot zone, soft-compress it
                if (absV > 0.8f)
                {
                    float excess = absV - 0.8f;
                    // Fast polynomial soft-knee emulation
                    absV = 0.8f + excess / (1f + excess * excess);

                    if (absV > t) absV = t;
                    v = Math.Sign(v) * absV;
                }

                pcm[i] = v;
            }
        }

        /// <summary>
        /// Applies a post-filter to PCM audio data to reduce ringing artifacts and enhance high-frequency content.
        /// </summary>
        /// <param name="pcm">An array of floating-point PCM audio samples to process.</param>
        public static void ApplyOpusPostFilter(float[] pcm)
        {
            const float hfBoost = 1.35f;
            const float smoothing = 0.995f;

            float prev = 0f;

            for (int i = 0; i < pcm.Length; i++)
            {
                float v = pcm[i];

                // de-ringing
                float smooth = (v * (1f - smoothing)) + (prev * smoothing);
                prev = smooth;

                // high-frequency restoration
                smooth *= hfBoost;

                // clamp
                if (smooth > 1f) smooth = 1f;
                if (smooth < -1f) smooth = -1f;

                pcm[i] = smooth;
            }
        }

        /// <summary>
        /// Applies soft compression to a PCM audio sample array using the specified threshold and compression ratio.
        /// </summary>
        /// <param name="pcm">The array of PCM audio samples to process.</param>
        /// <param name="threshold">The amplitude threshold above which compression is applied.</param>
        /// <param name="ratio">The compression ratio for samples exceeding the threshold.</param>
        public static void ApplySoftCompressor(float[] pcm, float threshold, float ratio)
        {
            for (int i = 0; i < pcm.Length; i++)
            {
                float v = pcm[i];
                float a = Math.Abs(v);

                if (a > threshold)
                {
                    float excess = a - threshold;
                    excess /= ratio;
                    float compressed = threshold + excess;
                    pcm[i] = Math.Sign(v) * compressed;
                }
            }
        }

        /// <summary>
        /// Pre-DSP Automatic Gain Control to normalize input peaks before modular processing.
        /// </summary>
        private static float[] ApplyAgc(float[] pcm, float targetPeak, float maxGain)
        {
            float peak = 0f;

            for (int i = 0; i < pcm.Length; i++)
            {
                float a = Math.Abs(pcm[i]);
                if (a > peak) peak = a;
            }

            if (peak < 0.0001f)
                return pcm;

            float gain = targetPeak / peak;
            if (gain > maxGain)
                gain = maxGain;

            for (int i = 0; i < pcm.Length; i++)
                pcm[i] *= gain;

            return pcm;
        }

        /// <summary>
        /// Applies preset OutputGain smoothly without redundant clipping loops.
        /// </summary>
        private static void ApplyOutputGain(float[] pcm, float gain)
        {
            gain = Clamp(gain, 0.0f, 3.0f);
            if (Math.Abs(gain - 1.0f) < 0.001f) return; // Skip loop if gain is neutral

            for (int i = 0; i < pcm.Length; i++)
            {
                pcm[i] *= gain;
            }
        }



    }
}