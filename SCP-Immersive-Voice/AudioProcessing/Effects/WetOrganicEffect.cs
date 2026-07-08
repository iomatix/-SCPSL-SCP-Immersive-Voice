using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Wet Organic layer simulating moist, living vocal tract tissue and saliva.
    /// Employs a sub-millisecond fractional micro-delay line, sample-rate independent 
    /// biquad high-pass filtering, and a high-speed bitwise LCG randomizer. Zero allocations.
    /// </summary>
    public class WetOrganicEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _amount;
        private readonly float _sampleRate;
        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;
        private readonly float _wobbleIncrement;

        // Ultra-short power-of-two ring buffer for micro-delay (128 samples @ 48kHz = ~2.6ms)
        private readonly float[] _microDelayBuffer;
        private readonly int _bufferMask;

        // Stateful parameters synchronized via local stack register windows
        private BiquadFilter _wetHighPass;
        private uint _lcgState;
        private float _envelope;
        private float _wobblePhase;
        private int _writeIndex;
        #endregion

        #region Public Metadata Properties
        public string Name => "Wet Organic";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Wet Organic effect.
        /// </summary>
        /// <param name="amount">Intensity of the wet tissue character (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public WetOrganicEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Enforcing boundary safety straight via math extensions primitives
            _amount = amount.Clamp(0f, 1.5f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            const int size = 128;
            _microDelayBuffer = new float[size];
            _bufferMask = size - 1;

            // Isolate high-mid biological saliva friction zones (around 3200Hz) cleanly
            _wetHighPass.ConfigureHighPass(3200f, _sampleRate, 0.707f);

            // PERFORMANCE FIX: Trigonometry coefficients mapped directly into float-native structures
            _envAttackCoef = Mathf.Exp(-1000f / (5f * _sampleRate));   // 5ms attack
            _envReleaseCoef = Mathf.Exp(-1000f / (40f * _sampleRate)); // 40ms release

            // Fast tissue fluid shift modulation (approx. 9.5 Hz micro-wobble)
            _wobbleIncrement = 9.5f / _sampleRate;

            _writeIndex = 0;
            _envelope = 0f;
            _wobblePhase = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            // Pre-computed wet/dry hybrid crossfade and gain scaling constants loaded outside the hot path frame.
            float wetMixFactor = (_amount * 0.45f).Clamp(0f, 0.65f);
            float dryMixFactor = 1f - wetMixFactor;

            float baseDelay = _sampleRate * 0.0008f;  // 0.8ms baseline mucous membrane thickness
            float modDepth = _sampleRate * 0.0005f;   // 0.5ms modulation sweep width
            float lcgAmtScalar = _amount;

            // Cache volatile parameters, oscillators, history states, and structures directly onto local stack frames.
            // Bypasses persistent pointer memory layout line checks completely to guarantee native silicon processing speeds.
            float localEnvelope = _envelope;
            float localWobblePhase = _wobblePhase;
            uint localLcgState = _lcgState;
            int localWriteIndex = _writeIndex;

            BiquadFilter fHighPass = _wetHighPass;

            float att = _envAttackCoef;
            float rel = _envReleaseCoef;
            float wInc = _wobbleIncrement;
            float[] buf = _microDelayBuffer;
            int mask = _bufferMask;
            int bufLen = buf.Length;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Voice envelope follower via custom fluent extensions
                float absInput = drySample.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = att * localEnvelope + (1f - att) * absInput;
                }
                else
                {
                    localEnvelope = rel * localEnvelope + (1f - rel) * absInput;
                }

                // 2. Advance micro-wobble phase using polynomial triangle-to-parabola LFO approximation
                localWobblePhase += wInc;
                if (localWobblePhase > 1f)
                    localWobblePhase -= 1f;

                float tri = localWobblePhase * 2f;
                if (tri > 1f)
                    tri = 2f - tri;
                float tissueWobble = 4f * tri * (1f - tri);

                // 3. Store current dry sample into the circular micro-buffer
                buf[localWriteIndex] = drySample;

                // 4. Compute fractional read position for fluid phase shifting
                float targetDelay = baseDelay + (tissueWobble * modDepth * (0.3f + localEnvelope * 0.7f));
                float readPos = localWriteIndex - targetDelay;

                // PERFORMANCE FIX: Eradicated high-overhead while loop execution loops.
                // Replaced with a streamlined hardware-friendly single conditional branch block.
                if (readPos < 0f)
                    readPos += bufLen;

                int i0 = (int)readPos;
                int i1 = (i0 + 1) & mask;
                float frac = readPos - i0;
                float delayedSample = buf[i0 & mask] * (1f - frac) + buf[i1] * frac;

                // 5. Ultra-fast local LCG stochastic saliva micro-crackle/bubble generator (1 CPU cycle execution cost)
                localLcgState = localLcgState * 1103515245 + 12345;
                float bubbleChance = 0.001f + (localEnvelope * 0.035f * lcgAmtScalar);
                uint maxThreshold = (uint)(bubbleChance * uint.MaxValue);

                float salivaImpulse = 0f;
                if (localLcgState < maxThreshold)
                {
                    // Generate bidirectional dynamic trigger spike cleanly using fast bitwise checks
                    float bubbleSign = ((localLcgState & 0x1000) != 0) ? 1f : -1f;
                    salivaImpulse = bubbleSign * 0.15f * localEnvelope;
                }

                // 6. Combine phase-shifted tissue layer and high-passed micro-bubbles inside stack registers
                float wetCombined = (drySample - delayedSample) + fHighPass.Process(salivaImpulse);

                // 7. Fast polynomial soft-clipping saturation via fluent primitives
                float drivenWet = wetCombined * 1.4f;
                float saturatedWet = drivenWet / (1f + drivenWet.Abs());

                // Increment write index securely using fast bitwise masking boundaries
                localWriteIndex = (localWriteIndex + 1) & mask;

                // 8. In-place buffer interpolation write back utilizing pre-computed gain constants
                pcm[i] = (drySample * dryMixFactor) + (saturatedWet * wetMixFactor);
            }

            // Flush calculated stack mods back into object persistent instance context fields atomically post execution loop.
            _envelope = localEnvelope;
            _wobblePhase = localWobblePhase;
            _lcgState = localLcgState;
            _writeIndex = localWriteIndex;
            _wetHighPass = fHighPass;
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

            public void ConfigureHighPass(float cutoffFrequency, float sampleRate, float q)
            {
                float w0 = TwoPi * cutoffFrequency / sampleRate;
                float alpha = Mathf.Sin(w0) / (2f * q);
                float cosW0 = Mathf.Cos(w0);

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
        #endregion

        #region Operational Parameter Adjustments
        public void AdjustParameter(float value)
        {
            _amount = value.Clamp(0f, 1f);
        }
        #endregion
    }
}