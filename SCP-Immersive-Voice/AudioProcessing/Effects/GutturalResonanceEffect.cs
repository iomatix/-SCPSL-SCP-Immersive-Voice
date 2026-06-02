namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    public class GutturalResonanceEffect : IAudioEffect
    {
        private readonly float _amount;
        private float _res;
        private float _phase;

        public GutturalResonanceEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(float[] samples, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float x = samples[i];

                // wolna modulacja niskiego rezonansu
                _phase += 0.002f;
                float mod = 0.6f + 0.4f * (float)Math.Sin(_phase);

                // prosty resonator (low-shelf style)
                _res = _res + mod * 0.1f * (x - _res);

                samples[i] = x * (1f - _amount * 0.5f) + _res * (_amount * 0.5f);
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
