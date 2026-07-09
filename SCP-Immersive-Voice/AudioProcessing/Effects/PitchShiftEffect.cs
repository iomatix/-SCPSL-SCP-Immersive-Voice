using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Delay-Line Crossfading Pitch Shifter (Doppler/Rotary method).
    /// Uses a circular buffer with dual read pointers and cubic Hermite spline interpolation.
    /// Provides completely natural pitch shifting without time-stretching or metallic artifacts.
    /// </summary>
    public class PitchShiftEffect : IAdjustableAudioEffect, IResettableAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private readonly float _sampleRate;
        private readonly int _windowSize;
        private readonly int _bufferMask;
        private readonly float[] _ringBuffer;

        private float _targetPitch;

        // Stateful parameters synchronized via local stack register windows
        private float _smoothPitch;
        private float _phase;
        private int _writeIndex;
        #endregion

        #region Public Metadata Properties
        public string Name => "Pitch Shift";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Pitch Shifter.
        /// </summary>
        /// <param name="pitch">Initial pitch ratio (1.0 is normal).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        /// <param name="windowSizeMs">Crossfade window size. 40-50ms is standard for creature/human voices.</param>
        public PitchShiftEffect(float pitch, float sampleRate, float windowSizeMs = 40f)
        {
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // FLUENT API ALIGNMENT: Enforcing safe operational bounds straight via math extensions
            _targetPitch = pitch.Clamp(0.25f, 4f);
            _smoothPitch = _targetPitch;

            // Calculate window size in samples
            _windowSize = (int)(_sampleRate * (windowSizeMs / 1000f));
            if (_windowSize < 64)
                _windowSize = 64;

            // Force ring buffer size to the next power of 2 for ultra-fast bitwise wrapping (& mask)
            int size = 1;
            while (size < _windowSize * 2)
                size <<= 1;

            _ringBuffer = new float[size];
            _bufferMask = size - 1;

            _writeIndex = 0;
            _phase = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 2) return;

            // Cache volatile pointers and structural states onto the CPU stack frame.
            // Bypasses persistent heap/sterta reference chasing completely across the audio stream block.
            float localPhase = _phase;
            float localSmoothPitch = _smoothPitch;
            int localWriteIndex = _writeIndex;

            float target = _targetPitch;
            int winSize = _windowSize;
            int mask = _bufferMask;
            float[] buf = _ringBuffer;
            int bufLen = buf.Length;

            for (int i = 0; i < length; i++)
            {
                // 1. Smooth pitch transitions linearly to avoid severe digital zipper noise
                localSmoothPitch += 0.005f * (target - localSmoothPitch);

                // 2. Write active input sample to cyclic storage
                buf[localWriteIndex] = pcm[i];

                // 3. Calculate phase increment based on pitch ratio trajectory
                float phaseInc = (1f - localSmoothPitch) / winSize;
                localPhase += phaseInc;

                // PERFORMANCE FIX: Eradicated high-overhead while loops. 
                // Since phaseInc is a tiny fractional sample offset, a single conditional branch is completely safe.
                if (localPhase >= 1f)
                    localPhase -= 1f;
                else if (localPhase < 0f)
                    localPhase += 1f;

                // 4. Calculate delay times (in samples) for the parallel dual-head crossfade layout
                float delayA = localPhase * winSize;
                float phaseB = localPhase + 0.5f;
                if (phaseB >= 1f)
                    phaseB -= 1f; // Fast branch-optimized modulo alternative

                float delayB = phaseB * winSize;

                // 5. Read from both heads using 4-point Cubic Hermite Spline interpolation (Aggressive Inlined)
                float readPosA = localWriteIndex - delayA;
                if (readPosA < 0f)
                    readPosA += bufLen;

                float tapA = DiscreteCubicRead(buf, readPosA, mask);

                float readPosB = localWriteIndex - delayB;
                if (readPosB < 0f)
                    readPosB += bufLen;

                float tapB = DiscreteCubicRead(buf, readPosB, mask);

                // 6. Calculate crossfade weights using a float-native Hann window for pristine constant power
                float weightA = 0.5f - 0.5f * Mathf.Cos(localPhase * TwoPi);
                float weightB = 0.5f - 0.5f * Mathf.Cos(phaseB * TwoPi);

                // 7. Sum the output and validate float stability via fluent extensions
                float mixed = (tapA * weightA) + (tapB * weightB);

                pcm[i] = mixed.IsNanOrInfinity() ? 0f : mixed;

                // 8. Advance the write head using bitwise mask for zero-cost wrapping boundaries
                localWriteIndex = (localWriteIndex + 1) & mask;
            }

            // Write computed local variables back into object persistent instance tracking storage fields.
            _phase = localPhase;
            _smoothPitch = localSmoothPitch;
            _writeIndex = localWriteIndex;
        }
        #endregion

        #region Operational Mutations
        /// <summary>
        /// Updates the target pitch at runtime.
        /// </summary>
        public void SetPitch(float pitch) => _targetPitch = pitch.Clamp(0.25f, 4f);
        #endregion

        #region Internal High-Performance Mathematical Interpolators
        /// <summary>
        /// Reads a fractional delay from the ring buffer using an optimized 4-point cubic Hermite spline interpolation.
        /// Fully inlined by the JIT compiler to eliminate stack frame instantiation costs entirely.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float DiscreteCubicRead(float[] buffer, float readPos, int mask)
        {
            int i0 = (int)readPos;
            float frac = readPos - i0;

            // Resolve 4 adjacent discrete sample index locations via bitwise masking boundaries
            int idxM1 = (i0 - 1) & mask;
            int idx0 = i0 & mask;
            int idx1 = (i0 + 1) & mask;
            int idx2 = (i0 + 2) & mask;

            float yM1 = buffer[idxM1];
            float y0 = buffer[idx0];
            float y1 = buffer[idx1];
            float y2 = buffer[idx2];

            // Evaluate Cubic Hermite Spline Matrix Coefficients
            float a = -0.5f * yM1 + 1.5f * y0 - 1.5f * y1 + 0.5f * y2;
            float b = yM1 - 2.5f * y0 + 2f * y1 - 0.5f * y2;
            float c = -0.5f * yM1 + 0.5f * y1;
            float d = y0;

            // Clamped Horner's scheme calculation structure
            return ((a * frac + b) * frac + c) * frac + d;
        }
        #endregion

        #region Operational Parameter Adjustments
        public void AdjustParameter(float value)
        {
            SetPitch(value);
        }
        #endregion

        public void ResetState()
        {
            Array.Clear(_ringBuffer, 0, _ringBuffer.Length);
            _writeIndex = 0;
            _phase = 0f;
            _smoothPitch = _targetPitch;
        }

    }
}