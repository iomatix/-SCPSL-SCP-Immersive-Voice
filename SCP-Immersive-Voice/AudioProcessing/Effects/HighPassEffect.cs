using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Stateful 2nd-order Butterworth High-Pass filter (12 dB/octave).
    /// Safely strips out low-end mud, rumble, or proximity bass spikes. 
    /// Ideal for radio intercom transmission style or small loudspeaker emulation.
    /// </summary>
    public class HighPassEffect : IAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Filter Coefficients
        private readonly float _b0, _b1, _b2, _a1, _a2;
        #endregion

        #region Private Stateful History Registers
        // Managed securely via stack allocation cache routing loops
        private float _x1, _x2, _y1, _y2;
        #endregion

        #region Public Metadata Properties
        public string Name => "High Pass";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the High-Pass Filter.
        /// </summary>
        /// <param name="cutoffHz">Cutoff frequency in Hertz.</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public HighPassEffect(float cutoffHz, float sampleRate)
        {
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            // Critical Nyquist safety guard using our fluent clamp extensions
            float clampedCutoff = cutoffHz.Clamp(20f, sr * 0.45f);

            // Standard Butterworth design criteria (Q = 1 / sqrt(2))
            const float q = 0.7071068f;

            // PERFORMANCE FIX: Shifted trigonometry generation directly into float-native hardware contexts
            float w0 = TwoPi * clampedCutoff / sr;
            float alpha = Mathf.Sin(w0) / (2f * q);
            float cosW0 = Mathf.Cos(w0);

            float a0 = 1f + alpha;
            _b0 = ((1f + cosW0) / 2f) / a0;
            _b1 = -(1f + cosW0) / a0;
            _b2 = ((1f + cosW0) / 2f) / a0;
            _a1 = (-2f * cosW0) / a0;
            _a2 = (1f - alpha) / a0;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1) return;

            // Caching biquad delay history registers directly onto the CPU execution stack frame.
            // Bypasses persistent RAM allocation line tracking completely across the sample collection array loop.
            float localX1 = _x1;
            float localX2 = _x2;
            float localY1 = _y1;
            float localY2 = _y2;

            float cB0 = _b0, cB1 = _b1, cB2 = _b2;
            float cA1 = _a1, cA2 = _a2;

            for (int i = 0; i < length; i++)
            {
                float x0 = pcm[i];

                // Standard Biquad Direct Form I difference equation executed via local cache values
                float y0 = cB0 * x0 + cB1 * localX1 + cB2 * localX2 - cA1 * localY1 - cA2 * localY2;

                // Shift time registers inside high-speed local processing contexts
                localX2 = localX1;
                localX1 = x0;
                localY2 = localY1;
                localY1 = y0;

                pcm[i] = y0;
            }

            // Write computed local historical states back into class persistence boundaries atomically.
            _x1 = localX1;
            _x2 = localX2;
            _y1 = localY1;
            _y2 = localY2;
        }
        #endregion
    }
}