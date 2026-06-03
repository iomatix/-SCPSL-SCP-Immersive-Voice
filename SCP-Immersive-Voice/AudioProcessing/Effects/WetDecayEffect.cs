namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Generates organic, wet, decaying textures using a reactive envelope,
    /// modulated low‑pass filtering, and randomized wet pops. Designed for
    /// SCP‑106, SCP‑939, SCP‑049‑2, SCP‑610, or any creature requiring moist,
    /// unstable, fleshy vocal artifacts. Includes smoothing and saturation
    /// for a fully organic AAA‑quality horror sound.
    /// </summary>
    public class WetDecayEffect : IAudioEffectShort
    {
        private readonly float _amount;

        // Low‑pass filter state (wet smear)
        private float _lpState;

        // Envelope follower (reacts to input loudness)
        private float _env;

        // Modulation phase for wobbling cutoff
        private float _phase;

        // Random generator for wet pops
        private static readonly Random _rng = new Random();

        public WetDecayEffect(float amount)
        {
            // amount 0 → dry
            // amount 1.5 → extremely wet, fleshy, unstable
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1
                float x = pcm[i] / 32768f;

                // ---------------------------------------------------------
                // 1. Envelope follower (detects how loud the voice is)
                //    Faster attack, slower release → organic response
                // ---------------------------------------------------------
                float abs = Math.Abs(x);
                float targetEnv = abs;
                _env += (targetEnv - _env) * (abs > _env ? 0.04f : 0.015f);

                // ---------------------------------------------------------
                // 2. Modulated low‑pass filter (wet smear)
                //    Wobbling cutoff simulates unstable, moist tissue
                // ---------------------------------------------------------
                _phase += 0.0013f + _env * 0.0004f; // louder → more wobble
                float wobble = 0.5f + 0.5f * (float)Math.Sin(_phase + _env * 2.7f);

                // Cutoff range: 0.12–0.78 (very wet, very organic)
                float cutoff = 0.12f + 0.66f * wobble;

                // One‑pole low‑pass
                _lpState += cutoff * (x - _lpState);
                float wetBase = _lpState;

                // ---------------------------------------------------------
                // 3. Random wet pops (organic bubbles, flesh pops)
                // ---------------------------------------------------------
                float popChance = 0.0025f + _env * 0.018f * _amount;
                float pop = 0f;

                if (_rng.NextDouble() < popChance)
                {
                    // Raw pop with slight amplitude randomness
                    float raw = (float)(_rng.NextDouble() * 2.0 - 1.0);

                    // Saturated pop for organic character
                    float popStrength = 0.28f + (float)_rng.NextDouble() * 0.12f;
                    pop = (float)Math.Tanh(raw * 3.0f) * popStrength * _amount;
                }

                float wet = wetBase + pop;

                // ---------------------------------------------------------
                // 4. Soft saturation for fleshy, moist character
                // ---------------------------------------------------------
                float saturated = (float)Math.Tanh(wet * 1.7f);

                // ---------------------------------------------------------
                // 5. Mix dry + wet
                // ---------------------------------------------------------
                float mixed = x * (1f - _amount * 0.52f) + saturated * (_amount * 0.52f);

                // Convert back to PCM
                int sample = (int)(mixed * 32767f);

                // Clamp
                if (sample > short.MaxValue) sample = short.MaxValue;
                if (sample < short.MinValue) sample = short.MinValue;

                pcm[i] = (short)sample;
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