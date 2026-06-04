namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA Whisper Filter converting voiced speech into an unvoiced whispered texture.
    /// Destroys harmonic pitch lines while preserving formant intelligibility using 
    /// phase-randomization sign inversion and high-frequency spectral shaping.
    /// </summary>
    public class WhisperFilterEffect : IAudioEffect
    {
        public string Name => "Whisper";

        private readonly float _amount;

        // Fast bitwise LCG state and spectral air shaper
        private uint _lcgState;
        private BiquadFilter _airShaper;

        /// <summary>
        /// Initializes the Whisper Filter effect.
        /// </summary>
        /// <param name="amount">Whisper intensity blend (0.0f = full dry, 1.0f = 100% unvoiced whisper).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public WhisperFilterEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1f);
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // High-pass filter at 1200Hz to remove low-end thuds and proximity mud,
            // since natural human whispers completely lack low vocal cord resonances.
            _airShaper.ConfigureHighPass(1200f, sr, q: 0.707f);
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Generate ultra-fast LCG random bits (1 CPU cycle cost)
                _lcgState = _lcgState * 1103515245 + 12345;

                // 2. Extract random bipolar sign (+1.0f or -1.0f) using bit mask
                float randomSign = ((_lcgState & 0x400) != 0) ? 1f : -1f;

                // 3. Perform Stochastic Sign Inversion (Whisperization)
                // This completely obliterates the fundamental harmonic lines (f0)
                // while preserving the raw time-domain envelope and formant peaks perfectly.
                float rawWhisper = Math.Abs(drySample) * randomSign;

                // 4. Filter the raw whisper to isolate the airy, turbulent friction band
                float airWhisper = _airShaper.Process(rawWhisper);

                // 5. Constrained blend between the original speech and the unvoiced whisper layer
                pcm[i] = (drySample * (1f - _amount)) + (airWhisper * _amount);
            }
        }

        // High-performance, stack-allocated 2nd order IIR filter structure
        private struct BiquadFilter
        {
            private float _b0, _b1, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public void ConfigureHighPass(float cutoffFrequency, float sampleRate, float q)
            {
                float w0 = 2f * (float)Math.PI * cutoffFrequency / sampleRate;
                float alpha = (float)Math.Sin(w0) / (2f * q);
                float cosW0 = (float)Math.Cos(w0);

                float a0 = 1f + alpha;
                _b0 = ((1f + cosW0) / 2f) / a0;
                _b1 = -(1f + cosW0) / a0;
                _b2 = ((1f + cosW0) / 2f) / a0;
                _a1 = (-2f * cosW0) / a0;
                _a2 = (1f - alpha) / a0;
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