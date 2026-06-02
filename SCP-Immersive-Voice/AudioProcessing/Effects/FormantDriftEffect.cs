namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    public class FormantDriftEffect : IAudioEffect
    {
        private readonly float _amount;
        private float _lpState;
        private float _hpState;
        private float _phase;
        private static readonly Random _rng = new Random();

        public FormantDriftEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(float[] samples, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float x = samples[i];

                // 1. Noise-modulated LFO
                _phase += 0.0007f;
                float noise = (float)(_rng.NextDouble() * 2.0 - 1.0);
                float drift = (float)Math.Sin(_phase * 1.3f + noise * 0.5f);

                // 2. Dynamic formant shifting (LP + HP)
                float lpCut = 0.15f + 0.25f * drift; // 0.15–0.40
                float hpCut = 0.85f + 0.10f * drift; // 0.75–0.95

                // Low-pass
                _lpState = _lpState + lpCut * (x - _lpState);

                // High-pass
                _hpState = x - _lpState * hpCut;

                // 3. Mix
                float shifted = _hpState;
                samples[i] = x * (1f - _amount * 0.5f) + shifted * (_amount * 0.5f);
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
