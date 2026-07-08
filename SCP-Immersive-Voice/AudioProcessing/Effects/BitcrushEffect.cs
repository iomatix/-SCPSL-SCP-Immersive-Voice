using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// High-performance Cybernetic Bitcrusher.
    /// Bypasses analog-style dithering smoothers to enforce raw, cold, non-linear mid-tread 
    /// digital step truncation, producing authentic crystalline digital quantization artifacts.
    /// </summary>
    public class BitcrushEffect : IAdjustableAudioEffect
    {
        #region Private Execution Vectors
        private float _amount;
        private readonly float _steps;
        private readonly float _invSteps;

        // Stateful parameters for the 1st-order DC recovery barrier
        private float _dcX1;
        private float _dcY1;
        #endregion

        #region Public Metadata Properties
        public string Name => "Bitcrush";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the cybernetic Bitcrush effect.
        /// </summary>
        /// <param name="amount">The quantization depth factor (0.0f to 1.0f).</param>
        public BitcrushEffect(float amount)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the argument payload
            _amount = amount.Clamp(0f, 1f);

            // Map linear amount exponentially to real bit depth (16 bits down to a brutal 2.5 bits)
            float bits = 16f - (_amount * 13.5f);

            _steps = (float)Math.Pow(2f, bits - 1f);
            _invSteps = 1f / _steps;

            _dcX1 = 0f;
            _dcY1 = 0f;
        }
        #endregion

        #region High-Frequency DSP Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1) return;

            for (int i = 0; i < length; i++)
            {
                float sample = pcm[i];

                // Hard-boundary ceiling truncation using our fluent extensions pattern
                sample = sample.Clamp(-1f, 1f);

                // High-performance Mid-Tread Quantization Loop
                // Swapped heavy Math.Floor double transformations for our fluent float-native execution bridge
                float quantized = (sample * _steps + 0.5f).Floor() * _invSteps;

                // Stateful 1st-order Recursive DC Blocker Filter to protect downstream buffers from offset drifts
                float dcFiltered = quantized - _dcX1 + 0.995f * _dcY1;
                _dcX1 = quantized;
                _dcY1 = dcFiltered;

                // Rational Soft-Clipping to compress digital spikes without adding analog tubes warmth
                float driven = dcFiltered * 1.10f;
                float saturated = driven / (1f + driven.Abs());

                pcm[i] = saturated;
            }
        }
        #endregion

        #region Operational Parameter Adjustments
        public void AdjustParameter(float value)
        {
            _amount = value.Clamp(0f, 1f);
        }
        #endregion
    }
}