using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Dry, brittle crackle simulating bone friction, snapping joints, or decayed tissue.
    /// Employs an ultra-fast bitwise LCG randomizer, amplitude envelope tracking, 
    /// and a high-frequency Biquad High-Pass filter to isolate crisp transients. Zero allocations.
    /// </summary>
    public class DryCrackleEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _amount;
        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        // Stack-allocated High-Pass filter to strip body and leave only crisp, dry snaps
        private BiquadFilter _brittleFilter;

        // Stateful parameters for LCG and envelope tracking managed via local stack registers
        private float _envelope;
        private uint _lcgState;
        #endregion

        #region Public Metadata Properties
        public string Name => "Dry Crackle";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Dry Crackle effect.
        /// </summary>
        /// <param name="amount">Density and volume of the dry crackle layer (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public DryCrackleEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the argument payload
            _amount = amount.Clamp(0f, 1.5f);
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            // Seed the fast LCG using a unique instance hash
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Configure the filter as a High-Pass at 4000Hz to enforce a dusty, brittle texture
            _brittleFilter.ConfigureHighPass(4000f, sr, 1.0f);

            // Sample-rate independent envelope tracking
            _envAttackCoef = Mathf.Exp(-1000f / (4f * sr));   // 4ms attack
            _envReleaseCoef = Mathf.Exp(-1000f / (50f * sr)); // 50ms release

            _envelope = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            float crackleIntensity = _amount * 0.28f;

            // Caching volatile instance fields and value-type filters straight into local memory registers.
            // Eradicates pointer chasing across the heap layout, unlocking peak performance on high-frequency voice threads.
            float localEnvelope = _envelope;
            uint localLcgState = _lcgState;
            BiquadFilter localFilter = _brittleFilter;

            float attCoef = _envAttackCoef;
            float relCoef = _envReleaseCoef;
            float amtScalar = _amount;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Track voice amplitude envelope via custom math extensions
                float absInput = drySample.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = attCoef * localEnvelope + (1f - attCoef) * absInput;
                }
                else
                {
                    localEnvelope = relCoef * localEnvelope + (1f - relCoef) * absInput;
                }

                // 2. Ultra-fast bitwise LCG Randomizer (1 CPU cycle cost execution)
                localLcgState = localLcgState * 1103515245 + 12345;

                // 3. Stochastic trigger threshold driven exponentially by vocal envelope
                float triggerChance = 0.0003f + (localEnvelope * 0.038f * amtScalar);
                uint maxThreshold = (uint)(triggerChance * uint.MaxValue);

                float impulse = 0f;
                if (localLcgState < maxThreshold)
                {
                    // Generate a sharp, instantaneous bipolar click using highly optimized bitwise sign checks
                    float randSign = ((localLcgState & 0x200) != 0) ? 1f : -1f;
                    impulse = randSign * (0.4f + (localEnvelope * 0.6f));
                }

                // 4. Filter the impulse through local register value-type space to retain only the high-frequency brittle "snap"
                float dryTexture = localFilter.Process(impulse);

                // 5. Inject the dry bone/crackle layer into the primary stream
                pcm[i] = drySample + (dryTexture * crackleIntensity);
            }

            // Flush computed local stack structures back into target class memory state storage fields.
            _envelope = localEnvelope;
            _lcgState = localLcgState;
            _brittleFilter = localFilter;
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
            _amount = value.Clamp(0f, 1.5f);
        }
        #endregion
    }
}