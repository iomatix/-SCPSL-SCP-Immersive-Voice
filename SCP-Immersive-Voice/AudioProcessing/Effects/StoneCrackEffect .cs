namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    public class StoneCrackEffect : IAudioEffect
    {
        private readonly float _intensity;
        private static readonly Random _rng = new Random();
        private float _last;

        public StoneCrackEffect(float intensity = 1.0f)
        {
            _intensity = Clamp(intensity, 0f, 2f);
        }

        public void Process(float[] samples, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // burst crackle
                float burst = (_rng.NextDouble() < 0.015 * _intensity)
                    ? (float)(_rng.NextDouble() * 2.0 - 1.0)
                    : 0f;

                // high strong transients
                float crack = burst * 0.8f + (_last * 0.2f);
                _last = crack;

                samples[i] += crack * 0.6f * _intensity;
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
