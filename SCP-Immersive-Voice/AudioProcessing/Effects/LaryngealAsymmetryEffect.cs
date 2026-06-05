namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Custom Psychoacoustic Uncanny Valley Generator for SCP-939.
    /// Simulates biological larynx asymmetry by splitting the vocal stream into two parallel paths 
    /// and introducing a sub-millisecond dynamic phase/delay drift. Zero heap allocations.
    /// </summary>
    public class LaryngealAsymmetryEffect : IAudioEffect
    {
        public string Name => "Laryngeal Asymmetry";

        private readonly float _amount;
        private readonly float _sampleRate;

        // Circular delay line for the parallel asymmetrical vocal tract path
        private const int BufferSize = 512;
        private const int BufferMask = BufferSize - 1;
        private readonly float[] _delayBuffer = new float[BufferSize];
        private int _writePtr = 0;

        // Stateful LFO phase tracker
        private float _lfoPhase = 0f;
        private readonly float _lfoIncrement;
        private const float TwoPi = (float)(Math.PI * 2.0);

        /// <summary>
        /// Initializes a new instance of the <see cref="LaryngealAsymmetryEffect"/> class.
        /// </summary>
        /// <param name="amount">The intensity of the uncanny asymmetry shift (0.0f to 1.0f).</param>
        /// <param name="sampleRate">The audio engine sample rate.</param>
        public LaryngealAsymmetryEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // Slow, organic tissue drift rate (approx 5.8 Hz wobble)
            _lfoIncrement = 5.8f / _sampleRate;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            float wetMix = _amount * 0.55f;
            float dryMix = 1f - (wetMix * 0.35f);

            // Bounded delay lengths: 0.15ms baseline up to 1.45ms displacement
            float baseDelaySamples = _sampleRate * 0.00015f;
            float maxModulationSamples = _sampleRate * 0.0013f;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Write the current active sample to cyclic storage
                _delayBuffer[_writePtr] = dry;

                // Advance slow organic asymmetry LFO phase
                _lfoPhase += _lfoIncrement;
                if (_lfoPhase > TwoPi) _lfoPhase -= TwoPi;

                float wobble = (float)Math.Sin(_lfoPhase);

                // Calculate sub-millisecond fractional delay path
                float targetDelay = baseDelaySamples + ((0.5f + 0.5f * wobble) * maxModulationSamples);
                float readPos = _writePtr - targetDelay;
                while (readPos < 0f) readPos += BufferSize;

                // Perform sub-sample linear interpolation to prevent dynamic splicing clicks
                int i0 = (int)readPos;
                int i1 = (i0 + 1) & BufferMask;
                float fraction = readPos - i0;
                float delayedSample = _delayBuffer[i0 & BufferMask] * (1f - fraction) + _delayBuffer[i1] * fraction;

                // Increment write index
                _writePtr = (_writePtr + 1) & BufferMask;

                // Blend the symmetrical real-world vocal with the asymmetric predator displacement
                pcm[i] = (dry * dryMix) + (delayedSample * wetMix * 1.35f);
            }
        }
    }
}