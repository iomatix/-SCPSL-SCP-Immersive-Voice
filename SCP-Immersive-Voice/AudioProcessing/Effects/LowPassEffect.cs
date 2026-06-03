namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using LabApi.Features.Audio;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Smooths and muffles high frequencies using a one‑pole low‑pass filter.
    /// Ideal for SCP‑049 mask muffling, SCP‑106 void dampening, or radio bandwidth
    /// limitation.
    /// </summary>
    public class LowPassEffect : IAudioEffectShort
    {
        private readonly float _cutoff;

        // Filter memory (float required for stability)
        private float _prev;

        public LowPassEffect(float cutoffHz)
        {
            // cutoff must be within a safe and meaningful range
            _cutoff = Clamp(cutoffHz, 20f, 20000f);
        }

        public void Process(short[] pcm, int length)
        {
            // Precompute filter coefficients
            float rc = 1.0f / (2f * (float)Math.PI * _cutoff);
            float dt = 1.0f / AudioTransmitter.SampleRate;
            float alpha = dt / (rc + dt);

            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1
                float x = pcm[i] / 32768f;

                // 1. One-pole low-pass filter
                _prev += alpha * (x - _prev);

                // 2. Convert back to PCM
                int sample = (int)(_prev * 32767f);

                // 3. Clamp to valid PCM range
                if (sample > short.MaxValue) sample = short.MaxValue;
                if (sample < short.MinValue) sample = short.MinValue;

                pcm[i] = (short)sample;
            }
        }

        private static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }
    }
}
