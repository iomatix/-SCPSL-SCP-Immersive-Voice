namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Produces a soft, slimy, organic wet layer using slow filter modulation,
    /// subtle high‑pass movement, envelope‑reactive shaping, and gentle noise.
    /// Ideal for SCP‑3114, SCP‑939, or any creature requiring moist, living,
    /// tissue‑like vocal textures. Designed to be subtle, smooth, and non‑bulky,
    /// unlike WetDecay which is heavier and more grotesque.
    /// </summary>
    public class WetOrganicEffect : IAudioEffectShort
    {
        private readonly float _amount;

        // Filter states
        private float _lp;
        private float _hp;

        // Envelope follower (reacts to input loudness)
        private float _env;

        // Modulation phase
        private float _phase;

        // Random generator
        private static readonly Random _rng = new Random();

        public WetOrganicEffect(float amount)
        {
            // amount 0 → dry
            // amount 1.5 → strong slimy organic layer
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1
                float x = pcm[i] / 32768f;

                // ---------------------------------------------------------
                // 1. Envelope follower (organic reactivity)
                // ---------------------------------------------------------
                float abs = Math.Abs(x);
                _env += 0.03f * (abs - _env);

                // ---------------------------------------------------------
                // 2. Slow filter modulation (slimy wobble)
                // ---------------------------------------------------------
                _phase += 0.0011f + _env * 0.0003f;
                float wobble = 0.5f + 0.5f * (float)Math.Sin(_phase * 1.35f);

                // Dynamic cutoffs
                float lpCut = 0.18f + wobble * 0.32f;  // 0.18–0.50
                float hpCut = 0.78f - wobble * 0.22f;  // 0.56–0.78

                // ---------------------------------------------------------
                // 3. Low‑pass (soft smear)
                // ---------------------------------------------------------
                _lp += lpCut * (x - _lp);

                // ---------------------------------------------------------
                // 4. High‑pass (slimy movement)
                // ---------------------------------------------------------
                _hp = x - _lp * hpCut;

                // ---------------------------------------------------------
                // 5. Subtle organic noise (wet friction)
                // ---------------------------------------------------------
                float noise = 0f;
                if (_amount > 0.05f)
                {
                    float raw = (float)(_rng.NextDouble() * 2.0 - 1.0);
                    noise = raw * 0.035f * _amount;
                }

                // ---------------------------------------------------------
                // 6. Soft saturation for organic smoothness
                // ---------------------------------------------------------
                float wet = (float)Math.Tanh((_hp + noise) * 1.4f);

                // ---------------------------------------------------------
                // 7. Mix dry + wet
                // ---------------------------------------------------------
                float mixed = x * (1f - _amount * 0.42f) + wet * (_amount * 0.42f);

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