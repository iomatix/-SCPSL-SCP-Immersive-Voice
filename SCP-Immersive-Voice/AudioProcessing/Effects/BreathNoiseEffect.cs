namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    public class BreathNoiseEffect : IAudioEffect
    {
        private readonly float _intensity;
        private static readonly Random _rng = new Random();
        private float _bpLast;      // band-pass memory
        private float _smoothLast;  // smoothing memory
        private float _t;           // time accumulator

        public BreathNoiseEffect(float intensity)
        {
            _intensity = Clamp(intensity, 0f, 2f);
        }

        public void Process(float[] samples, int length)
        {

            for (int i = 0; i < length; i++)
            {
                // 1. White noise
                float noise = (float)(_rng.NextDouble() * 2.0 - 1.0);

                // 2. Band-pass filter (simple resonant BP)
                float bp = noise - _bpLast * 0.98f;
                _bpLast = bp;

                // 3. Smoothing (soften harsh edges)
                float smooth = _smoothLast + 0.1f * (bp - _smoothLast);
                _smoothLast = smooth;

                // 4. Slow amplitude modulation (breathing feel)
                _t += 0.002f;
                float mod = 0.85f + 0.15f * (float)Math.Sin(_t);

                float breath = smooth * mod * _intensity * 0.25f;

                // 5. Mix with original signal
                samples[i] = samples[i] + breath;
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
