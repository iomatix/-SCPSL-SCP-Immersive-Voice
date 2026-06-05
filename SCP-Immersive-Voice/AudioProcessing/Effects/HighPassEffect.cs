namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    ///  Stateful 2nd-order Butterworth High-Pass filter (12 dB/octave).
    /// Safely strips out low-end mud, rumble or proximity bass spikes. 
    /// Ideal for radio intercom transmission style or small loudspeaker emulation.
    /// </summary>
    public class HighPassEffect : IAudioEffect
    {
        public string Name => "High Pass";

        // Pre-calculated filter coefficients
        private readonly float _b0, _b1, _b2, _a1, _a2;

        // Stateful history registers (Direct Form I)
        private float _x1, _x2, _y1, _y2;

        /// <summary>
        /// Initializes the High-Pass Filter.
        /// </summary>
        /// <param name="cutoffHz">Cutoff frequency in Hertz.</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public HighPassEffect(float cutoffHz, float sampleRate)
        {
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            // Critical Nyquist safety guard
            float clampedCutoff = Clamp(cutoffHz, 20f, sr * 0.45f);

            // Standard Butterworth design criteria (Q = 1 / sqrt(2))
            float q = 0.7071068f;
            float w0 = 2f * (float)Math.PI * clampedCutoff / sr;
            float alpha = (float)Math.Sin(w0) / (2f * q);
            float cosW0 = (float)Math.Cos(w0);

            float a0 = 1f + alpha;
            _b0 = ((1f + cosW0) / 2f) / a0;
            _b1 = -(1f + cosW0) / a0;
            _b2 = ((1f + cosW0) / 2f) / a0;
            _a1 = (-2f * cosW0) / a0;
            _a2 = (1f - alpha) / a0;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1) return;

            for (int i = 0; i < length; i++)
            {
                float x0 = pcm[i];

                // Standard Biquad Direct Form I difference equation
                float y0 = _b0 * x0 + _b1 * _x1 + _b2 * _x2 - _a1 * _y1 - _a2 * _y2;

                // Shift time registers
                _x2 = _x1;
                _x1 = x0;
                _y2 = _y1;
                _y1 = y0;

                pcm[i] = y0;
            }
        }
    }
}