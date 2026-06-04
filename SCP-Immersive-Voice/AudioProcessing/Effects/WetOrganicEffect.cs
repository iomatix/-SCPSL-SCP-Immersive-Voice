namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Subtle organic wet layer simulating moist, living tissue.
    /// Envelope-driven movement, dual-filter wobble, gentle wet noise
    /// and nonlinear shaping. Ideal for SCP-3114 or SCP-939.
    /// </summary>
    public class WetOrganicEffect : IAudioEffect
    {
        public string Name => "Wet Organic";

        private readonly float _amount;

        private float _lp;
        private float _hp;
        private float _env;
        private float _phase;

        private readonly Random _rng;

        public WetOrganicEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Soft envelope follower (living tissue reactivity)
                float abs = Math.Abs(dry);
                _env += 0.026f * (abs - _env);

                // Slow dual-filter wobble (slimy movement)
                _phase += 0.00112f + _env * 0.00031f;
                float wobble = 0.5f + 0.5f * (float)Math.Sin(_phase * 1.31f);

                float lpCut = 0.16f + wobble * 0.34f;
                float hpCut = 0.80f - wobble * 0.24f;

                // Low-pass smear
                _lp += lpCut * (dry - _lp);

                // High-pass shifting texture
                _hp = dry - _lp * hpCut;

                // Gentle wet noise (subtle friction)
                float noise = 0f;
                if (_amount > 0.04f)
                {
                    float raw = (float)(_rng.NextDouble() * 2.0 - 1.0);
                    noise = raw * 0.028f * _amount;
                }

                // Nonlinear shaping (living tissue softness)
                float shaped = (_hp + noise) * (0.89f + 0.11f * (_hp + noise));

                // Soft saturation
                float wet = (float)Math.Tanh(shaped * 1.33f);

                // Wet/dry mix
                pcm[i] = dry * (1f - _amount * 0.42f) + wet * (_amount * 0.42f);
            }
        }
    }
}