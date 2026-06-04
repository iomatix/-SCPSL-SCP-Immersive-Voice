namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Wet, organic crackle resembling tearing flesh or moist tissue.
    /// Envelope-driven probability, nonlinear shaping and moist decay.
    /// </summary>
    public class FleshCrackleEffect : IAudioEffect
    {
        private readonly float _amount;

        private float _smooth;

        // Per-instance RNG for isolated wet-crackle behavior
        private readonly Random _rng;

        public FleshCrackleEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Envelope: louder input → more wet crackle
                float env = Math.Abs(dry);

                float chance = 0.002f + env * 0.028f * _amount;

                float crack = 0f;

                if (_rng.NextDouble() < chance)
                {
                    // Wet fleshy impulse
                    float raw = (float)(_rng.NextDouble() * 2.0 - 1.0);
                    crack = raw * 0.58f * _amount;

                    // Nonlinear wet shaping
                    crack *= 0.72f + 0.28f * crack;
                }

                // Moist decay
                _smooth += 0.19f * (crack - _smooth);

                // Mix
                float mixed = dry + _smooth * 0.48f;

                // Soft clip
                pcm[i] = (float)Math.Tanh(mixed * 1.18f);
            }
        }
    }
}