namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA High-performance Cybernetic Bitcrusher.
    /// Bypasses analog-style dithering smoothers to enforce raw, cold, non-linear mid-tread 
    /// digital step truncation, producing authentic crystalline digital quantization artifacts.
    /// </summary>
    public class BitcrushEffect : IAudioEffect
    {
        public string Name => "Bitcrush";

        private readonly float _steps;
        private readonly float _invSteps;
        private readonly float _amount;

        private float _dcX1 = 0f;
        private float _dcY1 = 0f;

        /// <summary>
        /// Initializes the cybernetic Bitcrush effect.
        /// </summary>
        /// <param name="amount">The quantization depth factor (0.0f to 1.0f).</param>
        public BitcrushEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1f);

            // Map linear amount exponentially to real bit depth (16 bits down to a brutal 2.5 bits)
            float bits = 16f - (_amount * 13.5f);
            _steps = (float)Math.Pow(2f, bits - 1f);
            _invSteps = 1f / _steps;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1) return;

            for (int i = 0; i < length; i++)
            {
                float v = pcm[i];

                // AAA FIX: Removed TPDF Dither completely. 
                // Leaving the quantization un-dithered forces harsh, sterile, pixelated digital step boundaries 
                // native to corrupted computer mainframes and cold synthetic AI processors.
                if (v > 1f) v = 1f;
                else if (v < -1f) v = -1f;

                // High-performance Mid-Tread Quantization Loop
                float quantized = (float)Math.Floor(v * _steps + 0.5f) * _invSteps;

                // Stateful 1st-order Recursive DC Blocker Filter to protect downstream buffers from offset drifts
                float dcFiltered = quantized - _dcX1 + 0.995f * _dcY1;
                _dcX1 = quantized;
                _dcY1 = dcFiltered;

                // Rational Soft-Clipping to compress digital spikes without adding analog tubes warmth
                float driven = dcFiltered * 1.10f;
                float saturated = driven / (1f + Math.Abs(driven));

                pcm[i] = saturated;
            }
        }
    }
}