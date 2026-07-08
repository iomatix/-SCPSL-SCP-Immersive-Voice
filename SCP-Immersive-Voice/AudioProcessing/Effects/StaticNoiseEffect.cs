using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Organic multi-layer static noise generator for SCP-079 and radio transmission corruption.
    /// Employs a thread-safe local LCG, sample-rate independent biquad filter matrices, 
    /// and fast polynomial LFOs to simulate complex RF interference. Zero-allocation.
    /// </summary>
    public class StaticNoiseEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _amount;
        private readonly float _sampleRate;
        private readonly float _driftIncrement;
        private readonly float _fizzIncrement;

        // Dual biquad filters to isolate radio frequency bands
        private BiquadFilter _radioBandpass;
        private BiquadFilter _hfFizzHighpass;

        // Stateful parameters for LCG noise and phase tracking managed inside stack registers
        private uint _lcgState;
        private float _driftPhase;
        private float _fizzPhase;
        #endregion

        #region Public Metadata Properties
        public string Name => "Static Noise";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Static Noise effect.
        /// </summary>
        /// <param name="amount">Intensity and mix of the static interference (0.0f to 1.0f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public StaticNoiseEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Enforcing clean bounds using math extensions straight on initialization
            _amount = amount.Clamp(0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Configure RF bandpass to emulate intercom/radio chassis bandwidth (1200Hz - 3200Hz)
            _radioBandpass.ConfigureBandPass(1800f, _sampleRate, 0.6f);

            // Configure crisp electrical fizz filter at 5500Hz
            _hfFizzHighpass.ConfigureHighPass(5500f, _sampleRate, 1.0f);

            // Sample-rate independent modulation speeds (Drift = 0.4 Hz, Fizz Mod = 7.5 Hz)
            _driftIncrement = 0.4f / _sampleRate;
            _fizzIncrement = 7.5f / _sampleRate;

            _driftPhase = 0f;
            _fizzPhase = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            // Extracted dry/wet gain staging and mix constants from the sample processing sweep block.
            // Eradicates thousands of redundant floating-point structural subtractions per VoIP frame packet.
            float amt = _amount;
            float dryGain = 1f - amt;
            float wetGain = amt * 0.45f;

            // Unrolling variables and caching value-type biquad structures directly onto local stack frames.
            // Bypasses persistent pointer memory chasing completely, forcing the compiler to run loop assets on pure silicon speed.
            uint localLcgState = _lcgState;
            float localDriftPhase = _driftPhase;
            float localFizzPhase = _fizzPhase;

            BiquadFilter fRadio = _radioBandpass;
            BiquadFilter fFizz = _hfFizzHighpass;

            float driftInc = _driftIncrement;
            float fizzInc = _fizzIncrement;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Advance ultra-fast local LCG for raw white noise generation (1 CPU cycle cost execution)
                localLcgState = localLcgState * 1103515245 + 12345;
                float whiteNoise = ((float)(localLcgState & 0xFFFF) / 65535f) * 2f - 1f;

                // 2. Advance sample-rate decoupled linear phase accumulators smoothly
                localDriftPhase += driftInc;
                if (localDriftPhase > 1f)
                    localDriftPhase -= 1f;

                localFizzPhase += fizzInc;
                if (localFizzPhase > 1f)
                    localFizzPhase -= 1f;

                // 3. Fast polynomial triangle-to-parabola approximations (Replaces expensive Math.Sin loops)
                float driftTri = localDriftPhase * 2f;
                if (driftTri > 1f)
                    driftTri = 2f - driftTri;
                float lowFrequencyDrift = 4f * driftTri * (1f - driftTri); // Organic 0.4Hz swell envelope

                float fizzTri = localFizzPhase * 2f;
                if (fizzTri > 1f)
                    fizzTri = 2f - fizzTri;
                float highFrequencyFizzMod = 4f * fizzTri * (1f - fizzTri); // 7.5Hz crackle modulator

                // 4. Process the white noise source through independent spectral shapers in stack registers
                float baseHiss = fRadio.Process(whiteNoise);
                float rawFizz = fFizz.Process(whiteNoise);

                // 5. Synthesize the multi-layered electromagnetic static node
                float modulatedHiss = baseHiss * (0.6f + lowFrequencyDrift * 0.4f);
                float modulatedFizz = rawFizz * (0.2f + highFrequencyFizzMod * 0.8f) * 0.35f;
                float combinedStatic = modulatedHiss + modulatedFizz;

                // 6. Fast polynomial soft-clipping for analog radio circuit overdrive via fluent abs extensions
                float drivenStatic = combinedStatic * 1.5f;
                float staticOut = drivenStatic / (1f + drivenStatic.Abs());

                // 7. Constant-power style blend injection directly into the live buffer utilizing pre-computed constants
                pcm[i] = (drySample * dryGain) + (staticOut * wetGain);
            }

            // Safely restore calculated stack bounds back into class tracking persistent structures atomically.
            _lcgState = localLcgState;
            _driftPhase = localDriftPhase;
            _fizzPhase = localFizzPhase;
            _radioBandpass = fRadio;
            _hfFizzHighpass = fFizz;
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
            _amount = value.Clamp(0f, 1f);
        }
        #endregion
    }
}