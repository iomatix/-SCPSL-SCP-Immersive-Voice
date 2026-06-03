namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Reduces bit depth to create gritty, digital degradation. Includes dithering
    /// for smoother quantization and reduced harsh artifacts. Ideal for SCP‑079,
    /// corrupted radio, or retro digital distortion.
    /// </summary>

    public class BitcrushEffect : IAudioEffectShort
    {
        private readonly int _steps;
        private static readonly Random _rng = new Random();

        public BitcrushEffect(float amount)
        {
            // amount 0 → full quality
            // amount 1 → lowest bit amount
            amount = Clamp(amount, 0f, 1f);

            // 256 levels = 8-bit
            // 32 levels = 5-bit
            _steps = (int)(256 * (1f - amount));
            if (_steps < 2) _steps = 2;
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert to int for processing
                int sample = pcm[i];

                // Add small TPDF dither to reduce quantization artifacts
                // Range is very small compared to full PCM scale
                float dither = (float)(_rng.NextDouble() - _rng.NextDouble()) * (_steps * 0.25f);
                float withDither = sample + dither;

                // Quantization
                int crushed = (int)Math.Round(withDither / _steps) * _steps;

                // Clamp to valid PCM range
                if (crushed > short.MaxValue) crushed = short.MaxValue;
                if (crushed < short.MinValue) crushed = short.MinValue;

                pcm[i] = (short)crushed;
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
