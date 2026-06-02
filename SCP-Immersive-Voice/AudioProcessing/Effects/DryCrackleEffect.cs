namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    public class DryCrackleEffect : IAudioEffect
    {
        private readonly float _amount;
        private float _smooth;
        private static readonly Random _rng = new Random();

        public DryCrackleEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(float[] samples, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float x = samples[i];

                float chance = 0.001f + Math.Abs(x) * 0.02f * _amount;

                float crack = 0f;
                if (_rng.NextDouble() < chance)
                    crack = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.3f;

                _smooth = _smooth + 0.25f * (crack - _smooth);

                samples[i] = x + _smooth * 0.4f;
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
