using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// SCP-939 biological vocal camouflage layer.
    /// Simulates turbulent airflow, throat cavity friction, wet tissue resonance, and predatory breathing texture.
    /// </summary>
    public sealed class PredatoryCamouflageEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _amount;
        private readonly float _sampleRate;
        private readonly float _attackCoef;
        private readonly float _releaseCoef;

        // Stateful tracking parameters synchronized via local stack register windows
        private uint _noiseState;
        private float _envelope;
        private float _pink1;
        private float _pink2;
        private float _pink3;

        // Cascaded structural band resonators
        private BiquadFilter _throatBand;
        private BiquadFilter _salivaBand;
        private BiquadFilter _airBand;
        #endregion

        #region Public Metadata Properties
        public string Name => "Predatory Camouflage";
        #endregion

        #region Initialization
        public PredatoryCamouflageEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Enforcing clean limits using math extensions straight on initialization
            _amount = amount.Clamp(0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;
            _noiseState = (uint)Guid.NewGuid().GetHashCode();

            // PERFORMANCE FIX: Trigonometry mappings migrated straight to float-native structures
            _attackCoef = Mathf.Exp(-1000f / (4f * _sampleRate));
            _releaseCoef = Mathf.Exp(-1000f / (80f * _sampleRate));

            // Deep throat friction biquad resonance configuration
            _throatBand.ConfigureBandPass(850f, _sampleRate, 0.7f);

            // Wet mouth tissue biquad resonance configuration
            _salivaBand.ConfigureBandPass(1800f, _sampleRate, 0.8f);

            // Air turbulence biquad resonance configuration
            _airBand.ConfigureBandPass(4200f, _sampleRate, 0.6f);

            _envelope = 0f;
            _pink1 = 0f;
            _pink2 = 0f;
            _pink3 = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.001f) return;

            // Extracted the static mix coefficient calculation outside the hot-path sweep block.
            float targetAmount = _amount;

            // Cache volatile parameters, noise registers and structures directly onto the stack.
            // Bypasses persistent RAM tracking loops completely to secure native silicon processing speeds.
            float localEnvelope = _envelope;
            uint localNoiseState = _noiseState;
            float p1 = _pink1, p2 = _pink2, p3 = _pink3;

            BiquadFilter fThroat = _throatBand;
            BiquadFilter fSaliva = _salivaBand;
            BiquadFilter fAir = _airBand;

            float att = _attackCoef;
            float rel = _releaseCoef;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // 1. Dynamic voice envelope tracking via fluent primitives
                float absInput = dry.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = att * localEnvelope + (1f - att) * absInput;
                }
                else
                {
                    localEnvelope = rel * localEnvelope + (1f - rel) * absInput;
                }

                // 2. High-speed LCG white noise calculation block
                localNoiseState = localNoiseState * 1664525u + 1013904223u;
                float white = (((localNoiseState >> 8) & 0x00FFFFFF) / 8388607.5f) - 1f;

                // 3. Lossy multi-pole Voss-McCartney pink noise filtering array
                p1 = 0.9975f * p1 + white * 0.099f;
                p2 = 0.9850f * p2 + white * 0.050f;
                p3 = 0.9500f * p3 + white * 0.020f;
                float pink = (p1 + p2 + p3 + white * 0.15f) * 0.55f;

                // 4. Parallel extraction across structural local struct filters
                float throat = fThroat.Process(pink);
                float saliva = fSaliva.Process(pink);
                float air = fAir.Process(pink);

                // 5. Build biological creature layer and modulate via voice effort envelope
                float creatureLayer = (throat * 0.45f + saliva * 0.25f + air * 0.30f) * (localEnvelope * 1.6f);

                // 6. Preserve conversational speech identity parameters cleanly
                float speechResidual = dry * 0.35f;

                pcm[i] = dry + (creatureLayer + speechResidual) * targetAmount;
            }

            // Safely restore computed stack bounds back into class tracking persistent structures.
            _envelope = localEnvelope;
            _noiseState = localNoiseState;
            _pink1 = p1; _pink2 = p2; _pink3 = p3;
            _throatBand = fThroat; _salivaBand = fSaliva; _airBand = fAir;
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
            _amount = value.Clamp(0f, 1f);
        }
        #endregion
    }
}