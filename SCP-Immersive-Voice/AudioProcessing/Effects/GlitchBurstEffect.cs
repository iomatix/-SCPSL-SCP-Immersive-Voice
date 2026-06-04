namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Injects short glitch bursts with digital fracture, foldback distortion,
    /// bit-breakup and nonlinear shaping. Ideal for SCP-079 corruption or
    /// dimensional instability. Float-native and stable.
    /// </summary>
    public class GlitchBurstEffect : IAudioEffect
    {

        public string Name => "Glitch Burst";

        private readonly float _amount;

        private int _burstSamplesLeft;
        private float _burstPhase;

        // Per-instance RNG for isolated glitch behavior
        private readonly Random _rng;

        public GlitchBurstEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Trigger new burst
                if (_burstSamplesLeft <= 0)
                {
                    float trigger = _amount * 0.002f;
                    if (_rng.NextDouble() < trigger)
                    {
                        _burstSamplesLeft = _rng.Next(6, 26);
                        _burstPhase = 0f;
                    }
                }

                if (_burstSamplesLeft > 0)
                {
                    // Base noise
                    float noise = (float)(_rng.NextDouble() * 2.0 - 1.0);

                    // Foldback distortion (digital fracture)
                    float folded = Math.Abs((noise * 3.1f) % 2f - 1f);

                    // Bit fracture (controlled breakup)
                    float fractured = (float)Math.Floor(folded * 8f) * 0.125f;

                    // Burst envelope
                    float env = 1f - (_burstPhase / _burstSamplesLeft);

                    // Combine glitch layers
                    float glitch =
                        noise * 0.30f +
                        folded * 0.50f +
                        fractured * 0.20f;

                    glitch *= env;

                    // Anti-alias shaping
                    glitch *= 0.86f + 0.14f * glitch;

                    // Mix with original
                    float mixed = dry + glitch * 0.58f;

                    // Soft clip
                    pcm[i] = (float)Math.Tanh(mixed * 1.32f);

                    _burstPhase++;
                    _burstSamplesLeft--;
                }
            }
        }
    }
}