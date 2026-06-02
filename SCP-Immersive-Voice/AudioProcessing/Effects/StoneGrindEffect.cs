namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    public class StoneGrindEffect : IAudioEffect
    {
        private readonly float _intensity;
        private float _smooth;
        private static readonly Random _rng = new Random();

        public StoneGrindEffect(float intensity = 1.0f)
        {
            _intensity = Clamp(intensity, 0f, 2f);
        }

        public void Process(float[] samples, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // base noise
                float noise = (float)(_rng.NextDouble() * 2.0 - 1.0);

                // band-pass 200-800 Hz (simple filter)
                float bp = noise - _smooth * 0.85f;
                _smooth = bp;

                // modulation (friction)
                float mod = 0.7f + 0.3f * (float)Math.Sin(i * 0.002f);

                float grind = bp * mod * _intensity * 0.4f;

                samples[i] += grind;
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
