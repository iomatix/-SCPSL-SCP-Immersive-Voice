namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    public class FleshCrackleEffect : IAudioEffect
    {
        private readonly float _amount;
        private float _smooth;
        private static readonly Random _rng = new Random();

        public FleshCrackleEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(float[] samples, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float x = samples[i];

                // 1. Envelope follower
                float env = Math.Abs(x);

                // 2. Crackle probability grows with loudness
                float chance = 0.002f + env * 0.03f * _amount;

                float crack = 0f;
                if (_rng.NextDouble() < chance)
                {
                    crack = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.6f * _amount;
                }

                // 3. Smooth to avoid digital harshness
                _smooth = _smooth + 0.2f * (crack - _smooth);

                // 4. Mix
                samples[i] = x + _smooth * 0.5f;
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
