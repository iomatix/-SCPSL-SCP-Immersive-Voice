namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    ///  Non-Euclidean Extra-dimensional Echo Matrix tailored for SCP-106.
    /// Employs a sample-rate independent power-of-two ring buffer, fractional linear interpolation,
    /// fast polynomial LFOs and an internal feedback All-Pass phase-smear filter. Zero allocations.
    /// </summary>
    public class PocketDimensionEchoEffect : IAudioEffect
    {
        public string Name => "Pocket Dimension Echo";

        private readonly float _amount;
        private readonly float _sampleRate;

        // Persistent power-of-two delay line buffer
        private readonly float[] _buffer;
        private readonly int _bufferMask;
        private int _writeIndex;

        // Phase tracking for fast polynomial LFO modulators
        private float _lfoPhaseTime = 0f;
        private float _lfoPhaseFb = 0f;
        private readonly float _lfoIncTime;
        private readonly float _lfoIncFb;

        // Stateful history register for the internal All-Pass phase shifter
        private float _allPassState = 0f;

        /// <summary>
        /// Initializes the Pocket Dimension Echo effect.
        /// </summary>
        /// <param name="amount">Intensity and density of the dimensional echo (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public PocketDimensionEchoEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // Maximum required delay is around 450ms. 
            // Allocate a stable power-of-two buffer (32768 samples @ 48kHz = ~682ms)
            int size = 32768;
            _buffer = new float[size];
            _bufferMask = size - 1;
            _writeIndex = 0;

            // Sample-rate independent LFO speeds (Time modulator = 0.35 Hz, Feedback modulator = 0.12 Hz)
            _lfoIncTime = 0.35f / _sampleRate;
            _lfoIncFb = 0.12f / _sampleRate;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            // Define physical boundary properties for the extradimensional space
            float baseDelaySamples = _sampleRate * 0.28f; // 280ms baseline room echo length
            float maxModulationSamples = _sampleRate * 0.14f; // 140ms geometry warp depth

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Advance linear phase accumulators
                _lfoPhaseTime += _lfoIncTime;
                if (_lfoPhaseTime > 1f) _lfoPhaseTime -= 1f;

                _lfoPhaseFb += _lfoIncFb;
                if (_lfoPhaseFb > 1f) _lfoPhaseFb -= 1f;

                // 2. High-speed polynomial Triangle-to-Parabola LFO approximation (Replaces costly Math.Sin)
                float tTri = _lfoPhaseTime * 2f;
                if (tTri > 1f) tTri = 2f - tTri;
                float timeModulator = 4f * tTri * (1f - tTri);

                float fbTri = _lfoPhaseFb * 2f;
                if (fbTri > 1f) fbTri = 2f - fbTri;
                float feedbackModulator = 4f * fbTri * (1f - fbTri);

                // 3. Compute continuous fractional delay time to dynamically warp space geometry
                float targetDelay = baseDelaySamples + (timeModulator * maxModulationSamples);

                // 4. Extract sample using fractional linear interpolation to eliminate digital zipper artifacts
                float readPos = _writeIndex - targetDelay;
                while (readPos < 0f) readPos += _buffer.Length;

                int i0 = (int)readPos;
                int i1 = (i0 + 1) & _bufferMask;
                float frac = readPos - i0;
                float delayedSample = _buffer[i0 & _bufferMask] * (1f - frac) + _buffer[i1] * frac;

                // 5. Dynamic feedback calculation with strict stability protection cap
                float currentFeedback = 0.32f + (feedbackModulator * 0.26f);
                if (currentFeedback > 0.62f) currentFeedback = 0.62f;

                // 6. Inline All-Pass filter inside the feedback loop to create non-Euclidean phase smearing
                float apInput = delayedSample;
                float apOutput = -0.65f * apInput + _allPassState;
                _allPassState = apInput + (0.65f * apOutput);

                // 7. Fast polynomial soft-clipping to compress rogue resonances safely in the feedback path
                float feedbackDrive = drySample + (apOutput * currentFeedback);
                float saturatedFeedback = feedbackDrive / (1f + Math.Abs(feedbackDrive));

                // 8. Commit the processed node back into the power-of-two ring buffer
                _buffer[_writeIndex] = saturatedFeedback;
                _writeIndex = (_writeIndex + 1) & _bufferMask;

                // 9. Equal-power wet/dry hybrid crossfade to maintain verbal clarity
                float wetMix = _amount * 0.55f;
                if (wetMix > 0.75f) wetMix = 0.75f; // Security threshold for voice retention

                pcm[i] = (drySample * (1f - wetMix)) + (apOutput * wetMix);
            }
        }
    }
}