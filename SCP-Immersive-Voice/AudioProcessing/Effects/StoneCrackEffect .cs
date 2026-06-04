namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Clustered stone-fracture generator with micro-resonance and nonlinear shaping.
    /// Ideal for SCP-173 or any hard-surface entity. Produces organic, physical
    /// stone cracking with controlled timing and material stress behavior.
    /// </summary>
    public class StoneCrackEffect : IAudioEffect
    {
        private readonly float _intensity;

        private float _last;
        private float _resonance;
        private int _clusterSamples;
        private float _clusterEnergy;

        // Per-instance RNG for stable, isolated crack behavior
        private readonly Random _rng;

        public StoneCrackEffect(float intensity = 1.0f)
        {
            _intensity = Clamp(intensity, 0f, 2f);
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float x = pcm[i];
                float burst = 0f;

                // Start a new crack cluster
                if (_clusterSamples <= 0)
                {
                    float trigger = 0.01f * _intensity;
                    if (_rng.NextDouble() < trigger)
                    {
                        _clusterSamples = _rng.Next(6, 42); // slightly wider range
                        _clusterEnergy = (float)(_rng.NextDouble() * 0.75 + 0.25);
                    }
                }

                // Inside cluster: micro-cracks
                if (_clusterSamples > 0)
                {
                    float rnd = (float)(_rng.NextDouble() * 2.0 - 1.0);
                    burst = rnd * _clusterEnergy;

                    // Physical decay of stress energy
                    _clusterEnergy *= 0.915f;
                    _clusterSamples--;
                }
                else
                {
                    // Occasional isolated cracks
                    if (_rng.NextDouble() < 0.0038f * _intensity)
                        burst = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.38f;
                }

                // Micro-resonance (stone ringing)
                _resonance += 0.24f * (burst - _resonance);
                float ring = (float)Math.Sin(_resonance * 17.5f) * 0.24f;

                // Combine burst + resonance
                float crack = burst * 0.76f + ring * 0.24f;

                // Material damping
                _last += 0.21f * (crack - _last);

                // Nonlinear hardness shaping
                float shaped = _last * (0.86f + 0.14f * _last);

                // Stone impact saturation
                float saturated = (float)Math.Tanh(shaped * 3.1f);

                // Mix with original
                pcm[i] = x + saturated * (0.55f * _intensity);
            }
        }
    }
}