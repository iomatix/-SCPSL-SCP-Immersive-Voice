namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    public class WetOrganicEffect : IAudioEffect
    {
        private readonly float _amount;
        private float _lp;
        private float _hp;
        private float _phase;
        private static readonly Random _rng = new Random();

        public WetOrganicEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(float[] samples, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float x = samples[i];

                // 1. Slow filter modulation (slimy effect)
                _phase += 0.0012f;
                float wobble = 0.5f + 0.5f * (float)Math.Sin(_phase * 1.4f);

                // dynamic cutoffs based on wobble
                float lpCut = 0.2f + wobble * 0.3f;  // 0.2–0.5
                float hpCut = 0.8f - wobble * 0.2f;  // 0.6–0.8

                // 2. Low-pass
                _lp = _lp + lpCut * (x - _lp);

                // 3. High-pass
                _hp = x - _lp * hpCut;

                // 4. Subtle organic noise
                float noise = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.05f * _amount;

                // 5. Mix
                float wet = _hp + noise;
                samples[i] = x * (1f - _amount * 0.4f) + wet * (_amount * 0.4f);
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
