namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Generates wet, organic crackle resembling tearing flesh or moist tissue.
    /// Ideal for SCP‑610, SCP‑049‑2, or any grotesque biological transformation.
    /// </summary>
    public class FleshCrackleEffect : IAudioEffectShort
    {
        private readonly float _amount;
        private float _smooth;
        private static readonly Random _rng = new Random();

        public FleshCrackleEffect(float amount)
        {
            // amount 0 → no crackle
            // amount 1.5 → very strong organic crackle
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1 for envelope analysis
                float x = pcm[i] / 32768f;

                // Envelope follower: louder input → more crackle activity
                float env = Math.Abs(x);

                // Probability of crackle increases with loudness and amount
                float chance = 0.002f + env * 0.03f * _amount;

                // Generate crackle impulse
                float crack = 0f;
                if (_rng.NextDouble() < chance)
                {
                    // Organic, wet crackle impulse in range -0.6..0.6
                    crack = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.6f * _amount;
                }

                // Smooth crackle to avoid harsh digital clicks
                _smooth += 0.2f * (crack - _smooth);

                // Mix crackle into original signal
                float mixed = x + _smooth * 0.5f;

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
