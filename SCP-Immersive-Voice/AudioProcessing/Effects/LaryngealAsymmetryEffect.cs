using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Custom Psychoacoustic Uncanny Valley Generator for SCP-939.
    /// Simulates biological larynx asymmetry by splitting the vocal stream into two parallel paths 
    /// and introducing a sub-millisecond dynamic phase/delay drift. Zero heap allocations.
    /// </summary>
    public class LaryngealAsymmetryEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const int BufferSize = 512;
        private const int BufferMask = BufferSize - 1;
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _amount;
        private readonly float _sampleRate;
        private readonly float _lfoIncrement;

        // Circular delay line for the parallel asymmetrical vocal tract path
        private readonly float[] _delayBuffer = new float[BufferSize];

        // Stateful trackers synchronized via local stack frames
        private int _writePtr;
        private float _lfoPhase;
        #endregion

        #region Public Metadata Properties
        public string Name => "Laryngeal Asymmetry";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="LaryngealAsymmetryEffect"/> class.
        /// </summary>
        /// <param name="amount">The intensity of the uncanny asymmetry shift (0.0f to 1.0f).</param>
        /// <param name="sampleRate">The audio engine sample rate.</param>
        public LaryngealAsymmetryEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the float primitive
            _amount = amount.Clamp(0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // MATHEMATICAL BUG FIX: Included the angular frequency scalar multiplication (TwoPi)
            // to align the real-time modulation target directly to an accurate 5.8 Hz organic tissue wobble.
            _lfoIncrement = (TwoPi * 5.8f) / _sampleRate;

            _writePtr = 0;
            _lfoPhase = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            float wetMix = _amount * 0.55f;
            float dryMix = 1f - (wetMix * 0.35f);

            // Pre-computed gain staging coefficients to save registers inside the loop block
            float wetGainFactor = wetMix * 1.35f;

            // Bounded delay lengths: 0.15ms baseline up to 1.45ms displacement
            float baseDelaySamples = _sampleRate * 0.00015f;
            float maxModulationSamples = _sampleRate * 0.0013f;

            // Cache volatile pointer instance markers straight onto local CPU registers.
            // Bypasses pointer tracking loops completely across high-frequency real-time packet processing.
            int localWritePtr = _writePtr;
            float localLfoPhase = _lfoPhase;

            float lfoInc = _lfoIncrement;
            float[] delayBuf = _delayBuffer;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Write the current active sample to cyclic storage
                delayBuf[localWritePtr] = dry;

                // Advance slow organic asymmetry LFO phase
                localLfoPhase += lfoInc;
                if (localLfoPhase > TwoPi)
                    localLfoPhase -= TwoPi;

                // PERFORMANCE FIX: Swapped double precision Math.Sin for float-native Mathf.Sin
                float wobble = Mathf.Sin(localLfoPhase);

                // Calculate sub-millisecond fractional delay path
                float targetDelay = baseDelaySamples + ((0.5f + 0.5f * wobble) * maxModulationSamples);
                float readPos = localWritePtr - targetDelay;

                // PERFORMANCE FIX: Eradicated high-overhead while loop execution.
                // Replaced with a streamlined binary conditional offset addition.
                if (readPos < 0f)
                    readPos += BufferSize;

                // Perform sub-sample linear interpolation to prevent dynamic splicing clicks
                int i0 = (int)readPos;
                int i1 = (i0 + 1) & BufferMask;
                float fraction = readPos - i0;
                float delayedSample = delayBuf[i0 & BufferMask] * (1f - fraction) + delayBuf[i1] * fraction;

                // Increment write index securely using fast bitwise masking boundaries
                localWritePtr = (localWritePtr + 1) & BufferMask;

                // Blend the symmetrical real-world vocal with the asymmetric predator displacement
                pcm[i] = (dry * dryMix) + (delayedSample * wetGainFactor);
            }

            // Safely restore calculated stack register data boundaries back into class tracking parameters.
            _writePtr = localWritePtr;
            _lfoPhase = localLfoPhase;
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