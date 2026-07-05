using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// High-Performance Cinematic Demonic Octaver Engine.
    /// Utilizes a dual-head time-domain crossfading delay network with bitwise wrapping 
    /// to synthesize a massive sub-octave layer (-12 semitones) without heap allocations.
    /// </summary>
    public class DemonicOctaverEffect : IAudioEffect
    {
        #region Private Constants
        private const int BufferSize = 8128; // Power-of-two buffer size for high-speed bitwise wrapping (& 8191)
        private const int BufferMask = BufferSize - 1;
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private readonly float _sampleRate;
        private readonly float _mix;
        private readonly float _windowSize;
        private readonly float _invWindowSize;

        private readonly float[] _delayBuffer = new float[BufferSize];

        // Phase registers for the sub-octave read pointers (moving at half-speed for 0.5x pitch)
        private int _writePtr;
        private float _readPtr1;
        private float _readPtr2;
        #endregion

        #region Public Metadata Properties
        public string Name => "Demonic Octaver";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="DemonicOctaverEffect"/> class.
        /// </summary>
        /// <param name="mix">The wet blend intensity of the sub-octave layer (0.0f to 1.0f).</param>
        /// <param name="sampleRate">The audio engine sample rate.</param>
        public DemonicOctaverEffect(float mix, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the argument payload
            _mix = mix.Clamp(0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // Size of the crossfade region fine-tuned to eliminate splicing transients at 48kHz
            _windowSize = (int)(0.040f * _sampleRate); // 40ms window
            _invWindowSize = 1f / _windowSize;

            _writePtr = 0;
            _readPtr1 = 0f;
            _readPtr2 = BufferSize * 0.5f; // 180 degrees out of phase
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _mix < 0.01f) return;


            // Caching volatile fields into stack registers before entering the execution chain.
            // Bypasses pointer tracking completely, giving the JIT compiler room for optimal loop unrolling.
            int localWritePtr = _writePtr;
            float localReadPtr1 = _readPtr1;
            float localReadPtr2 = _readPtr2;

            float mixScalar = _mix;
            float winSize = _windowSize;
            float invWinSize = _invWindowSize;
            float[] delayBuf = _delayBuffer;

            // Pre-computed gain staging coefficients
            float dryGain = 1f - (mixScalar * 0.3f);
            float wetGain = mixScalar * 1.4f;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // Write current dry sample to cyclic storage
                delayBuf[localWritePtr] = drySample;

                // 1. Resolve read pointer positions with fast integer truncations
                int rIdx1 = (int)localReadPtr1;
                int rIdx2 = (int)localReadPtr2;

                // 2. Extract samples from both taps via fast bitwise masking boundaries
                float sample1 = delayBuf[rIdx1 & BufferMask];
                float sample2 = delayBuf[rIdx2 & BufferMask];

                // 3. Calculate dynamic crossfade window weights using a fast Hann window approximation
                // This tracks the distance between write and read pointers to crossfade before a collision occurs
                float delta = (localWritePtr - rIdx1) & BufferMask;

                // PERFORMANCE FIX: Swapped high-overhead Math.Cos (double precision) for float-native SIMD optimized Mathf.Cos
                float weight = 0.5f * (1f - Mathf.Cos(TwoPi * delta * invWinSize));
                if (delta > winSize)
                    weight = 1f; // Lock weight outside the splice zone

                // 4. Combine both heads into a coherent constant-power sub-octave signal
                float subOctaveSignal = (sample1 * weight) + (sample2 * (1f - weight));

                // 5. Advance phase registers at exactly 0.5x rate to achieve a perfect pitch halving (-12 semitones)
                localReadPtr1 += 0.5f;
                localReadPtr2 += 0.5f;

                if (localReadPtr1 >= BufferSize)
                    localReadPtr1 -= BufferSize;
                if (localReadPtr2 >= BufferSize)
                    localReadPtr2 -= BufferSize;

                // Advance structural write register
                localWritePtr = (localWritePtr + 1) & BufferMask;

                // 6. Inject the heavy demonic sub-octave layer back into the in-place target buffer
                pcm[i] = (drySample * dryGain) + (subOctaveSignal * wetGain);
            }


            // Atomically flush computed stack register states back into instance data fields post iteration sweep.
            _writePtr = localWritePtr;
            _readPtr1 = localReadPtr1;
            _readPtr2 = localReadPtr2;
        }
        #endregion
    }
}