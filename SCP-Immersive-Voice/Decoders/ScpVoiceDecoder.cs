namespace SCP_Immersive_Voice.Decoders
{
    using LabApi.Features.Audio;
    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.VoiceProfiles;
    using System;
    using VoiceChat.Codec;
    using VoiceChat.Codec.Enums;
    using VoiceChat.Networking;

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
        /// Applies the SCP voice DSP pipeline, output gain, and optional normalization.
        /// Fully float-native.
        /// </summary>
        public static float[] ApplyEffects(float[] pcm, Player scp)
        {
            if (pcm == null || pcm.Length == 0)
                return pcm ?? Array.Empty<float>();

            // Apply effects on healthy signal
            var pipeline = ScpVoiceProfiles.GetPipelineFor(scp);
            pipeline.Process(pcm, pcm.Length);

            // Gain after DSP
            var preset = ScpVoiceProfiles.GetPreset(scp);
            if (preset.OutputGain != 1f)
            {
                float gain = preset.OutputGain;
                for (int i = 0; i < pcm.Length; i++)
                {
                    float v = pcm[i] * gain;
                    if (v > 1f) v = 1f;
                    if (v < -1f) v = -1f;
                    pcm[i] = v;
                }
            }

            // Clipping fix
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
        /// Normalizes the buffer in-place so that the peak amplitude reaches targetPeak.
        /// </summary>
        /// <param name="pcm">An array of floating-point PCM audio samples to process.</param>
        /// <param name="targetPeak"></param>
        private static void NormalizeInPlace(float[] pcm, float targetPeak = 0.9f)
        {
            float max = 0f;

            for (int i = 0; i < pcm.Length; i++)
            {
                float abs = Math.Abs(pcm[i]);
                if (abs > max) max = abs;
            }

            if (max < 0.0001f)
                return;

            float gain = targetPeak / max;

            for (int i = 0; i < pcm.Length; i++)
            {
                float v = pcm[i] * gain;
                if (v > 1f) v = 1f;
                if (v < -1f) v = -1f;
                pcm[i] = v;
            }
        }

        /// <summary>
        /// Limits the buffer to threshold.
        /// </summary>
        /// <param name="pcm">An array of floating-point PCM audio samples to process.</param>
        /// <param name="threshold"></param>
        private static void ApplyLimiter(float[] pcm, float threshold = 0.98f)
        {
            float t = Math.Abs(threshold);

            for (int i = 0; i < pcm.Length; i++)
            {
                float v = pcm[i];

                if (v > t)
                    v = t;
                else if (v < -t)
                    v = -t;

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



    }
}