using LabApi.Extensions;
using LabApi.Features.Audio;
using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using SCP_Immersive_Voice.VoiceProfiles;
using System;
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;
using Logger = LabApi.Extensions.Misc.iLogger;

namespace SCP_Immersive_Voice.Decoders
{
    /// <summary>
    /// Handles Opus decoding/encoding and applies the SCP voice DSP pipeline on float PCM audio. 
    /// Fully float-native, zero-copy where possible, minimal allocations, and real-time safe.
    /// </summary>
    public static class ScpVoiceDecoder
    {
        #region Private Static Audio Hardware Resources
        // Shared Opus decoder: Opus → float PCM (Target-typed new expression)
        private static readonly OpusDecoder Decoder = new();

        // Shared Opus encoder: float PCM → Opus
        private static readonly OpusEncoder Encoder = new(OpusApplicationType.Voip);

        private static readonly int SampleRate = VoiceChatSettings.SampleRate;
        private static readonly int FrameSize = VoiceChatSettings.PacketSizePerChannel;
        private static readonly float[] FloatBuffer = new float[FrameSize];
        #endregion

        #region Core Decoding & Encoding Pipelines
        /// <summary>
        /// Decodes an incoming VoiceMessage from Opus to float[] PCM (-1..1).
        /// Returns an empty array on invalid or empty data.
        /// </summary>
        public static float[] Decode(VoiceMessage msg)
        {
            // C# 9.0 Pattern Matching structural guard replacing redundant try-catch blocks
            if (msg.Data is null || msg.DataLength <= 0)
                return Array.Empty<float>();

            int samples = Decoder.Decode(msg.Data, msg.DataLength, FloatBuffer);

            if (samples is <= 0 || samples > FrameSize)
                return Array.Empty<float>();

            // Copy only valid samples (decoder uses shared buffer execution space)
            float[] output = new float[samples];
            Array.Copy(FloatBuffer, output, samples);
            return output;
        }

        /// <summary>
        /// Encodes float PCM (-1..1) to Opus using a shared encoder instance.
        /// </summary>
        public static byte[] EncodeToOpus(float[] pcm)
        {
            if (pcm is null or { Length: 0 })
                return Array.Empty<byte>();

            byte[] encoded = new byte[AudioTransmitter.MaxEncodedSize];
            int len = Encoder.Encode(pcm, encoded, pcm.Length);

            if (len <= 0)
                return Array.Empty<byte>();

            if (len == encoded.Length)
                return encoded;

            byte[] trimmed = new byte[len];
            Buffer.BlockCopy(encoded, 0, trimmed, 0, len);
            return trimmed;
        }
        #endregion

        #region DSP Graph Application Core
        /// <summary>
        /// Applies the SCP voice DSP pipeline. Fully float-native.
        /// <para>
        /// Decode → AGC(pre-DSP) → DSP (pitch, formant, reverb…) → OutputGain(preset) → Limiter → return pcm
        /// </para>
        /// </summary>
        public static float[] ApplyEffects(float[] pcm, Player scp)
        {
            if (pcm is null or { Length: 0 } || scp is null)
                return pcm ?? Array.Empty<float>();

            // 1. Resolve architectural preset profile
            var activePreset = ScpVoiceProfiles.GetPreset(scp);
            if (activePreset is null || !activePreset.Enable)
                return pcm;

            // 2. Pre-DSP Automatic Gain Control normalization
            pcm = ApplyAgc(pcm, targetPeak: 0.7f, maxGain: 3f);

            // 3. DSP Pipeline Forensic Processing Loop
            var pipeline = ScpVoiceProfiles.GetPipelineFor(scp, activePreset);
            if (pipeline is not null)
            {
                // INTENT: Fetching a local array reference insulation protects the hot-path loop from out-of-bounds 
                // crashes if a separate management thread triggers an atomic reference swap mid-execution.
                IAudioEffect[] localEffects = pipeline.Effects;
                int effectCount = localEffects.Length;

                for (int i = 0; i < effectCount; i++)
                {
                    var effect = localEffects[i];
                    if (effect is null) continue;

                    try
                    {
                        effect.Process(pcm, pcm.Length);

                        // High-performance hot check looking for arithmetic NaN or Infinity anomalies
                        for (int sampleIdx = 0; sampleIdx < pcm.Length; sampleIdx++)
                        {
                            float sample = pcm[sampleIdx];
                            if (float.IsNaN(sample) || float.IsInfinity(sample))
                            {
                                Logger.Error(nameof(ScpVoiceDecoder), $"[DSP-CRASH] Effect '{effect.GetType().Name}' generated an invalid float value ({sample}) at sample index {sampleIdx}! Defensively silencing the pipeline.");

                                // Defensive safeguard: Force-purge the tainted array back to zero to prevent downstream crash waves
                                Array.Clear(pcm, 0, pcm.Length);
                                return pcm;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(nameof(ScpVoiceDecoder), $"[DSP-EXCEPTION] Exception thrown by audio effect block '{effect.GetType().Name}': {ex.Message}");
                    }
                }
            }

            // 4. Apply Output Gain spectrum modifiers
            ApplyOutputGain(pcm, activePreset.OutputGain);

            // 5. Studio-grade Soft Limiter saturation safety guard
            ApplyLimiter(pcm, threshold: 0.98f);

            return pcm;
        }
        #endregion

        #region Advanced Mathematical DSP Sub-Filters
        /// <summary>
        /// Returns true if the frame is considered silent based on amplitude threshold metrics.
        /// </summary>
        public static bool IsSilent(float[] pcm, float threshold = 0.01f)
        {
            if (pcm is null or { Length: 0 })
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

                // If sample enters the hot zone, soft-compress it smoothly
                if (absV > 0.8f)
                {
                    float excess = absV - 0.8f;

                    // Fast polynomial soft-knee emulation block
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
        public static void ApplyOpusPostFilter(float[] pcm)
        {
            const float hfBoost = 1.35f;
            const float smoothing = 0.995f;
            float prev = 0f;

            for (int i = 0; i < pcm.Length; i++)
            {
                float v = pcm[i];

                // de-ringing phase filtration
                float smooth = (v * (1f - smoothing)) + (prev * smoothing);
                prev = smooth;

                // high-frequency harmonic restoration
                smooth *= hfBoost;

                // hardware boundary clamping
                if (smooth > 1f) smooth = 1f;
                if (smooth < -1f) smooth = -1f;

                pcm[i] = smooth;
            }
        }

        /// <summary>
        /// Applies soft compression to a PCM audio sample array using the specified threshold and compression ratio.
        /// </summary>
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
            // FLUENT API CORE ALIGNMENT: 
            // Leveraged the custom zero-allocation MathExtensions scalar clamp straight on the float parameter primitive
            gain = gain.Clamp(0.0f, 3.0f);
            if (Math.Abs(gain - 1.0f) < 0.001f) return;

            for (int i = 0; i < pcm.Length; i++)
            {
                pcm[i] *= gain;
            }
        }
        #endregion
    }
}