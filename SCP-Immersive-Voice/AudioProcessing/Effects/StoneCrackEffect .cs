using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Physical Modeling Stone Fracture Generator tailored for SCP-173.
    /// Excites a low-frequency high-Q modal concrete resonator matrix using sparse, explosive
    /// impulse streams, while actively cross-modulating the voice buffer to strip humanity.
    /// </summary>
    public class StoneCrackEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _intensity;
        private readonly float _sampleRate;
        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        // Cascaded processing filter topologies
        private BiquadFilter _stoneBodyResonator;
        private BiquadFilter _brittleSurfaceResonator;

        // Stateful parameters for low-level register synchronization managed via local stack windows
        private int _cascadeSamplesRemaining;
        private int _ticksUntilNextSnap;
        private float _cascadeEnergy;
        private float _envelope;
        private uint _lcgState;
        #endregion

        #region Public Metadata Properties
        public string Name => "Stone Crack";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the structural Stone Crack engine.
        /// </summary>
        public StoneCrackEffect(float intensity, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Enforcing boundary safety straight via math extensions primitives
            _intensity = intensity.Clamp(0f, 2.0f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Deep structural concrete mass resonance (220Hz) with extreme Q for solid metallic-rock thud
            _stoneBodyResonator.ConfigureBandPass(220f, _sampleRate, 38.0f);

            // Mid-range stone surface cleavage fracture line (1100Hz) with high rigidity
            _brittleSurfaceResonator.ConfigureBandPass(1100f, _sampleRate, 16.0f);

            // PERFORMANCE FIX: Shifted trigonometry generation directly into float-native structures
            _envAttackCoef = Mathf.Exp(-1000f / (3f * _sampleRate));
            _envReleaseCoef = Mathf.Exp(-1000f / (80f * _sampleRate));

            _cascadeSamplesRemaining = 0;
            _ticksUntilNextSnap = 0;
            _cascadeEnergy = 0f;
            _envelope = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _intensity < 0.01f) return;

            // Drastically lowered base frequency to produce rare, massive impacts instead of sandy grain buzz
            float baseTriggerChance = (0.0012f * _intensity) / _sampleRate;
            uint triggerThreshold = (uint)(baseTriggerChance * uint.MaxValue);

            // Extracted the static mix coefficient calculation and boundary limits outside the sample processing sweep block.
            float wetMix = (_intensity * 0.65f).Clamp(0f, 0.85f);

            // Cache volatile parameters, structures and counters directly onto the stack.
            // Bypasses persistent pointer memory chasing completely to guarantee extreme packet processing speed.
            float localEnvelope = _envelope;
            int localCascadeRemaining = _cascadeSamplesRemaining;
            int localTicksUntilSnap = _ticksUntilNextSnap;
            float localCascadeEnergy = _cascadeEnergy;
            uint localLcgState = _lcgState;

            BiquadFilter fBody = _stoneBodyResonator;
            BiquadFilter fSurface = _brittleSurfaceResonator;

            float att = _envAttackCoef;
            float rel = _envReleaseCoef;
            float rate = _sampleRate;
            float intense = _intensity;

            for (int i = 0; i < length; i++)
            {
                float dryInput = pcm[i];

                // 1. Dynamic voice envelope tracking via fluent primitives
                float absInput = dryInput.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = att * localEnvelope + (1f - att) * absInput;
                }
                else
                {
                    localEnvelope = rel * localEnvelope + (1f - rel) * absInput;
                }

                // 2. Advance fast local LCG sequence (1 CPU cycle cost execution)
                localLcgState = localLcgState * 1103515245 + 12345;
                float structuralImpulse = 0f;

                // 3. Check for fresh rock fault line failure cascades
                if (localCascadeRemaining <= 0)
                {
                    uint dynamicThreshold = (uint)(triggerThreshold * (0.05f + localEnvelope * 2.4f));

                    if (localLcgState < dynamicThreshold)
                    {
                        float randomMod = (float)(localLcgState & 0xFFFF) / 65535f;
                        // Long, heavy fault line failure cascades (60ms to 200ms)
                        float cascadeDurationMs = 60f + (randomMod * 140f * intense);

                        localCascadeRemaining = (int)(rate * (cascadeDurationMs / 1000f));
                        localCascadeEnergy = 0.7f + (randomMod * 0.5f); // High impact force
                        localTicksUntilSnap = 0;
                    }
                }

                // 4. Execute active structural fracture breakdown loops
                if (localCascadeRemaining > 0)
                {
                    if (localTicksUntilSnap <= 0)
                    {
                        float snapSign = ((localLcgState & 0x2000) != 0) ? 1f : -1f;
                        structuralImpulse = snapSign * localCascadeEnergy;

                        // Extended time gap (25ms to 115ms) to separate impulses into hard, heavy macroscopic events
                        uint lcgBits = localLcgState * 1103515245 + 12345;
                        float deltaMod = (float)(lcgBits & 0xFFFF) / 65535f;
                        float timeGapMs = 25f + (deltaMod * 90f);

                        localTicksUntilSnap = (int)(rate * (timeGapMs / 1000f));
                        localCascadeEnergy *= 0.78f; // Slow decay rate to let the structural energy echo
                    }

                    localTicksUntilSnap--;
                    localCascadeRemaining--;
                }

                // 5. Excite concrete resonators inside stack memory register windows
                float bodyResonance = fBody.Process(structuralImpulse);
                float surfaceResonance = fSurface.Process(structuralImpulse);

                // Emphasize the deep concrete sub-thud (80% body mass, 20% surface cleave)
                float combinedStoneCrack = (bodyResonance * 0.8f) + (surfaceResonance * 0.2f);

                float drivenCrack = combinedStoneCrack * 5.0f;
                float hardShapedCrack = drivenCrack / (1f + drivenCrack.Abs());

                // 6. Vocal Shredder Cross-Modulation: 
                // The physical fracture actively deconstructs and cancels out the phase of the human voice stream.
                float voiceDestructionFactor = (1.0f - (hardShapedCrack.Abs() * 1.6f)).LimitMin(-0.3f);

                pcm[i] = (dryInput * voiceDestructionFactor) + (hardShapedCrack * wetMix);
            }

            // Flush calculated stack structures back into object persistent instance layout fields atomically.
            _envelope = localEnvelope;
            _cascadeSamplesRemaining = localCascadeRemaining;
            _ticksUntilNextSnap = localTicksUntilSnap;
            _cascadeEnergy = localCascadeEnergy;
            _lcgState = localLcgState;
            _stoneBodyResonator = fBody;
            _brittleSurfaceResonator = fSurface;
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
            _intensity = value.Clamp(0f, 2.0f);
        }
        #endregion


    }
}