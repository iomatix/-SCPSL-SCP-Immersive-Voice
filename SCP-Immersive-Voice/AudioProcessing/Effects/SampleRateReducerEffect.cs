namespace SCP_Immersive_Voice.AudioProcessing.Effects
{

    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    public class SampleRateReducerEffect : IAudioEffect
    {
        private readonly float _amount;
        private float _hold;

        public SampleRateReducerEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1f);
        }

        public void Process(float[] samples, int length)
        {
            int skip = (int)(1 + _amount * 12f); // 1–12 samples per hold

            for (int i = 0; i < length; i++)
            {
                if (i % skip == 0)
                    _hold = samples[i];

                samples[i] = _hold;
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
