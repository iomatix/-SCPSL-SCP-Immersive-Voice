namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Generates sharp, stone‑like crack transients with subtle resonance.
    /// Ideal for SCP‑173, stone creatures, or any hard-surface impact texture.
    /// Produces randomized micro‑bursts with smoothing and saturation to avoid
    /// digital harshness and create an organic stone‑fracture character.
    /// </summary>
    public class StoneCrackEffect : IAudioEffectShort
    {
        private readonly float _intensity;

        // Previous crack value for smoothing
        private float _last;

        // Random generator
        private static readonly Random _rng = new Random();

        public StoneCrackEffect(float intensity = 1.0f)
        {
            // intensity 0 → no cracks
            // intensity 2 → very aggressive stone cracking
            _intensity = Clamp(intensity, 0f, 2f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1
                float x = pcm[i] / 32768f;

                // 1. Random burst trigger (stone crack impulse)
                float burst = 0f;
                if (_rng.NextDouble() < 0.015 * _intensity)
                {
                    // Sharp impulse with slight asymmetry
                    burst = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.9f;
                }

                // 2. Add micro‑resonance (stone ringing)
                float resonance = (float)Math.Sin(burst * 12f) * 0.2f;

                // 3. Smooth transient to avoid digital clicks
                float crack = burst * 0.75f + resonance * 0.25f;
                _last += 0.25f * (crack - _last);

                // 4. Soft saturation for natural stone hardness
                float saturated = (float)Math.Tanh(_last * 2.5f);

                // 5. Mix with original signal
                float mixed = x + saturated * (0.6f * _intensity);

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
