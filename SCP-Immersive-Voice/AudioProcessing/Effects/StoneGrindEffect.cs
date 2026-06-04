namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Continuous stone-grinding engine with granular texture, nonlinear resonance
    /// and heavy material damping. Ideal for SCP-173 movement or massive stone friction.
    /// Produces dense, organic, physical scraping.
    /// </summary>
    public class StoneGrindEffect : IAudioEffect
    {
        public string Name => "Stone Grind";

        private readonly float _intensity;

        private float _bpState;
        private float _smooth;
        private float _grainPhase;
        private float _resonance;

        // Per-instance RNG for stable granular texture
        private readonly Random _rng;

        public StoneGrindEffect(float intensity = 1.0f)
        {
            _intensity = Clamp(intensity, 0f, 2f);
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Granular noise (stone texture)
                float grain = (float)(_rng.NextDouble() * 2.0 - 1.0);

                // Irregular friction movement
                _grainPhase += 0.0021f + _intensity * 0.0011f;
                float grainMod = 0.70f + 0.30f * (float)Math.Sin(_grainPhase * 1.28f);

                float textured = grain * grainMod;

                // Band-pass shaping (stone friction band)
                float bp = textured - _bpState * 0.83f;
                _bpState = bp;

                // Stone body resonance
                _resonance += 0.115f * (bp - _resonance);
                float ring = (float)Math.Sin(_resonance * 13.8f) * 0.16f;

                // Combine friction + resonance
                float grind = bp * 0.84f + ring * 0.16f;

                // Material damping (heavy friction smoothing)
                _smooth += 0.175f * (grind - _smooth);

                // Nonlinear hardness shaping
                float shaped = _smooth * (0.86f + 0.14f * _smooth);

                // Stone hardness saturation
                float saturated = (float)Math.Tanh(shaped * 2.45f);

                // Mix with original
                pcm[i] = dry + saturated * _intensity;
            }
        }
    }
}