namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    public class StaticNoiseEffect : IAudioEffect
    {
        private readonly float _amount;
        private static readonly Random _rng = new Random();

        public StaticNoiseEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1f);
        }

        public void Process(float[] samples, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float noise = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.03f * _amount;
                samples[i] += noise;
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
