namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Produces dry, brittle crackle transients. Useful for dusty, old, or damaged
    /// audio textures, SCP‑106 ambience, or environmental decay effects.
    /// </summary>
    public class DryCrackleEffect : IAudioEffectShort
    {
        private readonly float _amount;
        private float _smooth;
        private static readonly Random _rng = new Random();

        public DryCrackleEffect(float amount)
        {
            // amount 0 → no crackle
            // amount 1.5 → very strong crackle
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1 for amplitude analysis
                float x = pcm[i] / 32768f;

                // Probability of crackle increases with signal amplitude
                float chance = 0.001f + Math.Abs(x) * 0.02f * _amount;

                // Generate crackle impulse
                float crack = 0f;
                if (_rng.NextDouble() < chance)
                {
                    // Random impulse in range -0.3..0.3
                    crack = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.3f;
                }

                // Smooth crackle to avoid harsh digital clicks
                _smooth += 0.25f * (crack - _smooth);

                // Mix crackle into original signal
                float mixed = x + _smooth * 0.4f;

                // Convert back to PCM
                int sample = (int)(mixed * 32767f);

                // Clamp to valid PCM range
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
