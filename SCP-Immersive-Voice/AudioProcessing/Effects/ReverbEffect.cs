namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA Studio-grade Schroeder-Moorer Reverb Engine.
    /// Employs 4 parallel low-pass feedback comb filters for structural echo density
    /// and 2 cascaded all-pass filters for smooth spectral diffusion. Zero allocations in loop.
    /// </summary>
    public class ReverbEffect : IAudioEffect
    {
        public string Name => "Reverb";

        private readonly float _mix;

        // Parallel Low-Pass Feedback Comb Filters (LBCF) for early reflection density
        private LowPassCombFilter _comb1;
        private LowPassCombFilter _comb2;
        private LowPassCombFilter _comb3;
        private LowPassCombFilter _comb4;

        // Cascaded All-Pass Filters (APF) for late decay diffusion
        private AllPassFilter _allPass1;
        private AllPassFilter _allPass2;

        /// <summary>
        /// Initializes the Reverb Effect.
        /// </summary>
        /// <param name="mix">Reverb wet mix coefficient (0.0f = dry, 1.0f = hot decay room).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public ReverbEffect(float mix, float sampleRate)
        {
            _mix = Clamp(mix, 0f, 1f);
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            // Tailored prime-like delay times in milliseconds to prevent metallic comb resonances
            _comb1.Initialize(29.7f, sr, feedback: 0.82f, damp: 0.25f);
            _comb2.Initialize(37.1f, sr, feedback: 0.78f, damp: 0.28f);
            _comb3.Initialize(41.3f, sr, feedback: 0.75f, damp: 0.32f);
            _comb4.Initialize(45.7f, sr, feedback: 0.71f, damp: 0.35f);

            // Diffusion stages parameters
            _allPass1.Initialize(5.2f, sr, diffusion: 0.7f);
            _allPass2.Initialize(1.7f, sr, diffusion: 0.6f);
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _mix < 0.01f) return;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // 1. Process parallel comb filters matrix to build acoustic energy density
                float c1 = _comb1.Process(dry);
                float c2 = _comb2.Process(dry);
                float c3 = _comb3.Process(dry);
                float c4 = _comb4.Process(dry);

                // Sum parallel structures (attenuated to prevent accumulation clipping)
                float combSum = (c1 + c2 + c3 + c4) * 0.25f;

                // 2. Pass through cascaded all-pass filters to smear impulses into a lush room tail
                float diffused = _allPass1.Process(combSum);
                float wet = _allPass2.Process(diffused);

                // 3. Linear constant-power style mix blend (Pristine retention of source text)
                pcm[i] = (dry * (1f - _mix * 0.5f)) + (wet * _mix * 0.65f);
            }
        }

        // Inline high-performance parallel comb filter layout
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
                while (size < delaySamples + 2) size <<= 1;

                _buffer = new float[size];
                _bufferMask = size - 1;
                _writeIndex = 0;
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

        // Inline high-performance serialization all-pass filter layout
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
                while (size < delaySamples + 2) size <<= 1;

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
    }
}