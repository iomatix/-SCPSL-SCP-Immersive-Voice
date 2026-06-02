namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    public class BitcrushEffect : IAudioEffect
    {
        private readonly float _amount;

        public BitcrushEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1f);
        }

        public void Process(float[] samples, int length)
        {
            int steps = (int)(256 * (1f - _amount)); // 256 → 8-bit
            if (steps < 2) steps = 2;

            for (int i = 0; i < length; i++)
            {
                float x = samples[i];
                float crushed = (float)Math.Round(x * steps) / steps;
                samples[i] = crushed;
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
