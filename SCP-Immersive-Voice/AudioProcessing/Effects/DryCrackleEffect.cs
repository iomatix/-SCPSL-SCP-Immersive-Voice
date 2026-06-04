namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Dry, brittle crackle transients with organic smoothing and nonlinear shaping.
    /// Ideal for dusty, decayed or corrupted textures.
    /// </summary>
    public class DryCrackleEffect : IAudioEffect
    {
        private readonly float _amount;

        private float _smooth;

        // Per-instance RNG for isolated crackle behavior
        private readonly Random _rng;

        public DryCrackleEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Probability increases with amplitude
                float chance = 0.001f + Math.Abs(dry) * 0.018f * _amount;

                float crack = 0f;

                if (_rng.NextDouble() < chance)
                {
                    // Dry brittle impulse
                    float raw = (float)(_rng.NextDouble() * 2.0 - 1.0);
                    crack = raw * 0.28f;

                    // Anti-alias shaping
                    crack *= 0.82f + 0.18f * crack;
                }

                // Organic decay
                _smooth += 0.23f * (crack - _smooth);

                // Mix
                float mixed = dry + _smooth * 0.38f;

                // Soft clip
                pcm[i] = (float)Math.Tanh(mixed * 1.05f);
            }
        }
    }
}