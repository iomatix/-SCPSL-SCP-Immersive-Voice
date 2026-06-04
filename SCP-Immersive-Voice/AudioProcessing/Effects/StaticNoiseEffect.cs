namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Organic multi-layer static for SCP-079, radio corruption or dimensional interference.
    /// Combines white noise, LF drift, HF fizz and nonlinear shaping.
    /// </summary>
    public class StaticNoiseEffect : IAudioEffect
    {
        public string Name => "Static Noise";

        private readonly float _amount;

        private float _smooth;
        private float _driftPhase;
        private float _fizzPhase;

        // Per-instance RNG to avoid shared-state artifacts
        private readonly Random _rng;

        public StaticNoiseEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1f);
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float x = pcm[i];

                // White noise layer
                float white = (float)(_rng.NextDouble() * 2.0 - 1.0);

                // Low-frequency drift (dimensional instability)
                _driftPhase += 0.0014f + _amount * 0.0011f;
                float drift = (float)Math.Sin(_driftPhase * 0.65f) * 0.42f;

                // High-frequency fizz (spectral crackle)
                _fizzPhase += 0.0035f + _amount * 0.0022f;
                float fizz = (float)Math.Sin(_fizzPhase * 17.3f);
                fizz = fizz * fizz * 0.22f; // HF bias

                // Layer blend
                float combined =
                    white * (0.55f + _amount * 0.25f) +
                    drift * 0.25f +
                    fizz * 0.20f;

                // Organic smoothing
                _smooth += 0.11f * (combined - _smooth);

                // Nonlinear shaping
                float shaped = _smooth * (0.86f + 0.14f * _smooth);

                // Soft saturation
                float staticOut = (float)Math.Tanh(shaped * 2.1f);

                // Wet/dry mix
                pcm[i] = x * (1f - _amount) + staticOut * _amount;
            }
        }
    }
}