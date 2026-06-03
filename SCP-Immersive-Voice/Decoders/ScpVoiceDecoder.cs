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
    /// on 16-bit PCM audio. Designed for real-time voice processing with
    /// short-native DSP, zero-copy where possible, and minimal allocations.
    /// </summary>
    public static class ScpVoiceDecoder
    {
        // Shared Opus decoder: Opus → float PCM
        private static readonly OpusDecoder _decoder = new OpusDecoder();

        // Shared Opus encoder: float PCM → Opus
        private static readonly OpusEncoder _encoder = new OpusEncoder(OpusApplicationType.Voip);

        // Max Opus frame size in samples (48 kHz, mono, 120 ms)
        private const int MaxOpusSamples = 5760;

        // Float buffer for Opus decode (shared, reused)
        private static readonly float[] _floatBuffer = new float[MaxOpusSamples];

        // Optional: enable/disable post-DSP normalization
        private const bool NormalizeEnabled = true;

        /// <summary>
        /// Decodes an incoming VoiceMessage from Opus to 16-bit PCM (short[]).
        /// Returns an empty array on invalid or empty data.
        /// </summary>
        public static short[] Decode(VoiceMessage msg)
        {
            try
            {
                if (msg.Data == null || msg.DataLength <= 0)
                    return Array.Empty<short>();
            }
            catch
            {
                return Array.Empty<short>();
            }

            int samples = _decoder.Decode(msg.Data, msg.DataLength, _floatBuffer);

            if (samples <= 0 || samples > MaxOpusSamples)
                return Array.Empty<short>();

            short[] pcm = new short[samples];

            for (int i = 0; i < samples; i++)
            {
                float f = _floatBuffer[i];

                if (f > 1f) f = 1f;
                if (f < -1f) f = -1f;

                pcm[i] = (short)(f * short.MaxValue);
            }

            return pcm;
        }

        /// <summary>
        /// Encodes 16-bit PCM (short[]) to Opus using a shared encoder.
        /// </summary>
        public static byte[] EncodeToOpus(short[] pcm)
        {
            if (pcm == null || pcm.Length == 0)
                return Array.Empty<byte>();

            float[] f = ToFloat(pcm);

            byte[] encoded = new byte[AudioTransmitter.MaxEncodedSize];
            int len = _encoder.Encode(f, encoded, f.Length);

            if (len <= 0)
                return Array.Empty<byte>();

            if (len == encoded.Length)
                return encoded;

            byte[] trimmed = new byte[len];
            Buffer.BlockCopy(encoded, 0, trimmed, 0, len);
            return trimmed;
        }

        /// <summary>
        /// Applies the SCP voice DSP pipeline, output gain, and optional
        /// normalization to the given PCM buffer in-place.
        /// </summary>
        public static short[] ApplyEffects(short[] pcm, Player scp)
        {
            if (pcm == null || pcm.Length == 0)
                return pcm ?? Array.Empty<short>();

            var pipeline = ScpVoiceProfiles.GetPipelineFor(scp);
            pipeline.Process(pcm, pcm.Length);

            var preset = ScpVoiceProfiles.GetPreset(scp);
            if (preset.OutputGain != 1f)
            {
                float gain = preset.OutputGain;
                for (int i = 0; i < pcm.Length; i++)
                {
                    int v = (int)(pcm[i] * gain);
                    if (v > short.MaxValue) v = short.MaxValue;
                    if (v < short.MinValue) v = short.MinValue;
                    pcm[i] = (short)v;
                }
            }

            if (NormalizeEnabled)
                pcm = Normalize(pcm);

            return pcm;
        }

        /// <summary>
        /// Returns true if the frame is considered silent based on a simple
        /// amplitude threshold.
        /// </summary>
        public static bool IsSilent(short[] pcm, int threshold = 200)
        {
            if (pcm == null || pcm.Length == 0)
                return true;

            for (int i = 0; i < pcm.Length; i++)
            {
                if (Math.Abs(pcm[i]) > threshold)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Normalizes the buffer in-place so that the peak amplitude reaches
        /// targetPeak of full-scale (0–1 range). Skips if signal is effectively
        /// silent.
        /// </summary>
        public static short[] Normalize(short[] pcm, float targetPeak = 0.9f)
        {
            if (pcm == null || pcm.Length == 0)
                return pcm ?? Array.Empty<short>();

            short max = 0;

            for (int i = 0; i < pcm.Length; i++)
            {
                short abs = (short)Math.Abs(pcm[i]);
                if (abs > max) max = abs;
            }

            if (max < 1)
                return pcm;

            float gain = (targetPeak * 32767f) / max;

            for (int i = 0; i < pcm.Length; i++)
            {
                int v = (int)(pcm[i] * gain);
                if (v > short.MaxValue) v = short.MaxValue;
                if (v < short.MinValue) v = short.MinValue;
                pcm[i] = (short)v;
            }

            return pcm;
        }

        /// <summary>
        /// Converts 16-bit PCM to float PCM in the -1..1 range.
        /// </summary>
        private static float[] ToFloat(short[] pcm)
        {
            float[] f = new float[pcm.Length];
            const float inv = 1f / 32768f;

            for (int i = 0; i < pcm.Length; i++)
                f[i] = pcm[i] * inv;

            return f;
        }
    }
}
