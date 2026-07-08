using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Custom Silicon Ring Modulator and Mainframe Enclosure Comb Resonator.
    /// Destroys human harmonic intervals using a tracking low-frequency carrier wave 
    /// and simulates empty server-rack steel cabinet acoustic boundary reflections. Zero allocations.
    /// </summary>
    public class SiliconRingModulatorEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const int CombDelaySamples = 144;
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _amount;
        private readonly float _sampleRate;
        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        // Fixed circular buffer for server chassis acoustic comb reflections (4ms delay boundary)
        private readonly float[] _combBuffer = new float[CombDelaySamples];

        // Stateful trackers synchronized via local stack register windows
        private int _combWritePtr;
        private float _carrierPhase;
        private float _envelope;
        #endregion

        #region Public Metadata Properties
        public string Name => "Silicon Ring Modulator";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="SiliconRingModulatorEffect"/> class.
        /// </summary>
        /// <param name="amount">Intensity of the ring modulation and metal resonance (0.0f to 1.0f).</param>
        /// <param name="sampleRate">The primary engine VoIP sample rate.</param>
        public SiliconRingModulatorEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Enforcing boundary safety straight via math extensions primitives
            _amount = amount.Clamp(0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // PERFORMANCE FIX: Trigonometry coefficients mapped directly into float-native structures
            _envAttackCoef = Mathf.Exp(-1000f / (5f * _sampleRate));   // 5ms tracking response
            _envReleaseCoef = Mathf.Exp(-1000f / (75f * _sampleRate)); // 75ms release smoothing

            _combWritePtr = 0;
            _carrierPhase = 0f;
            _envelope = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            // Gain scaling matrix constants loaded outside the hot path
            float wetMix = _amount * 0.75f;
            float dryMix = 1f - (wetMix * 0.4f);

            // Pre-computed constants for loop throughput acceleration
            float wetGainFactor = wetMix * 1.2f;

            // Cache volatile parameters, pointers, and tracking variables directly into the CPU stack frame.
            // Completely cuts off L1/L2 cache line pointer chasing overhead across the high-frequency loop.
            float localEnvelope = _envelope;
            float localCarrierPhase = _carrierPhase;
            int localCombWritePtr = _combWritePtr;

            float att = _envAttackCoef;
            float rel = _envReleaseCoef;
            float rate = _sampleRate;
            float[] combBuf = _combBuffer;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // 1. Fast RMS-style absolute voice envelope tracking via fluent primitives
                float absInput = dry.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = att * localEnvelope + (1f - att) * absInput;
                }
                else
                {
                    localEnvelope = rel * localEnvelope + (1f - rel) * absInput;
                }

                // 2. Cybernetic Inharmonic Carrier Wave Generation
                // The carrier shifts its frequency dynamically under vocal load (from 58Hz up to 92Hz)
                // to disrupt static structures and simulate logic gate power fluctuations.
                float dynamicCarrierFreq = 58f + (localEnvelope * 34f);
                localCarrierPhase += (TwoPi * dynamicCarrierFreq) / rate;
                if (localCarrierPhase > TwoPi)
                    localCarrierPhase -= TwoPi;

                // PERFORMANCE FIX: Swapped double precision Math.Cos for float-native SIMD optimized Mathf.Cos
                float sineCarrier = Mathf.Cos(localCarrierPhase);

                // Reshape to a jagged, cold pseudo-square wave typical of silicon transistors
                float siliconCarrier = sineCarrier > 0f ? 0.7f : -0.7f;

                // 3. Intermodulate: Multiply raw human voice with the mathematical silicon grid
                float modulatedNode = dry * siliconCarrier;

                // 4. Server Rack Steel Chassis Simulation (Comb Filtering Layer)
                // Read the old reflected sample from the fixed matrix using local stack register index
                float delayedSample = combBuf[localCombWritePtr];

                // 45% feedback coefficient models high-frequency metal scattering boundaries
                float combResonance = modulatedNode + (delayedSample * 0.45f);

                // Write back the current composite node to the circular lattice memory space
                combBuf[localCombWritePtr] = combResonance;
                localCombWritePtr++;

                // Optimized linear branch predictor bounds check for non-power-of-two arrays
                if (localCombWritePtr >= CombDelaySamples)
                    localCombWritePtr = 0;

                // 5. Finalize staging mix injection utilizing pre-computed gain constants
                pcm[i] = (dry * dryMix) + (combResonance * wetGainFactor);
            }

            // Flush calculated stack modifications back into object persistent instance trackers atomically.
            _envelope = localEnvelope;
            _carrierPhase = localCarrierPhase;
            _combWritePtr = localCombWritePtr;
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