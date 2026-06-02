namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    public class WetDecayEffect : IAudioEffect
    {
        private readonly float _amount;
        private float _lpState;
        private float _env;
        private float _phase;
        private static readonly Random _rng = new Random();

        public WetDecayEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(float[] samples, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float x = samples[i];

                // 1. Envelope follower (meat reactive to volume)
                float abs = Math.Abs(x);
                _env += 0.01f * (abs - _env);

                // 2. Slowly modulated cutoff (wet, unstable filter)
                _phase += 0.0015f;
                float wobble = 0.5f + 0.5f * (float)Math.Sin(_phase + _env * 3f);

                // simple lowpass with modulation
                float cutoff = 0.25f + 0.5f * wobble; // 0.25–0.75
                _lpState = _lpState + cutoff * (x - _lpState);
                float wetBase = _lpState;

                // 3. Random "wet pops" based on volume
                float popChance = 0.0025f + _env * 0.02f * _amount;
                float pop = 0f;
                if (_rng.NextDouble() < popChance)
                {
                    pop = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.4f * _amount;
                }

                float wet = wetBase + pop;

                // 4. Mix
                samples[i] = x * (1f - _amount * 0.6f) + wet * (_amount * 0.6f);
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
