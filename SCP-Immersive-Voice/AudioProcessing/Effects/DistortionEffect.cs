namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// High-quality analog-style distortion with soft-knee saturation,
    /// drive shaping, anti-alias tilt and output limiting. Ideal for
    /// monstrous SCP voices, corrupted radio or aggressive growls.
    /// </summary>
    public class DistortionEffect : IAudioEffect
    {
        private readonly float _drive;
        private readonly float _preGain;
        private readonly float _postGain;

        public DistortionEffect(float drive)
        {
            _drive = Clamp(drive, 0f, 5f);

            // Pre-emphasis: push mids/highs into saturation
            _preGain = 0.8f + _drive * 0.4f;

            // Post-emphasis: restore body after saturation
            _postGain = 1f / (1f + _drive * 0.16f);
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float x = pcm[i];

                // Pre-emphasis
                x *= _preGain;

                // Drive
                x *= _drive;

                // Soft-knee pre-shaping (tube-like)
                float knee = x * (0.92f + 0.08f * x);

                // Main saturation (analog-style)
                float sat = (float)Math.Tanh(knee * 0.95f);

                // Anti-alias tilt (reduces harsh HF)
                sat *= 0.88f + 0.12f * sat;

                // Post-emphasis (restore low-end)
                sat *= _postGain;

                // Output limiter (prevents runaway peaks)
                sat = (float)Math.Tanh(sat * 1.18f);

                pcm[i] = sat;
            }
        }
    }
}