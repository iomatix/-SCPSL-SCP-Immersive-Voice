using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Wet, organic crackle simulating tearing flesh and cellular tissue snapping.
    /// Employs an ultra-fast LCG randomizer, voice amplitude envelope tracking, 
    /// and an excited lossy bandpass resonator matrix. Zero allocations, real-time safe.
    /// </summary>
    public class FleshCrackleEffect : IAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private readonly float _amount;
        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        // Stack-allocated biquad resonator to shape raw impulses into wet squelches
        private BiquadFilter _wetResonator;

        // Stateful parameters for envelope and LCG random seed managed under local stack windows
        private float _envelope;
        private uint _lcgState;
        #endregion

        #region Public Metadata Properties
        public string Name => "Flesh Crackle";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Flesh Crackle effect.
        /// </summary>
        /// <param name="amount">Density and intensity of the flesh crackle layer (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public FleshCrackleEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the argument payload
            _amount = amount.Clamp(0f, 1.5f);
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            // Seed the fast LCG using a unique identifier hash
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Configure the resonator filter to the acoustic zone of wet biological tissue (1600 Hz)
            // High Q creates an organic, fluid-like damped ringing effect
            _wetResonator.ConfigureBandPass(1600f, sr, 4.5f);

            // Sample-rate independent envelope coefficients using float-native math
            _envAttackCoef = Mathf.Exp(-1000f / (5f * sr));   // 5ms attack
            _envReleaseCoef = Mathf.Exp(-1000f / (60f * sr)); // 60ms release

            _envelope = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            // Global intensity scalar based on preset amount
            float crackleIntensity = _amount * 0.35f;

            // Caching volatile fields and the value-type biquad filter struct locally into CPU registers.
            // Eliminates L1/L2 cache pointer chasing to yield pure performance on high-frequency streaming threads.
            float localEnvelope = _envelope;
            uint localLcgState = _lcgState;
            BiquadFilter localFilter = _wetResonator;

            float attCoef = _envAttackCoef;
            float relCoef = _envReleaseCoef;
            float amtScalar = _amount;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Track voice amplitude envelope using our fluent extension methods
                float absInput = drySample.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = attCoef * localEnvelope + (1f - attCoef) * absInput;
                }
                else
                {
                    localEnvelope = relCoef * localEnvelope + (1f - relCoef) * absInput;
                }

                // 2. Ultra-fast bitwise LCG Random Number Generator (1 CPU cycle cost execution)
                localLcgState = localLcgState * 1103515245 + 12345;

                // 3. Envelope-driven stochastic trigger threshold
                // Higher vocal volume exponentially scales the probability of a tissue snap
                float triggerChance = 0.0005f + (localEnvelope * 0.045f * amtScalar);
                uint maxThreshold = (uint)(triggerChance * uint.MaxValue);

                float impulse = 0f;
                if (localLcgState < maxThreshold)
                {
                    // Generate a rapid, high-energy bidirectional spike
                    // Re-use LCG bits for internal bipolar amplitude decoration cleanly
                    float randSign = ((localLcgState & 0x100) != 0) ? 1f : -1f;
                    impulse = randSign * (0.3f + (localEnvelope * 0.7f));
                }

                // 4. Excite the biological resonator filter inside local stack struct register memory space
                float wetTexture = localFilter.Process(impulse);

                // 5. Accumulate the wet-flesh crackle layer into the original audio stream buffer
                pcm[i] = drySample + (wetTexture * crackleIntensity);
            }

            // Flush computed local variables back into target object configuration instance states.
            _envelope = localEnvelope;
            _lcgState = localLcgState;
            _wetResonator = localFilter;
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
    }
}