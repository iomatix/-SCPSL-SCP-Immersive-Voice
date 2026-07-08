using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Voice-Reactive Breath Noise Simulator.
    /// Combines an ultra-fast local LCG randomizer, vocal envelope tracking, and a slow 
    /// physiological LFO into a custom Biquad Bandpass filter to synthesize organic airflow turbulence.
    /// </summary>
    public class BreathNoiseEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _intensity;
        private readonly float _sampleRate;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;
        private readonly float _lfoIncrement;

        // Stack-allocated Biquad filter representing the physical throat/mouth air cavity
        private BiquadFilter _airCavityFilter;

        // Stateful tracking parameters
        private uint _lcgState;
        private float _envelope;
        private float _lfoPhase;
        #endregion

        #region Public Metadata Properties
        public string Name => "Breath Noise";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Breath Noise effect.
        /// </summary>
        /// <param name="intensity">Global scaling factor of the breath layer (0.0f to 2.0f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public BreathNoiseEffect(float intensity, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the argument payload
            _intensity = intensity.Clamp(0f, 2f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // Isolate local LCG state per player instance to achieve 100% thread safety
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Configure throat resonance bandpass at 1800Hz with low Q for smooth, wide breath rush
            _airCavityFilter.ConfigureBandPass(1800f, _sampleRate, 0.5f);

            // Sample-rate independent smooth envelope multipliers
            _envAttackCoef = Mathf.Exp(-1000f / (8f * _sampleRate));   // 8ms attack
            _envReleaseCoef = Mathf.Exp(-1000f / (70f * _sampleRate)); // 70ms release

            // Slow physiological lung movement LFO (approx. 0.25Hz cycle = 1 breath every 4 seconds)
            _lfoIncrement = (TwoPi * 0.25f) / _sampleRate;

            _envelope = 0f;
            _lfoPhase = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _intensity < 0.01f) return;

            float baseGain = _intensity * 0.15f;


            // Caching volatile instance fields into stack-local registers before entering the processing sequence.
            // This grants the CPU 100% register-level access speed, avoiding continuous memory store/load delays.
            float localEnvelope = _envelope;
            float localLfoPhase = _lfoPhase;
            uint localLcgState = _lcgState;
            float attCoef = _envAttackCoef;
            float relCoef = _envReleaseCoef;
            float lfoInc = _lfoIncrement;
            BiquadFilter localFilter = _airCavityFilter;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Voice envelope follower (modulates air rush density dynamically during speech)
                float absInput = drySample.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = attCoef * localEnvelope + (1f - attCoef) * absInput;
                }
                else
                {
                    localEnvelope = relCoef * localEnvelope + (1f - relCoef) * absInput;
                }

                // 2. Advance physiological lung LFO cycle
                localLfoPhase += lfoInc;
                if (localLfoPhase > TwoPi)
                    localLfoPhase -= TwoPi;

                float lungSwell = 0.7f + 0.3f * Mathf.Sin(localLfoPhase);

                // 3. Ultra-fast thread-isolated LCG white noise generation
                localLcgState = localLcgState * 1103515245 + 12345;
                float rawNoise = ((float)(localLcgState & 0xFFFF) / 65535f) * 2f - 1f;

                // 4. Shape the white noise through the acoustic throat biquad filter
                float shapedAir = localFilter.Process(rawNoise);

                // 5. Dual-modulation matrix combining voice effort and underlying breathing rhythm
                float dynamicModulation = localEnvelope * 0.7f + 0.3f * lungSwell;
                float synthesizedBreath = shapedAir * dynamicModulation * baseGain;

                // 6. Clean summation into the target in-place buffer (Soft-Limiter handles safety later)
                pcm[i] = drySample + synthesizedBreath;
            }

            // Atomically flush computed register values back into instance tracking storage fields post execution.
            _envelope = localEnvelope;
            _lfoPhase = localLfoPhase;
            _lcgState = localLcgState;
            _airCavityFilter = localFilter;
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