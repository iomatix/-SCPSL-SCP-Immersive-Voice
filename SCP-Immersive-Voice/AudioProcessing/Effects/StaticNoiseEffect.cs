namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Adds subtle or strong static noise to the signal. 
    /// Designed for radio distortion, dimensional interference, 
    /// SCP‑079 corruption, or ambient static layers. 
    /// Noise is smoothed and micro‑modulated to avoid harsh digital artifacts.
    /// </summary>
    public class StaticNoiseEffect : IAudioEffectShort
    {
        private readonly float _amount;

        // Smoothing state to avoid harsh edges
        private float _smooth;

        // Random generator for noise
        private static readonly Random _rng = new Random();

        public StaticNoiseEffect(float amount)
        {
            // amount 0 → no noise
            // amount 1 → strong static
            _amount = Clamp(amount, 0f, 1f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1
                float x = pcm[i] / 32768f;

                // 1. Generate static noise (white noise)
                float noise = (float)(_rng.NextDouble() * 2.0 - 1.0);

                // 2. Micro‑modulation to avoid robotic uniformity
                noise *= 0.02f + _amount * 0.04f;

                // 3. Smooth noise to avoid harsh digital edges
                _smooth += 0.15f * (noise - _smooth);

                // 4. Soft saturation for analog‑style static
                float staticOut = (float)Math.Tanh(_smooth * 2.0f);

                // 5. Mix dry + wet
                float mixed = x + staticOut * _amount;

                // Convert back to PCM
                int sample = (int)(mixed * 32767f);

                // Clamp
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