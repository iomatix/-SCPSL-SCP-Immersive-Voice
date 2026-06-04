namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA Organic wet-decay engine simulating viscous fluid reflections and moist tissue.
    /// Employs a sample-rate independent micro-diffuser delay line, a stateful low-frequency 
    /// fluid bubble resonator, and an ultra-fast LCG randomizer. Zero-allocation.
    /// </summary>
    public class WetDecayEffect : IAudioEffect
    {
        public string Name => "Wet Decay";

        private readonly float _amount;
        private readonly float _sampleRate;

        // Ring buffer for the viscous fluid micro-reflection line
        private readonly float[] _decayBuffer;
        private readonly int _bufferMask;
        private int _writeIndex;

        // Sub-modules for biological texturing
        private BiquadFilter _bubbleResonator;
        private float _dampFilterState = 0f;

        // Stateful tracking parameters
        private uint _lcgState;
        private float _envelope = 0f;
        private float _wobblePhase = 0f;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        /// <summary>
        /// Initializes the Wet Decay effect.
        /// </summary>
        /// <param name="amount">Intensity of the wet mud/decay coating (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public WetDecayEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Allocate a short reflection buffer (1024 samples @ 48kHz = ~21ms of dense slime space)
            int size = 1024;
            _decayBuffer = new float[size];
            _bufferMask = size - 1;
            _writeIndex = 0;

            // Configure bubble generator to resonant fluid frequencies (320Hz) for authentic organic squelches
            _bubbleResonator.ConfigureBandPass(320f, _sampleRate, q: 5.5f);

            // Sample-rate independent envelope coefficients
            _envAttackCoef = (float)Math.Exp(-1000.0 / (6f * _sampleRate));   // 6ms attack
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (55f * _sampleRate)); // 55ms release
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            // Physical constant parameters for viscous slime modeling
            float reflectionDelay = _sampleRate * 0.016f; // 16ms baseline fluid wall distance
            float feedbackGain = _amount * 0.55f;
            if (feedbackGain > 0.7f) feedbackGain = 0.7f; // Enforce strict loop stability

            // Viscous absorption low-pass coefficient (~800Hz dampening boundary)
            float dampOmega = 2f * (float)Math.PI * 800f / _sampleRate;
            float dampCoef = dampOmega / (dampOmega + 1f);

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Track voice amplitude envelope
                float absInput = Math.Abs(drySample);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                // 2. Continuous organic wobble LFO for liquid layer movement
                _wobblePhase += 0.0035f;
                if (_wobblePhase > 2f * (float)Math.PI) _wobblePhase -= 2f * (float)Math.PI;
                float liquidWobble = (float)Math.Sin(_wobblePhase);

                // 3. Extract fluid-dampened delayed sample from the ring buffer
                float readPos = _writeIndex - (reflectionDelay + liquidWobble * _sampleRate * 0.002f);
                while (readPos < 0f) readPos += _decayBuffer.Length;

                int i0 = (int)readPos;
                int i1 = (i0 + 1) & _bufferMask;
                float frac = readPos - i0;
                float rawDelayed = _decayBuffer[i0 & _bufferMask] * (1f - frac) + _decayBuffer[i1] * frac;

                // 4. Heavy high-frequency acoustic absorption inside the fluid cavity
                _dampFilterState = _dampFilterState + dampCoef * (rawDelayed - _dampFilterState);

                // 5. Ultra-fast local LCG stochastic bubble/pop simulator
                _lcgState = _lcgState * 1103515245 + 12345;
                float popChance = 0.0008f + (_envelope * 0.025f * _amount);
                uint maxThreshold = (uint)(popChance * uint.MaxValue);

                float bubbleImpulse = 0f;
                if (_lcgState < maxThreshold)
                {
                    // Generate bidirectional dynamic trigger spike
                    float popSign = ((_lcgState & 0x800) != 0) ? 1f : -1f;
                    bubbleImpulse = popSign * (0.25f + _envelope * 0.75f);
                }

                // 6. Resonate the bubble impulse into a wet organic liquidity "plop"
                float liquidPop = _bubbleResonator.Process(bubbleImpulse);

                // 7. Inject dry input and wet texturing back into the absorption loop
                float feedbackDrive = drySample + (_dampFilterState * feedbackGain) + (liquidPop * _amount * 0.4f);

                // Fast polynomial saturation to compress feedback peaks safely
                float saturatedFeedback = feedbackDrive / (1f + Math.Abs(feedbackDrive));
                _decayBuffer[_writeIndex] = saturatedFeedback;
                _writeIndex = (_writeIndex + 1) & _bufferMask;

                // 8. Equal-power style mix projection into live buffer
                float wetMix = _amount * 0.45f;
                if (wetMix > 0.65f) wetMix = 0.65f; // Maintain high speech articulation

                pcm[i] = (drySample * (1f - wetMix)) + (saturatedFeedback * wetMix);
            }
        }

        // High-performance, stack-allocated 2nd order IIR filter structure
        private struct BiquadFilter
        {
            private float _b0, _b1, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public void ConfigureBandPass(float centerFrequency, float sampleRate, float q)
            {
                float w0 = 2f * (float)Math.PI * centerFrequency / sampleRate;
                float alpha = (float)Math.Sin(w0) / (2f * q);
                float cosW0 = (float)Math.Cos(w0);

                float a0 = 1f + alpha;
                _b0 = alpha / a0;
                _b1 = 0f;
                _b2 = -alpha / a0;
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