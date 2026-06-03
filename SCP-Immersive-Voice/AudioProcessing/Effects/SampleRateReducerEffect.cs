namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Reduces perceived sample rate using sample holding, smoothing, and
    /// micro‑modulation. Produces organic lo‑fi degradation ideal for SCP‑079,
    /// corrupted audio, or retro digital textures.
    /// </summary>
    public class SampleRateReducerEffect : IAudioEffectShort
    {
        private readonly float _amount;

        // Last held sample (float for stability)
        private float _hold;

        // Smoothing state to avoid harsh edges
        private float _smooth;

        // Random for micro‑modulation
        private static readonly Random _rng = new Random();

        public SampleRateReducerEffect(float amount)
        {
            // amount 0 → full quality
            // amount 1 → heavy reduction
            _amount = Clamp(amount, 0f, 1f);
        }

        public void Process(short[] pcm, int length)
        {
            // Base skip factor (1–12 samples)
            int baseSkip = 1 + (int)(_amount * 12f);

            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1
                float x = pcm[i] / 32768f;

                // 1. Micro‑modulation to avoid robotic artifacts
                int skip = baseSkip;

                if (_amount > 0.2f)
                {
                    // ±1 sample jitter for organic lo‑fi
                    skip += _rng.Next(-1, 2);
                    if (skip < 1) skip = 1;
                }

                // 2. Hold logic
                if (i % skip == 0)
                    _hold = x;

                // 3. Smooth transitions to avoid harsh steps
                _smooth += 0.2f * (_hold - _smooth);

                // 4. Soft saturation for analog‑style lo‑fi
                float outSample = (float)Math.Tanh(_smooth * 1.4f);

                // Convert back to PCM
                int sample = (int)(outSample * 32767f);

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
