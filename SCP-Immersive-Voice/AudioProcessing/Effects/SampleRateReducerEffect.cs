using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Digital clock-divider sample rate reducer (downsampler).
    /// Employs a persistent frame-independent phase accumulator, a high-speed LCG clock jitter 
    /// emulator, and a low-quality reconstruction DAC filter to generate authentic aliasing.
    /// </summary>
    public class SampleRateReducerEffect : IAdjustableAudioEffect
    {
        #region Private Execution Vectors
        private float _amount;
        private readonly float _sampleRate;
        private readonly float _jitterLfoIncrement;

        // Persistent sample-and-hold states across frame boundaries synchronized via local stack registers
        private float _phaseAccumulator;
        private float _heldSample;
        private float _reconstructionState;
        private float _jitterLfoPhase;

        // Local thread-safe LCG random state tracker
        private uint _lcgState;
        #endregion

        #region Public Metadata Properties
        public string Name => "Sample Rate Reducer";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Sample Rate Reducer effect.
        /// </summary>
        /// <param name="amount">Intensity of downsampling (0.0f = clear, 1.0f = severe 800Hz aliasing).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public SampleRateReducerEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Enforcing safe operational bounds straight via math extensions
            _amount = amount.Clamp(0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Slow clock thermal drift LFO speed (~1.2 Hz)
            _jitterLfoIncrement = 1.2f / _sampleRate;

            _phaseAccumulator = 0f;
            _heldSample = 0f;
            _reconstructionState = 0f;
            _jitterLfoPhase = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            // PERFORMANCE FIX: Swapped double precision Math.Pow for float-native Mathf.Pow
            // Map linear amount to an exponential frequency downsampling divisor
            // At amount=0: factor=1.0 (no reduction). At amount=1: factor=~0.016 (down to 800Hz)
            float targetFrequencyFactor = Mathf.Pow(2f, -_amount * 6.0f);

            // Configure lossy reconstruction DAC filter coefficients (~2200Hz smoothing window)
            float rcOmega = 2f * Mathf.PI * 2200f / _sampleRate;
            float reconstructionDampCoef = rcOmega / (rcOmega + 1f);

            // Cache volatile parameters, accumulators and LCG seeds directly inside the CPU stack context.
            // Bypasses persistent memory line synchronization tracking completely across the Hot-Path loop.
            float localPhaseAccumulator = _phaseAccumulator;
            float localHeldSample = _heldSample;
            float localReconstructionState = _reconstructionState;
            float localJitterLfoPhase = _jitterLfoPhase;
            uint localLcgState = _lcgState;

            float amtScalar = _amount;
            float lfoInc = _jitterLfoIncrement;

            for (int i = 0; i < length; i++)
            {
                float dryInput = pcm[i];

                // 1. Advance thermal clock drift LFO via fast polynomial triangle-to-parabola approximation
                localJitterLfoPhase += lfoInc;
                if (localJitterLfoPhase > 1f)
                    localJitterLfoPhase -= 1f;

                float tri = localJitterLfoPhase * 2f;
                if (tri > 1f)
                    tri = 2f - tri;

                float clockDrift = 4f * tri * (1f - tri);

                // 2. Compute ultra-fast LCG high-frequency phase jitter
                localLcgState = localLcgState * 1103515245 + 12345;
                float phaseJitter = ((float)(localLcgState & 0xFFFF) / 65535f) * 0.12f * amtScalar;

                // 3. Modulate step clock rate based on jitter matrix parameters
                float currentStepRate = targetFrequencyFactor * (1f - (clockDrift * 0.08f * amtScalar));

                // 4. Persistent Phase Accumulator step
                localPhaseAccumulator += currentStepRate + phaseJitter;

                // When phase accumulator crosses the unit threshold, open the digital gate to latch a new sample
                if (localPhaseAccumulator >= 1f || localHeldSample is 0f)
                {
                    // PERFORMANCE FIX: Eradicated double precision Math.Floor loop tracker.
                    // Utilizing our custom fluent float-native extension method to retain fractional phase.
                    localPhaseAccumulator -= localPhaseAccumulator.Floor();
                    localHeldSample = dryInput;
                }

                // 5. Simulate bad historical DAC reconstruction (stair-case smoothing smear)
                // Blends pristine digital aliasing stair-steps with loose analog cable capacitance
                localReconstructionState += reconstructionDampCoef * (localHeldSample - localReconstructionState);

                // 6. Polynomial soft-clipping saturation to emulate vintage operational amplifier warmth via fluent primitives
                float driven = localReconstructionState * 1.1f;
                float saturatedOut = driven / (1f + driven.Abs());

                // 7. Full insert injection into the target PCM buffer
                pcm[i] = saturatedOut;
            }

            // Write local stack variables back into class tracking persistent context structures atomically post loop execution.
            _phaseAccumulator = localPhaseAccumulator;
            _heldSample = localHeldSample;
            _reconstructionState = localReconstructionState;
            _jitterLfoPhase = localJitterLfoPhase;
            _lcgState = localLcgState;
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