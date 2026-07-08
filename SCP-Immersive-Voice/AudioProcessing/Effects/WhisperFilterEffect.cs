using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Spectral Whisper Synthesizer.
    /// Removes vocal fold dominance while preserving articulation via voice-reactive 
    /// pink-noise biquad resonance tracking. Fully real-time safe and allocation-free.
    /// </summary>
    public sealed class WhisperFilterEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _amount;
        private readonly float _sampleRate;
        private readonly float _attackCoef;
        private readonly float _releaseCoef;

        // Stateful parameters synchronized via local stack register windows
        private float _envelope;
        private uint _noiseState;
        private float _pink1;
        private float _pink2;
        private float _pink3;

        // Whisper articulation filters
        private BiquadFilter _presenceBand;
        private BiquadFilter _airBand;
        #endregion

        #region Public Metadata Properties
        public string Name => "Whisper Filter";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the whisper synthesizer.
        /// </summary>
        /// <param name="amount">Whisper intensity (0.0 = bypass, 1.0 = maximum whisper transformation).</param>
        /// <param name="sampleRate">Engine sample rate.</param>
        public WhisperFilterEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Enforcing clean operational bounds straight via math extensions
            _amount = amount.Clamp(0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _noiseState = (uint)Guid.NewGuid().GetHashCode();

            // PERFORMANCE FIX: Trigonometry generation migrated straight to float-native structures
            _attackCoef = Mathf.Exp(-1000f / (5f * _sampleRate));  // Fast 5ms speech tracking attack
            _releaseCoef = Mathf.Exp(-1000f / (60f * _sampleRate)); // Slower release preserves articulation tails

            // Main articulation band: Centered at 2800Hz to capture high-mid speech presence
            _presenceBand.ConfigureBandPass(2800f, _sampleRate, 0.8f);

            // Secondary air turbulence band: Centered at 4500Hz for upper sibilant air friction
            _airBand.ConfigureBandPass(4500f, _sampleRate, 0.6f);

            _envelope = 0f;
            _pink1 = 0f; _pink2 = 0f; _pink3 = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount <= 0.001f) return;

            // Extracted constant-power blend and speech residual gains out of the loop sweep block.
            // PERFORMANCE FIX: Swapped double precision Math.Sqrt for float-native Mathf.Sqrt.
            float amount = _amount.Clamp(0f, 1f);
            float dryGain = Mathf.Sqrt(1f - amount);
            float wetGain = Mathf.Sqrt(amount);

            // Pre-computed speech residual factor to protect intelligibility without data bloat
            float residualSpeechGain = (1f - amount) * 0.18f;

            // Cache volatile properties, noise states, and filters straight onto local CPU registers.
            // Eradicates stera reference pointer chasing completely across the high-frequency VoIP block.
            float localEnvelope = _envelope;
            uint localNoiseState = _noiseState;
            float p1 = _pink1, p2 = _pink2, p3 = _pink3;

            BiquadFilter fPresence = _presenceBand;
            BiquadFilter fAir = _airBand;

            float att = _attackCoef;
            float rel = _releaseCoef;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // 1. Voice amplitude envelope follower via custom fluent extensions
                float absInput = dry.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = att * localEnvelope + (1f - att) * absInput;
                }
                else
                {
                    localEnvelope = rel * localEnvelope + (1f - rel) * absInput;
                }

                // 2. Ultra-fast local LCG for raw white noise generation (1 CPU cycle cost execution)
                localNoiseState = localNoiseState * 1664525u + 1013904223u;
                float white = (((localNoiseState >> 8) & 0x00FFFFFF) / 8388607.5f) - 1f;

                // 3. Lightweight Voss-McCartney Pink Noise Approximation matrix
                p1 = 0.9975f * p1 + white * 0.0990f;
                p2 = 0.9850f * p2 + white * 0.0500f;
                p3 = 0.9500f * p3 + white * 0.0200f;
                float pink = (p1 + p2 + p3 + white * 0.150f) * 0.55f;

                // 4. Extract parallel spectral bands within local stack structure boundaries
                float articulation = fPresence.Process(pink);
                float turbulence = fAir.Process(pink);

                // Blend articulation (70%) and air (30%) layers into a coherent whisper node
                float whisperCore = articulation * 0.70f + turbulence * 0.30f;

                // Voice-reactive modulation mapping
                float modulatedWhisper = whisperCore * (localEnvelope * 1.35f);

                // Preserve a tiny amount of speech residue utilizing pre-computed loop invariant scalars
                float residualSpeech = dry * residualSpeechGain;
                float whisperSignal = modulatedWhisper + residualSpeech;

                // 5. Constant-Power Output Blend writeback directly into the live audio array
                pcm[i] = dry * dryGain + whisperSignal * wetGain;
            }

            // Safely restore calculated stack bounds back into class tracking persistent structures atomically.
            _envelope = localEnvelope;
            _noiseState = localNoiseState;
            _pink1 = p1; _pink2 = p2; _pink3 = p3;
            _presenceBand = fPresence; _airBand = fAir;
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
                // Boundary sanity validation using float primitives
                centerFrequency = centerFrequency.Clamp(40f, sampleRate * 0.45f);
                q = q.LimitMin(0.1f);

                float w0 = TwoPi * centerFrequency / sampleRate;
                float sinW0 = Mathf.Sin(w0);
                float cosW0 = Mathf.Cos(w0);

                float alpha = sinW0 / (2f * q);
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