namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Organic chirp bursts with FM wobble, jitter, fade-out shaping and
    /// nonlinear saturation. Ideal for SCP-079 interference, dimensional
    /// chirps, corrupted comms or creepy creature/flamingo vocalizations.
    /// </summary>
    public class ChirpEffect : IAudioEffect
    {
        public string Name => "Chirp";

        private readonly float _amount;

        private float _phase;
        private float _env;
        private float _freqMod;

        private readonly Random _rng;

        public ChirpEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1f);
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Random chance to trigger chirp
                if (_rng.NextDouble() < _amount * 0.00085f)
                {
                    _phase = 0f;
                    _env = 1f;

                    // Random FM wobble (creature-like)
                    _freqMod = 0.9f + (float)_rng.NextDouble() * 0.4f;
                }

                if (_env > 0.001f)
                {
                    // FM wobble + jitter (creepy flamingo gdakanie)
                    float jitter = ((float)_rng.NextDouble() * 2f - 1f) * 0.03f;
                    float freq = 18f * _freqMod + jitter * 12f;

                    // Base chirp tone
                    float tone = (float)Math.Sin(_phase * freq);

                    // Anti-alias tilt
                    tone *= 0.82f + 0.18f * (float)Math.Sin(_phase * 0.45f);

                    // Envelope shaping (smooth fade-out)
                    float shapedEnv = _env * _env; // quadratic fade
                    tone *= shapedEnv * _amount * 0.32f;

                    // Mix
                    float mixed = dry + tone;

                    // Soft clip
                    mixed = (float)Math.Tanh(mixed * 1.05f);

                    pcm[i] = mixed;

                    // Advance phase
                    _phase += 0.16f;

                    // Organic exponential decay
                    _env *= 0.962f;
                }
            }
        }
    }
}