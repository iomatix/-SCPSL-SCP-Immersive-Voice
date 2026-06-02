namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    public class ChirpEffect : IAudioEffect
    {
        private readonly float _amount;
        private float _phase;
        private static readonly Random _rng = new Random();

        public ChirpEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1f);
        }

        public void Process(float[] samples, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // random chance for "chirp"
                if (_rng.NextDouble() < _amount * 0.0008)
                {
                    _phase = 0f; // reset chirp
                }

                // if chrip is active
                if (_phase < Math.PI * 2)
                {
                    float chirp = (float)Math.Sin(_phase * 20f) * 0.15f * _amount;
                    samples[i] += chirp;
                    _phase += 0.15f;
                }
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