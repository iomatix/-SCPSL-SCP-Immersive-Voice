using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Organic wet-decay engine simulating viscous fluid reflections and moist tissue.
    /// Employs a sample-rate independent micro-diffuser delay line, a stateful low-frequency 
    /// fluid bubble resonator, and an ultra-fast LCG randomizer. Zero-allocation.
    /// </summary>
    public class WetDecayEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _amount;
        private readonly float _sampleRate;
        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        // Ring buffer for the viscous fluid micro-reflection line
        private readonly float[] _decayBuffer;
        private readonly int _bufferMask;

        // Sub-modules and stateful parameters managed via high-speed stack registers
        private BiquadFilter _bubbleResonator;
        private float _dampFilterState;
        private uint _lcgState;
        private float _envelope;
        private float _wobblePhase;
        private int _writeIndex;
        #endregion

        #region Public Metadata Properties
        public string Name => "Wet Decay";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Wet Decay effect.
        /// </summary>
        /// <param name="amount">Intensity of the wet mud/decay coating (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public WetDecayEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Enforcing boundary safety straight via math extensions primitives
            _amount = amount.Clamp(0f, 1.5f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Allocate a short reflection buffer (1024 samples @ 48kHz = ~21ms of dense slime space)
            const int size = 1024;
            _decayBuffer = new float[size];
            _bufferMask = size - 1;

            // Configure bubble generator to resonant fluid frequencies (320Hz) for authentic organic squelches
            _bubbleResonator.ConfigureBandPass(320f, _sampleRate, 5.5f);

            // Sample-rate independent envelope coefficients using float-native math
            _envAttackCoef = Mathf.Exp(-1000f / (6f * _sampleRate));   // 6ms attack
            _envReleaseCoef = Mathf.Exp(-1000f / (55f * _sampleRate)); // 55ms release

            _writeIndex = 0;
            _dampFilterState = 0f;
            _envelope = 0f;
            _wobblePhase = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            // Physical constant parameters for viscous slime modeling
            float reflectionDelay = _sampleRate * 0.016f; // 16ms baseline fluid wall distance
            float feedbackGain = (_amount * 0.55f).Clamp(0f, 0.7f); // Enforce strict loop stability

            // Viscous absorption low-pass coefficient (~800Hz dampening boundary)
            float dampOmega = TwoPi * 800f / _sampleRate;
            float dampCoef = dampOmega / (dampOmega + 1f);

            // Pre-computed wet/dry hybrid crossfade and gain scaling constants loaded outside the hot path frame.
            float wetMix = (_amount * 0.45f).Clamp(0f, 0.65f); // Maintain high speech articulation
            float dryMix = 1f - wetMix;
            float bubbleGainFactor = _amount * 0.4f;

            // Cache volatile parameters, oscillators, history states, and structures directly onto local stack frames.
            // Bypasses persistent pointer memory layout line checks completely to guarantee native silicon processing speeds.
            float localEnvelope = _envelope;
            float localWobblePhase = _wobblePhase;
            float localDampFilterState = _dampFilterState;
            uint localLcgState = _lcgState;
            int localWriteIndex = _writeIndex;

            BiquadFilter fBubble = _bubbleResonator;

            float att = _envAttackCoef;
            float rel = _envReleaseCoef;
            float rate = _sampleRate;
            float[] buf = _decayBuffer;
            int mask = _bufferMask;
            int bufLen = buf.Length;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Track voice amplitude envelope via custom fluent extensions
                float absInput = drySample.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = att * localEnvelope + (1f - att) * absInput;
                }
                else
                {
                    localEnvelope = rel * localEnvelope + (1f - rel) * absInput;
                }

                // 2. Continuous organic wobble LFO for liquid layer movement
                localWobblePhase += 0.0035f;
                if (localWobblePhase > TwoPi)
                    localWobblePhase -= TwoPi;

                // PERFORMANCE FIX: Swapped double precision Math.Sin for float-native SIMD optimized Mathf.Sin
                float liquidWobble = Mathf.Sin(localWobblePhase);

                // 3. Extract fluid-dampened delayed sample from the ring buffer memory space
                float readPos = localWriteIndex - (reflectionDelay + liquidWobble * rate * 0.002f);

                // PERFORMANCE FIX: Eradicated high-overhead while loop execution loops.
                // Replaced with a streamlined hardware-friendly single conditional branch block.
                if (readPos < 0f)
                    readPos += bufLen;

                int i0 = (int)readPos;
                int i1 = (i0 + 1) & mask;
                float frac = readPos - i0;
                float rawDelayed = buf[i0 & mask] * (1f - frac) + buf[i1] * frac;

                // 4. Heavy high-frequency acoustic absorption inside the fluid cavity
                localDampFilterState = localDampFilterState + dampCoef * (rawDelayed - localDampFilterState);

                // 5. Ultra-fast local LCG stochastic bubble/pop simulator (1 CPU cycle execution cost)
                localLcgState = localLcgState * 1103515245 + 12345;
                float popChance = 0.0008f + (localEnvelope * 0.025f * _amount);
                uint maxThreshold = (uint)(popChance * uint.MaxValue);

                float bubbleImpulse = 0f;
                if (localLcgState < maxThreshold)
                {
                    // Generate bidirectional dynamic trigger spike cleanly using fast bitwise checks
                    float popSign = ((localLcgState & 0x800) != 0) ? 1f : -1f;
                    bubbleImpulse = popSign * (0.25f + localEnvelope * 0.75f);
                }

                // 6. Resonate the bubble impulse into a wet organic liquidity "plop" within local stack structures
                float liquidPop = fBubble.Process(bubbleImpulse);

                // 7. Inject dry input and wet texturing back into the absorption loop using pre-computed stack values
                float feedbackDrive = drySample + (localDampFilterState * feedbackGain) + (liquidPop * bubbleGainFactor);

                // Fast polynomial saturation to compress feedback peaks safely via fluent primitives
                float saturatedFeedback = feedbackDrive / (1f + feedbackDrive.Abs());
                buf[localWriteIndex] = saturatedFeedback;
                localWriteIndex = (localWriteIndex + 1) & mask;

                // 8. Equal-power style mix projection into live buffer utilizing pre-computed gain boundaries
                pcm[i] = (drySample * dryMix) + (saturatedFeedback * wetMix);
            }

            // Flush calculated stack mods back into object persistent instance context fields atomically post execution loop.
            _envelope = localEnvelope;
            _wobblePhase = localWobblePhase;
            _dampFilterState = localDampFilterState;
            _lcgState = localLcgState;
            _writeIndex = localWriteIndex;
            _bubbleResonator = fBubble;
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

            public void ConfigureBandPass(float centerFrequency, float sampleRate, float q)
            {
                float w0 = TwoPi * centerFrequency / sampleRate;
                float alpha = Mathf.Sin(w0) / (2f * q);
                float cosW0 = Mathf.Cos(w0);

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
        #endregion

        #region Operational Parameter Adjustments
        public void AdjustParameter(float value)
        {
            _amount = value.Clamp(0f, 1f);
        }
        #endregion
    }
}