namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using LabApi.Features.Audio;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Removes low‑frequency rumble using a one‑pole high‑pass filter. Useful for
    /// radio clarity, SCP‑079 comms, or removing proximity boominess.
    /// </summary>
    public class HighPassEffect : IAudioEffectShort
    {
        private readonly float _cutoff;

        // Filter memory (float required for stability)
        private float _prevLow;

        public HighPassEffect(float cutoffHz)
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

                // 1. Low-pass stage (extract low frequencies)
                float low = _prevLow + alpha * (x - _prevLow);
                _prevLow = low;

                // 2. High-pass = input - low frequencies
                float high = x - low;

                // 3. Convert back to PCM
                int sample = (int)(high * 32767f);

                // 4. Clamp to valid PCM range
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
