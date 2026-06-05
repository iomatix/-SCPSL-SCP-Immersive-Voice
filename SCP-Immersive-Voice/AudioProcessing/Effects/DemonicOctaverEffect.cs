namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    ///  High-Performance Cinematic Demonic Octaver Engine.
    /// Utilizes a dual-head time-domain crossfading delay network with bitwise wrapping 
    /// to synthesize a massive sub-octave layer (-12 semitones) without heap allocations.
    /// </summary>
    public class DemonicOctaverEffect : IAudioEffect
    {
        public string Name => "Demonic Octaver";

        private readonly float _sampleRate;
        private float _mix;

        // Power-of-two buffer size for high-speed bitwise wrapping (& 8191)
        private const int BufferSize = 8192;
        private const int BufferMask = BufferSize - 1;

        private readonly float[] _delayBuffer = new float[BufferSize];
        private int _writePtr = 0;

        // Phase registers for the sub-octave read pointers (moving at half-speed for 0.5x pitch)
        private float _readPtr1 = 0f;
        private float _readPtr2 = BufferSize * 0.5f; // 180 degrees out of phase

        private readonly float _windowSize;
        private readonly float _invWindowSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemonicOctaverEffect"/> class.
        /// </summary>
        /// <param name="mix">The wet blend intensity of the sub-octave layer (0.0f to 1.0f).</param>
        /// <param name="sampleRate">The audio engine sample rate.</param>
        public DemonicOctaverEffect(float mix, float sampleRate)
        {
            _mix = Clamp(mix, 0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // Size of the crossfade region fine-tuned to eliminate splicing transients at 48kHz
            _windowSize = (int)(0.040f * _sampleRate); // 40ms window
            _invWindowSize = 1f / _windowSize;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _mix < 0.01f) return;

            float pi = (float)Math.PI;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // Write current dry sample to cyclic storage
                _delayBuffer[_writePtr] = drySample;

                // 1. Resolve read pointer positions with integer truncations
                int rIdx1 = (int)_readPtr1;
                int rIdx2 = (int)_readPtr2;

                // 2. Extract samples from both taps
                float sample1 = _delayBuffer[rIdx1 & BufferMask];
                float sample2 = _delayBuffer[rIdx2 & BufferMask];

                // 3. Calculate dynamic crossfade window weights using a fast Hann window approximation
                // This tracks the distance between write and read pointers to crossfade before a collision occurs
                float delta = (_writePtr - rIdx1) & BufferMask;
                float weight = 0.5f * (1f - (float)Math.Cos(2f * pi * delta * _invWindowSize));
                if (delta > _windowSize) weight = 1f; // Lock weight outside the splice zone

                // 4. Combine both heads into a coherent constant-power sub-octave signal
                float subOctaveSignal = (sample1 * weight) + (sample2 * (1f - weight));

                // 5. Advance phase registers at exactly 0.5x rate to achieve a perfect pitch halving (-12 semitones)
                _readPtr1 += 0.5f;
                _readPtr2 += 0.5f;

                if (_readPtr1 >= BufferSize) _readPtr1 -= BufferSize;
                if (_readPtr2 >= BufferSize) _readPtr2 -= BufferSize;

                // Advance structural write register
                _writePtr = (_writePtr + 1) & BufferMask;

                // 6. Inject the heavy demonic sub-octave layer back into the active pot
                pcm[i] = (drySample * (1f - _mix * 0.3f)) + (subOctaveSignal * _mix * 1.4f);
            }
        }
    }
}