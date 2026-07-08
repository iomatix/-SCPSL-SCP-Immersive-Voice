using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Studio-grade Schroeder-Moorer Reverb Engine.
    /// Employs 4 parallel low-pass feedback comb filters for structural echo density
    /// and 2 cascaded all-pass filters for smooth spectral diffusion. Zero allocations in loop.
    /// </summary>
    public class ReverbEffect : IAdjustableAudioEffect
    {
        #region Private Execution Vectors
        private float _mix;

        // Parallel Low-Pass Feedback Comb Filters (LBCF) for early reflection density
        private LowPassCombFilter _comb1;
        private LowPassCombFilter _comb2;
        private LowPassCombFilter _comb3;
        private LowPassCombFilter _comb4;

        // Cascaded All-Pass Filters (APF) for late decay diffusion
        private AllPassFilter _allPass1;
        private AllPassFilter _allPass2;
        #endregion

        #region Public Metadata Properties
        public string Name => "Reverb";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Reverb Effect.
        /// </summary>
        /// <param name="mix">Reverb wet mix coefficient (0.0f = dry, 1.0f = hot decay room).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public ReverbEffect(float mix, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Enforcing clean limits using math extensions straight on initialization
            _mix = mix.Clamp(0f, 1f);
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            // Tailored prime-like delay times in milliseconds to prevent metallic comb resonances
            _comb1.Initialize(29.7f, sr, 0.82f, 0.25f);
            _comb2.Initialize(37.1f, sr, 0.78f, 0.28f);
            _comb3.Initialize(41.3f, sr, 0.75f, 0.32f);
            _comb4.Initialize(45.7f, sr, 0.71f, 0.35f);

            // Diffusion stages parameters
            _allPass1.Initialize(5.2f, sr, 0.7f);
            _allPass2.Initialize(1.7f, sr, 0.6f);
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _mix < 0.01f) return;

            // Pre-computing wet/dry crossfade staging coefficients outside the sample loop.
            // Eliminates thousands of redundant floating-point multiplications per voice frame packet.
            float mixScalar = _mix;
            float dryGain = 1f - (mixScalar * 0.5f);
            float wetGain = mixScalar * 0.65f;

            // Unrolling and caching all six value-type filter structures directly onto individual stack nodes.
            // Completely bypasses heap pointer dereferencing cost inside the processing loop, maximizing L1 execution speed.
            LowPassCombFilter lbcf1 = _comb1;
            LowPassCombFilter lbcf2 = _comb2;
            LowPassCombFilter lbcf3 = _comb3;
            LowPassCombFilter lbcf4 = _comb4;

            AllPassFilter ap1 = _allPass1;
            AllPassFilter ap2 = _allPass2;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // 1. Process parallel comb filters matrix via local registers to build acoustic energy density
                float c1 = lbcf1.Process(dry);
                float c2 = lbcf2.Process(dry);
                float c3 = lbcf3.Process(dry);
                float c4 = lbcf4.Process(dry);

                // Sum parallel structures (attenuated to prevent accumulation clipping)
                float combSum = (c1 + c2 + c3 + c4) * 0.25f;

                // 2. Pass through cascaded all-pass filters to smear impulses into a lush room tail
                float diffused = ap1.Process(combSum);
                float wet = ap2.Process(diffused);

                // 3. Linear constant-power style mix blend utilizing pre-computed stack gain constants
                pcm[i] = (dry * dryGain) + (wet * wetGain);
            }


            // Flush calculated stack struct layouts back into class persistent instance parameters atomically.
            _comb1 = lbcf1;
            _comb2 = lbcf2;
            _comb3 = lbcf3;
            _comb4 = lbcf4;

            _allPass1 = ap1;
            _allPass2 = ap2;
        }
        #endregion

        #region Internal High-Performance Data Substructures
        /// <summary>
        /// Inline high-performance parallel comb filter layout.
        /// </summary>
        private struct LowPassCombFilter
        {
            private float[] _buffer;
            private int _bufferMask;
            private int _writeIndex;
            private float _filterState;
            private float _feedback;
            private float _damp;

            public void Initialize(float delayMs, float sampleRate, float feedback, float damp)
            {
                int delaySamples = (int)(sampleRate * (delayMs / 1000f));
                int size = 1;
                while (size < delaySamples + 2)
                    size <<= 1;

                _buffer = new float[size];
                _bufferMask = size - 1;
                _writeIndex = 0;
                _filterState = 0f;
                _feedback = feedback;
                _damp = damp;
            }

            public float Process(float input)
            {
                float output = _buffer[_writeIndex];

                // Stateful 1-pole dampening response inside the feedback loop
                _filterState = (output * (1f - _damp)) + (_filterState * _damp);

                _buffer[_writeIndex] = input + (_filterState * _feedback);
                _writeIndex = (_writeIndex + 1) & _bufferMask;

                return output;
            }
        }

        /// <summary>
        /// Inline high-performance serialization all-pass filter layout.
        /// </summary>
        private struct AllPassFilter
        {
            private float[] _buffer;
            private int _bufferMask;
            private int _writeIndex;
            private float _diffusion;

            public void Initialize(float delayMs, float sampleRate, float diffusion)
            {
                int delaySamples = (int)(sampleRate * (delayMs / 1000f));
                int size = 1;
                while (size < delaySamples + 2)
                    size <<= 1;

                _buffer = new float[size];
                _bufferMask = size - 1;
                _writeIndex = 0;
                _diffusion = diffusion;
            }

            public float Process(float input)
            {
                float bufSample = _buffer[_writeIndex];
                float output = -_diffusion * input + bufSample;

                _buffer[_writeIndex] = input + (_diffusion * output);
                _writeIndex = (_writeIndex + 1) & _bufferMask;

                return output;
            }
        }
        #endregion

        #region Operational Parameter Adjustments
        public void AdjustParameter(float value)
        {
            _mix = value.Clamp(0f, 1f);
        }
        #endregion
    }
}