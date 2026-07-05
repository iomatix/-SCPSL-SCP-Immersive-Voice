namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    ///  Digital clock-divider sample rate reducer (downsampler).
    /// Employs a persistent frame-independent phase accumulator, a high-speed LCG clock jitter 
    /// emulator, and a low-quality reconstruction DAC filter to generate authentic aliasing.
    /// </summary>
    public class SampleRateReducerEffect : IAudioEffect
    {
        public string Name => "Sample Rate Reducer";

        private readonly float _amount;
        private readonly float _sampleRate;

        // Persistent sample-and-hold states across frame boundaries
        private float _phaseAccumulator = 0f;
        private float _heldSample = 0f;
        private float _reconstructionState = 0f;

        // Phase tracking for the parabolic low-frequency clock jitter LFO
        private float _jitterLfoPhase = 0f;
        private readonly float _jitterLfoIncrement;

        // Local thread-safe LCG random state
        private uint _lcgState;

        /// <summary>
        /// Initializes the Sample Rate Reducer effect.
        /// </summary>
        /// <param name="amount">Intensity of downsampling (0.0f = clear, 1.0f = severe 800Hz aliasing).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public SampleRateReducerEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Slow clock thermal drift LFO speed (~1.2 Hz)
            _jitterLfoIncrement = 1.2f / _sampleRate;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            // Map linear amount to an exponential frequency downsampling divisor
            // At amount=0: factor=1.0 (no reduction). At amount=1: factor=~0.016 (down to 800Hz)
            float targetFrequencyFactor = (float)Math.Pow(2f, -_amount * 6.0f);

            // Configure lossy reconstruction DAC filter coefficients (~2200Hz smoothing window)
            float rcOmega = 2f * (float)Math.PI * 2200f / _sampleRate;
            float reconstructionDampCoef = rcOmega / (rcOmega + 1f);

            for (int i = 0; i < length; i++)
            {
                float dryInput = pcm[i];

                // 1. Advance thermal clock drift LFO via fast polynomial triangle-to-parabola approximation
                _jitterLfoPhase += _jitterLfoIncrement;
                if (_jitterLfoPhase > 1f) _jitterLfoPhase -= 1f;

                float tri = _jitterLfoPhase * 2f;
                if (tri > 1f) tri = 2f - tri;
                float clockDrift = 4f * tri * (1f - tri);

                // 2. Compute ultra-fast LCG high-frequency phase jitter
                _lcgState = _lcgState * 1103515245 + 12345;
                float phaseJitter = ((float)(_lcgState & 0xFFFF) / 65535f) * 0.12f * _amount;

                // 3. Modulate step clock rate based on jitter matrix
                float currentStepRate = targetFrequencyFactor * (1f - (clockDrift * 0.08f * _amount));

                // 4. Persistent Phase Accumulator step
                _phaseAccumulator += currentStepRate + phaseJitter;

                // When phase accumulator cross the unit threshold, open the digital gate to latch a new sample
                if (_phaseAccumulator >= 1f || _heldSample == 0f)
                {
                    _phaseAccumulator -= (float)Math.Floor(_phaseAccumulator); // Retain fractional phase
                    _heldSample = dryInput;
                }

                // 5. Simulate bad historical DAC reconstruction (stair-case smoothing smear)
                // Blends pristine digital aliasing stair-steps with loose analog cable capacitance
                _reconstructionState = _reconstructionState + reconstructionDampCoef * (_heldSample - _reconstructionState);

                // 6. Polynomial soft-clipping saturation to emulate vintage operational amplifier warmth
                float driven = _reconstructionState * 1.1f;
                float saturatedOut = driven / (1f + Math.Abs(driven));

                // 7. Full insert injection into the target PCM buffer
                pcm[i] = saturatedOut;
            }
        }
    }
}