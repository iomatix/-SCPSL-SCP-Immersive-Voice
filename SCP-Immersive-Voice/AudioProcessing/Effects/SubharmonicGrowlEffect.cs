namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    public class SubharmonicGrowlEffect : IAudioEffect
    {
        private readonly float _amount;
        private float _phase;

        public SubharmonicGrowlEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(float[] samples, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float x = samples[i];

                // subharmonic = sin(phase * 0.5)
                _phase += 0.02f;
                float sub = (float)Math.Sin(_phase * 0.5f) * _amount * 0.3f;

                samples[i] = x + sub;
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
