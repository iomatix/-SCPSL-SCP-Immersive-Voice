namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Organic wet-decay engine simulating moist, unstable, fleshy textures.
    /// Envelope-driven smear, wobbling low-pass tissue filtering, randomized
    /// wet pops and nonlinear shaping. Ideal for SCP-106, SCP-939, SCP-049-2
    /// or SCP-610. Float-native and stable.
    /// </summary>
    public class WetDecayEffect : IAudioEffect
    {
        public string Name => "Wet Decay";

        private readonly float _amount;

        private float _lpState;
        private float _env;
        private float _phase;

        // Per-instance RNG for isolated wet-pop behavior
        private readonly Random _rng;

        public WetDecayEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Envelope follower (organic amplitude tracking)
                float abs = Math.Abs(dry);
                float attack = 0.045f;
                float release = 0.018f;
                _env += (abs - _env) * (abs > _env ? attack : release);

                // Wobbling low-pass (wet smear)
                _phase += 0.00135f + _env * 0.00048f;
                float wobble = 0.5f + 0.5f * (float)Math.Sin(_phase + _env * 2.35f);

                float cutoff = 0.10f + 0.70f * wobble;
                _lpState += cutoff * (dry - _lpState);

                float wetBase = _lpState;

                // Random wet pops (organic bubbles / flesh pops)
                float popChance = 0.0021f + _env * 0.0165f * _amount;
                float pop = 0f;

                if (_rng.NextDouble() < popChance)
                {
                    float raw = (float)(_rng.NextDouble() * 2.0 - 1.0);
                    float strength = 0.24f + (float)_rng.NextDouble() * 0.16f;
                    pop = (float)Math.Tanh(raw * 3.1f) * strength * _amount;
                }

                float wet = wetBase + pop;

                // Nonlinear shaping (fleshy smear)
                float shaped = wet * (0.86f + 0.14f * wet);

                // Soft saturation (organic moist character)
                float saturated = (float)Math.Tanh(shaped * 1.92f);

                // Wet/dry mix
                pcm[i] = dry * (1f - _amount * 0.5f) + saturated * (_amount * 0.5f);
            }
        }
    }
}