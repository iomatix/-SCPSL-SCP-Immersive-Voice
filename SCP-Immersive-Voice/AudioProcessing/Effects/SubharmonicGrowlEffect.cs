using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Phase-Locked Subharmonic Generator.
    /// Tracks the voice fundamental frequency (f0) via zero-crossing detection 
    /// and synthesizes a perfect sub-octave (f0 / 2) sub-bass layer.
    /// </summary>
    public class SubharmonicGrowlEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _amount;
        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        // Dynamic Biquad filters for tracking and reconstruction
        private BiquadFilter _inputAnalysisLp;
        private BiquadFilter _subharmonicSmoothLp;

        // Stateful tracking variables managed securely via stack frames
        private float _prevFilteredSample;
        private float _flipFlopState;
        private float _envelope;
        #endregion

        #region Public Metadata Properties
        public string Name => "Subharmonic Growl";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Subharmonic Growl Effect.
        /// </summary>
        /// <param name="amount">Intensity of the subharmonic growl (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public SubharmonicGrowlEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing pristine clamping straight from your extensions matrix
            _amount = amount.Clamp(0f, 1.5f);
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            // Filter 1: Isolate vocal fundamental frequency (f0) below 130Hz safely
            _inputAnalysisLp.ConfigureLowPass(130f, sr, 0.707f);

            // Filter 2: Smooth out generated raw square sub-edges below 75Hz into pure sub-bass
            _subharmonicSmoothLp.ConfigureLowPass(75f, sr, 0.85f);

            // Envelope follower coefficients using float-native math
            _envAttackCoef = Mathf.Exp(-1000f / (4f * sr));
            _envReleaseCoef = Mathf.Exp(-1000f / (45f * sr));

            _prevFilteredSample = 0f;
            _flipFlopState = 1f;
            _envelope = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            // Pre-computed gain staging mix constants outside the hot-path sweep block.
            // Eliminates thousands of redundant floating-point multiplications per voice frame buffer.
            float wetGainFactor = _amount * 0.75f;

            // Cache volatile counters, tracking vectors, and structural filter layouts straight onto stack frame registers.
            // Prevents heap memory reference chasing overhead completely across high-frequency packet loops.
            float localEnvelope = _envelope;
            float localFlipFlop = _flipFlopState;
            float localPrevSample = _prevFilteredSample;

            BiquadFilter fAnalysis = _inputAnalysisLp;
            BiquadFilter fSmooth = _subharmonicSmoothLp;

            float att = _envAttackCoef;
            float rel = _envReleaseCoef;

            for (int i = 0; i < length; i++)
            {
                float inputSample = pcm[i];

                // 1. Track global amplitude envelope for dynamic scaling via fluent primitives
                float absInput = inputSample.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = att * localEnvelope + (1f - att) * absInput;
                }
                else
                {
                    localEnvelope = rel * localEnvelope + (1f - rel) * absInput;
                }

                // 2. Isolate the low fundamental using the first cached register Biquad filter
                float fundamentalZone = fAnalysis.Process(inputSample);

                // 3. Time-domain frequency divider (Zero-Crossing Flip-Flop execution)
                // Every full cycle of fundamentalZone triggers half a cycle of _flipFlopState (f0 / 2)
                if (fundamentalZone > 0f && localPrevSample <= 0f)
                {
                    localFlipFlop = -localFlipFlop;
                }
                localPrevSample = fundamentalZone;

                // 4. Shape and reconstruct the subharmonic wave inside register windows
                float rawSub = localFlipFlop * localEnvelope;
                float cleanSubBass = fSmooth.Process(rawSub);

                // 5. Apply polynomial soft-clipping for a warm, guttural monster growl via fluent abs extensions
                float drivenSub = cleanSubBass * 1.6f;
                float saturatedSub = drivenSub / (1f + drivenSub.Abs());

                // 6. Mix the synthesized cinematic sub-bass back into the primary signal stream
                pcm[i] = inputSample + (saturatedSub * wetGainFactor);
            }

            // Flush calculated stack modifications back into persistent instance data boundaries atomically.
            _envelope = localEnvelope;
            _flipFlopState = localFlipFlop;
            _prevFilteredSample = localPrevSample;
            _inputAnalysisLp = fAnalysis;
            _subharmonicSmoothLp = fSmooth;
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

            public void ConfigureLowPass(float cutoffFrequency, float sampleRate, float q)
            {
                float w0 = TwoPi * cutoffFrequency / sampleRate;
                float alpha = Mathf.Sin(w0) / (2f * q);
                float cosW0 = Mathf.Cos(w0);

                float a0 = 1f + alpha;
                _b0 = ((1f - cosW0) / 2f) / a0;
                _b1 = (1f - cosW0) / a0;
                _b2 = ((1f - cosW0) / 2f) / a0;
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