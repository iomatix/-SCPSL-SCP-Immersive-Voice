using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Formant Shifter simulating vocal tract cavity scaling.
    /// Uses a cascaded 4-band Biquad Resonator Matrix to shift spectral envelopes 
    /// without altering the fundamental pitch. Zero-allocation and real-time safe.
    /// </summary>
    public class FormantShiftEffect : IAudioEffect
    {
        #region Private Constants
        private const float F1 = 500f;  // Throat resonance
        private const float F2 = 1500f; // Mouth cavity resonance
        private const float F3 = 2500f; // Nasal / Palate resonance
        private const float F4 = 3500f; // Vocal presence resonance
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        // Cascaded processing filter topologies
        private BiquadFilter _filter1;
        private BiquadFilter _filter2;
        private BiquadFilter _filter3;
        private BiquadFilter _filter4;
        #endregion

        #region Public Metadata Properties
        public string Name => "Formant Shift";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Formant Shifter.
        /// </summary>
        /// <param name="formant">Formant scale ratio (0.5 = massive giant, 2.0 = tiny goblin).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public FormantShiftEffect(float formant, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the argument payload
            float scale = formant.Clamp(0.5f, 2.0f);
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            // Scale the formant centers to simulate expanding or shrinking the physical throat
            // Q values and Gains are tailored to emulate distinct biological cavities safely
            _filter1.ConfigurePeakingEQ(F1 * scale, sr, 1.5f, 12f);
            _filter2.ConfigurePeakingEQ(F2 * scale, sr, 2.0f, 10f);
            _filter3.ConfigurePeakingEQ(F3 * scale, sr, 2.5f, 8f);
            _filter4.ConfigurePeakingEQ(F4 * scale, sr, 3.0f, 6f);
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1) return;

            // Unrolling and isolating the value-type biquad filter struct matrix into individual stack nodes.
            // Drops pointer reference dereferencing cost to absolute zero within the kaskadowym hot path loop.
            BiquadFilter f1 = _filter1;
            BiquadFilter f2 = _filter2;
            BiquadFilter f3 = _filter3;
            BiquadFilter f4 = _filter4;

            for (int i = 0; i < length; i++)
            {
                float input = pcm[i];

                // Process sequentially through the internal local vocal tract matrix registers
                float output = f1.Process(input);
                output = f2.Process(output);
                output = f3.Process(output);
                output = f4.Process(output);

                // Blend dry and wet carefully to keep text intelligibility high
                pcm[i] = input * 0.4f + output * 0.6f;
            }

            // Safely restore calculated stack struct layouts back into class persistent parameters.
            _filter1 = f1;
            _filter2 = f2;
            _filter3 = f3;
            _filter4 = f4;
        }
        #endregion

        #region Internal High-Performance Data Substructures
        /// <summary>
        /// High-performance, stack-allocated 2nd order IIR filter structure.
        /// </summary>
        private struct BiquadFilter
        {
            private float _b0, _b1, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public void ConfigurePeakingEQ(float frequency, float sampleRate, float q, float gainDb)
            {
                // Ensure the scaled frequency does not breach Nyquist limit constraints
                frequency = frequency.Clamp(10f, sampleRate * 0.45f);

                // PERFORMANCE FIX: Replaced double precision Math.Pow, Math.Sin, and Math.Cos with float-native alternatives
                float a = Mathf.Pow(10f, gainDb / 40f);
                float w0 = TwoPi * frequency / sampleRate;
                float alpha = Mathf.Sin(w0) / (2f * q);
                float cosW0 = Mathf.Cos(w0);

                float a0 = 1f + alpha / a;
                _b0 = (1f + alpha * a) / a0;
                _b1 = (-2f * cosW0) / a0;
                _b2 = (1f - alpha * a) / a0;
                _a1 = (-2f * cosW0) / a0;
                _a2 = (1f - alpha / a) / a0;
            }

            public float Process(float input)
            {
                float output = _b0 * input + _b1 * _x1 + _b2 * _x2 - _a1 * _y1 - _a2 * _y2;

                _x2 = _x1;
                _x1 = input;
                _y2 = _y1;
                _y1 = output;

                return output;
            }
        }
        #endregion
    }
}