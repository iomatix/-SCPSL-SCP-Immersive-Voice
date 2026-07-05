namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    ///  Formant Shifter simulating vocal tract cavity scaling.
    /// Uses a cascaded 4-band Biquad Resonator Matrix to shift spectral envelopes 
    /// without altering the fundamental pitch. Zero-allocation and real-time safe.
    /// </summary>
    public class FormantShiftEffect : IAudioEffect
    {
        public string Name => "Formant Shift";

        // 4-band formant frequencies for standard vocal tract representation (Hz)
        private const float F1 = 500f;  // Throat resonance
        private const float F2 = 1500f; // Mouth cavity resonance
        private const float F3 = 2500f; // Nasal / Palate resonance
        private const float F4 = 3500f; // Vocal presence resonance

        private BiquadFilter _filter1;
        private BiquadFilter _filter2;
        private BiquadFilter _filter3;
        private BiquadFilter _filter4;

        /// <summary>
        /// Initializes the Formant Shifter.
        /// </summary>
        /// <param name="formant">Formant scale ratio (0.5 = massive giant, 2.0 = tiny goblin).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public FormantShiftEffect(float formant, float sampleRate)
        {
            float scale = Clamp(formant, 0.5f, 2.0f);
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            // Scale the formant centers to simulate expanding or shrinking the physical throat
            // Q values and Gains are tailored to emulate distinct biological cavities
            _filter1.ConfigurePeakingEQ(F1 * scale, sr, q: 1.5f, gainDb: 12f);
            _filter2.ConfigurePeakingEQ(F2 * scale, sr, q: 2.0f, gainDb: 10f);
            _filter3.ConfigurePeakingEQ(F3 * scale, sr, q: 2.5f, gainDb: 8f);
            _filter4.ConfigurePeakingEQ(F4 * scale, sr, q: 3.0f, gainDb: 6f);
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1) return;

            for (int i = 0; i < length; i++)
            {
                float input = pcm[i];

                // Process sequentially through the vocal tract matrix
                float output = _filter1.Process(input);
                output = _filter2.Process(output);
                output = _filter3.Process(output);
                output = _filter4.Process(output);

                // Blend dry and wet carefully to keep text intelligibility high
                pcm[i] = input * 0.4f + output * 0.6f;
            }
        }

        // High-performance, stack-allocated 2nd order IIR filter structure
        private struct BiquadFilter
        {
            private float _b0, _b1, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public void ConfigurePeakingEQ(float frequency, float sampleRate, float q, float gainDb)
            {
                // Ensure the scaled frequency does not breach Nyquist limit
                frequency = Math.Min(frequency, sampleRate * 0.45f);

                float a = (float)Math.Pow(10, gainDb / 40.0);
                float w0 = 2f * (float)Math.PI * frequency / sampleRate;
                float alpha = (float)Math.Sin(w0) / (2f * q);
                float cosW0 = (float)Math.Cos(w0);

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
    }
}