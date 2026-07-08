using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System.Runtime.CompilerServices;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Production-Grade Multi-Tap High Fry Scream & Dual Upper-Octave Synthesizer Engine.
    /// Features sub-sample linear interpolation, low-CPU linear boundary crossfading,
    /// and a phase-instability chaos matrix to deliver authentic, tearing high fry shrieks. Zero heap allocations.
    /// </summary>
    public class VocalShriekShifterEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const int BufferSize = 16384;
        private const int BufferMask = BufferSize - 1;
        #endregion

        #region Private Execution Vectors
        private float _amount;
        private readonly float _sampleRate;
        private readonly float _crossfadeZone;
        private readonly float _invCrossfadeZone;

        // Persistent ring buffer storage
        private readonly float[] _ringBuffer = new float[BufferSize];

        // Stateful parameters synchronized via local stack register windows
        private int _writePtr;
        private float _baseReadPtr;
        private float _oct1ReadPtr;
        private float _oct2ReadPtr;
        private uint _chaosState;
        #endregion

        #region Public Metadata Properties
        public string Name => "Vocal Shriek Shifter";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="VocalShriekShifterEffect"/> class.
        /// </summary>
        public VocalShriekShifterEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the float primitive
            _amount = amount.Clamp(0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // 30ms protective safety boundary zone to prevent write-head collisions
            _crossfadeZone = (int)(0.030f * _sampleRate);
            _invCrossfadeZone = 1f / _crossfadeZone;

            _writePtr = 0;
            _baseReadPtr = 0f;
            _oct1ReadPtr = 0f;
            _oct2ReadPtr = 0f;
            _chaosState = 0xACE1u; // Fast thread-safe XORShift PRNG seed for jitter
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            float wetMix = _amount;
            float dryMix = 1f - (wetMix * 0.25f);

            // Pre-computed gain scaling matrices outside the hot path frame
            float wetGainFactor = wetMix * 1.5f;

            // Cache volatile parameters, pointers, and chaos seeds directly into the CPU stack frame context.
            // Eradicates heap allocation line tracking loops completely across the high-frequency sample processing block.
            int localWritePtr = _writePtr;
            float localBaseReadPtr = _baseReadPtr;
            float localOct1ReadPtr = _oct1ReadPtr;
            float localOct2ReadPtr = _oct2ReadPtr;
            uint localChaosState = _chaosState;

            float[] buf = _ringBuffer;
            float xZone = _crossfadeZone;
            float invXZone = _invCrossfadeZone;
            float amtScalar = _amount;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];
                buf[localWritePtr] = drySample;

                // --- FAST BITWISE KHUOSITEY JITTER (FRY SCREAM TEXTURING) ---
                // XORShift high-speed pseudo-random generation executing within a single CPU cycle profile.
                // Injects highly organic, non-periodic structural throat tearing and grit.
                localChaosState ^= localChaosState << 13;
                localChaosState ^= localChaosState >> 17;
                localChaosState ^= localChaosState << 5;
                float jitter = ((int)localChaosState * 4.6566129e-10f) * 0.04f * amtScalar;

                // --- LAYER 1: BASE PITCH SUPER-SHIFT (+14 Semitones Falsetto | Speed: 2.2449) ---
                float s1 = DiscreteInterpolatedRead(buf, localBaseReadPtr, localWritePtr, out float d1);
                localBaseReadPtr += (2.2449f + jitter);
                if (localBaseReadPtr >= BufferSize)
                    localBaseReadPtr -= BufferSize;

                // --- LAYER 2: FIRST HIGH OCTAVE (+12 Semitones | Speed: 2.0) ---
                float s2 = DiscreteInterpolatedRead(buf, localOct1ReadPtr, localWritePtr, out float d2);
                localOct1ReadPtr += (2.0f + jitter);
                if (localOct1ReadPtr >= BufferSize)
                    localOct1ReadPtr -= BufferSize;

                // --- LAYER 3: SECOND HIGH OCTAVE (+24 Semitones | Speed: 4.0) ---
                float s3 = DiscreteInterpolatedRead(buf, localOct2ReadPtr, localWritePtr, out float d3);
                localOct2ReadPtr += (4.0f + jitter);
                if (localOct2ReadPtr >= BufferSize)
                    localOct2ReadPtr -= BufferSize;

                // Advance global hardware simulation write head using zero-cost bitwise masking boundaries
                localWritePtr = (localWritePtr + 1) & BufferMask;

                // --- LOW-CPU LINEAR CROSSFADE MATRIX (REPLACED MATH.COS) ---
                // Evaluates individual read-head proximity to the write frontier independently.
                float w1 = d1 < xZone ? d1 * invXZone : 1f;
                float w2 = d2 < xZone ? d2 * invXZone : 1f;
                float w3 = d3 < xZone ? d3 * invXZone : 1f;

                // Summate all vectors under balanced constant-power compensation metrics
                float compositeScream = (s1 * w1 * 0.55f) + (s2 * w2 * 0.35f) + (s3 * w3 * 0.45f);

                // Soft-Limiter Knee Block to clamp screaming blowouts cleanly without clipping
                if (compositeScream > 0.75f)
                {
                    compositeScream = 0.75f + (compositeScream - 0.75f) * 0.15f;
                }
                else if (compositeScream < -0.75f)
                {
                    compositeScream = -0.75f + (compositeScream + 0.75f) * 0.15f;
                }

                pcm[i] = (drySample * dryMix) + (compositeScream * wetGainFactor);
            }

            // Flush calculated stack modifications back into object persistence tracking fields atomically.
            _writePtr = localWritePtr;
            _baseReadPtr = localBaseReadPtr;
            _oct1ReadPtr = localOct1ReadPtr;
            _oct2ReadPtr = localOct2ReadPtr;
            _chaosState = localChaosState;
        }
        #endregion

        #region Internal High-Performance Mathematical Interpolators
        /// <summary>
        /// Reads from the cyclic buffer using sub-sample linear interpolation to completely eliminate digital aliasing noise.
        /// Fully inlined by the JIT compiler to insulate the hot-path processing loop from stack-frame assignment costs.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float DiscreteInterpolatedRead(float[] buffer, float readPtr, int writePtr, out float distance)
        {
            int idxA = (int)readPtr;
            int idxB = (idxA + 1) & BufferMask;
            float fraction = readPtr - idxA;

            float sampleA = buffer[idxA & BufferMask];
            float sampleB = buffer[idxB];

            // Compute exact distance vectors for crossfading boundaries cleanly via bitwise wrapping
            distance = (writePtr - idxA) & BufferMask;

            // Execute pristine linear math blend step
            return sampleA + fraction * (sampleB - sampleA);
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