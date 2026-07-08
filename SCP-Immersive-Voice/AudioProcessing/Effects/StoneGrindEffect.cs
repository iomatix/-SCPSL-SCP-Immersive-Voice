using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Physical Modeling Stick-Slip Stone Grinding and Friction Engine for SCP-173.
    /// Replaces continuous thermal white noise with a discrete interlocking crystal grid shear matrix.
    /// Uses asymmetric ring modulation to turn human speech into jagged rock grit.
    /// </summary>
    public class StoneGrindEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private readonly float _intensity;
        private readonly float _sampleRate;
        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;
        private readonly float _lfoIncrement;

        // Structural internal biquad filters
        private BiquadFilter _stoneRumbleFilter;
        private BiquadFilter _stoneGritFilter;

        // Stateful tracking parameters synchronized via local stack registers
        private uint _lcgState;
        private float _envelope;
        private float _frictionLfoPhase;
        #endregion

        #region Public Metadata Properties
        public string Name => "Stone Grind";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the tectonic Stone Grind engine.
        /// </summary>
        public StoneGrindEffect(float intensity, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Enforcing clean limits using math extensions straight on initialization
            _intensity = intensity.Clamp(0f, 2f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Sub-bass tectonic structural displacement rumble (110Hz)
            _stoneRumbleFilter.ConfigureBandPass(110f, _sampleRate, 5.0f);

            // Dense, crushing aggregate rock-grain abrasive scratch (750Hz)
            _stoneGritFilter.ConfigureBandPass(750f, _sampleRate, 1.8f);

            // PERFORMANCE FIX: Trigonometry coefficients mapped directly into float-native structures
            _envAttackCoef = Mathf.Exp(-1000f / (15f * _sampleRate));
            _envReleaseCoef = Mathf.Exp(-1000f / (110f * _sampleRate));

            _lfoIncrement = 4.2f / _sampleRate; // 4.2 Hz surface macro-fault rate

            _envelope = 0f;
            _frictionLfoPhase = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _intensity < 0.01f) return;

            float frictionGainFactor = _intensity * 0.65f;

            // Cache volatile parameters, noise states, and filters straight onto local CPU registers.
            // Bypasses persistent pointer memory lines to secure raw execution speed on high-frequency threads.
            float localEnvelope = _envelope;
            float localFrictionLfoPhase = _frictionLfoPhase;
            uint localLcgState = _lcgState;

            BiquadFilter fRumble = _stoneRumbleFilter;
            BiquadFilter fGrit = _stoneGritFilter;

            float att = _envAttackCoef;
            float rel = _envReleaseCoef;
            float lfoInc = _lfoIncrement;
            float intense = _intensity;

            // Pre-computed constant for binary shear mapping
            uint slipThreshold = (uint)(0.22f * uint.MaxValue);

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

                // 2. Advance structural linear phase accumulator smoothly
                localFrictionLfoPhase += lfoInc;
                if (localFrictionLfoPhase > 1f)
                    localFrictionLfoPhase -= 1f;

                float tri = localFrictionLfoPhase * 2f;
                if (tri > 1f)
                    tri = 2f - tri;
                float surfaceWobble = 4f * tri * (1f - tri);

                // 3. Advance fast LCG random state sequence (1 CPU cycle cost)
                localLcgState = localLcgState * 1103515245 + 12345;

                // Stick-Slip Macro Modeling:
                // Instead of streaming continuous white noise (which sounds like sand/sandpaper),
                // we model interlocking crystal ridges. Sound is only generated when micro-fault thresholds crash.
                float stickSlipSource = 0f;
                if (localLcgState < slipThreshold)
                {
                    stickSlipSource = ((float)(localLcgState & 0xFFFF) / 65535f) * 2f - 1f;
                }

                // 4. Run source nodes across independent register-cached biquads
                float massRumble = fRumble.Process(stickSlipSource);
                float surfaceGrit = fGrit.Process(stickSlipSource);

                float dynamicallyModulatedGrit = surfaceGrit * (0.3f + surfaceWobble * 0.7f);

                // Heavy 75% internal weight density blend staging
                float compositeGrind = (massRumble * 0.75f) + (dynamicallyModulatedGrit * 0.25f);
                float activeGrindLayer = compositeGrind * localEnvelope * frictionGainFactor;

                float drivenGrind = activeGrindLayer * 3.0f;
                float saturatedGrind = drivenGrind / (1f + drivenGrind.Abs());

                // PERFORMANCE FIX: Swapped out high-overhead double Math.Sin for float-native SIMD optimized Mathf.Sin
                // Asymmetric Tectonic Ring-Modulation:
                // We use a high-frequency stone lattice modulation wave driven directly by the surface wobble LFO
                // to completely disrupt human pitch components, turning vocals into mineral grinding textures.
                float mineralModulationWave = Mathf.Sin(localFrictionLfoPhase * TwoPi * 28f);

                float lithosphericVoice = dryInput * (1f - (localEnvelope * intense * 0.6f)) +
                                         (dryInput * mineralModulationWave * localEnvelope * intense * 0.5f);

                pcm[i] = lithosphericVoice + saturatedGrind;
            }

            // Flush computed register values back into object configuration persistence parameters.
            _envelope = localEnvelope;
            _frictionLfoPhase = localFrictionLfoPhase;
            _lcgState = localLcgState;
            _stoneRumbleFilter = fRumble;
            _stoneGritFilter = fGrit;
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