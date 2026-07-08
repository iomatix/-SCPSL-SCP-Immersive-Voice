using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Stateful physical-modeling DSP effect simulating liquid pooling in a decaying trachea.
    /// Employs a voice-envelope driven stochastic LCG bubble generator and a modulated viscous 
    /// comb filter to synthesize realistic death rattle choking textures. Zero heap allocations.
    /// </summary>
    public class DeathRattleEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const int CombSize = 256;
        private const int CombMask = CombSize - 1;
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _amount;
        private readonly float _sampleRate;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        // Ultra-short comb filter ring buffer modeling viscous throat fluid reflections (Max 4ms delay)
        private readonly float[] _combBuffer = new float[CombSize];

        // Stateful tracking parameters managed under local stack windows
        private int _writePtr;
        private float _envelope;
        private float _bubblePhase;
        private uint _lcgState;
        #endregion

        #region Public Metadata Properties
        public string Name => "Death Rattle";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="DeathRattleEffect"/> class.
        /// </summary>
        /// <param name="amount">The wet mix intensity of the choking rattle (0.0f to 1.0f).</param>
        /// <param name="sampleRate">The audio engine sample rate from VoiceChatSettings.</param>
        public DeathRattleEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the argument payload
            _amount = amount.Clamp(0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            _envAttackCoef = Mathf.Exp(-1000f / (6f * _sampleRate));   // Fast 6ms tracking
            _envReleaseCoef = Mathf.Exp(-1000f / (55f * _sampleRate)); // 55ms release smoothing

            _writePtr = 0;
            _envelope = 0f;
            _bubblePhase = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            float wetMix = _amount * 0.75f;
            float dryMix = 1f - (wetMix * 0.35f);

            // Baseline viscous comb delay boundaries (0.5ms to 2.2ms)
            float baseDelaySamples = _sampleRate * 0.0005f;
            float modDepthSamples = _sampleRate * 0.0017f;

            // Isolating and loading stateful fields onto stack-bound local hardware registers.
            // Bypasses pointer chasing across the heap layout for the duration of the VoIP processing tick.
            float localEnvelope = _envelope;
            float localBubblePhase = _bubblePhase;
            uint localLcgState = _lcgState;
            int localWritePtr = _writePtr;

            float attCoef = _envAttackCoef;
            float relCoef = _envReleaseCoef;
            float rate = _sampleRate;
            float[] combBuf = _combBuffer;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // 1. Track voice amplitude envelope smoothly
                float absInput = dry.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = attCoef * localEnvelope + (1f - attCoef) * absInput;
                }
                else
                {
                    localEnvelope = relCoef * localEnvelope + (1f - relCoef) * absInput;
                }

                // 2. Advance fast local pseudo-random LCG sequence
                localLcgState = localLcgState * 1103515245 + 12345;
                float jitter = ((float)(localLcgState & 0xFFFF) / 65535f) * 0.15f;

                // 3. Dynamic Stochastic LFO (Liquid Bubbling Engine)
                // Bubbling frequency scales exponentially with vocal intensity (14Hz up to 38Hz)
                float bubbleFrequency = 14f + (localEnvelope * 24f) + (jitter * 6f);
                localBubblePhase += (TwoPi * bubbleFrequency) / rate;
                if (localBubblePhase > TwoPi)
                    localBubblePhase -= TwoPi;

                // Generate a chaotic amplitude modulation bubble wave
                float bubbleWave = Mathf.Sin(localBubblePhase);

                // 4. Store current sample inside the viscous comb circular buffer
                combBuf[localWritePtr] = dry;

                // 5. Compute dynamic fractional delay based on bubble wave state
                float targetDelay = baseDelaySamples + ((0.5f + 0.5f * bubbleWave) * modDepthSamples);
                float readPos = localWritePtr - targetDelay;

                // PERFORMANCE FIX: Swapped out high-overhead while iteration gates for a streamlined binary conditional offset assignment
                if (readPos < 0f)
                    readPos += CombSize;

                // Sub-sample linear interpolation to prevent click artifacts
                int i0 = (int)readPos;
                int i1 = (i0 + 1) & CombMask;
                float fraction = readPos - i0;
                float delayedSample = combBuf[i0 & CombMask] * (1f - fraction) + combBuf[i1] * fraction;

                // 40% feedback coefficient models viscous fluid dampening and acoustic scattering
                float combFilteredOutput = dry + (delayedSample * 0.40f);

                // Commit comb node back to circular memory tracking vectors
                combBuf[localWritePtr] = combFilteredOutput;
                localWritePtr = (localWritePtr + 1) & CombMask;

                // 6. Dynamic Amplitude Shuttering: The bubble wave physically blocks the voice amplitude
                float shutteredWet = combFilteredOutput * (0.45f + (0.55f * bubbleWave));

                // 7. Fast polynomial soft-clipping to prevent gain spikes using fluent arithmetic primitives
                float driven = shutteredWet * 1.35f;
                float saturatedWet = driven / (1f + driven.Abs());

                pcm[i] = (dry * dryMix) + (saturatedWet * wetMix);
            }

            // Write computed local variables back into object instance state storage points atomically.
            _envelope = localEnvelope;
            _bubblePhase = localBubblePhase;
            _lcgState = localLcgState;
            _writePtr = localWritePtr;
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