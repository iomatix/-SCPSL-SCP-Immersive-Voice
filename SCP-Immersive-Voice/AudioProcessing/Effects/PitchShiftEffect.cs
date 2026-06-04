namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA Delay-Line Crossfading Pitch Shifter (Doppler/Rotary method).
    /// Uses a circular buffer with dual read pointers and cubic interpolation.
    /// Provides completely natural pitch shifting without time-stretching or metallic artifacts.
    /// </summary>
    public class PitchShiftEffect : IAudioEffect
    {
        public string Name => "Pitch Shift";

        private float _targetPitch;
        private float _smoothPitch;

        // Ring buffer parameters
        private readonly float[] _ringBuffer;
        private readonly int _bufferMask;
        private int _writeIndex;

        // Pitch shifter parameters
        private float _phase;
        private readonly int _windowSize;
        private readonly float _sampleRate;

        // Precalculated constants
        private const float Pi2 = 2f * (float)Math.PI;

        /// <summary>
        /// Initializes the Pitch Shifter.
        /// </summary>
        /// <param name="pitch">Initial pitch ratio (1.0 is normal).</param>
        /// <param name="sampleRate">Engine sample rate (e.g., 48000 or 44100).</param>
        /// <param name="windowSizeMs">Crossfade window size. 40-50ms is standard for creature/human voices.</param>
        public PitchShiftEffect(float pitch, float sampleRate = 48000f, float windowSizeMs = 40f)
        {
            _sampleRate = sampleRate;
            _targetPitch = Clamp(pitch, 0.25f, 4f);
            _smoothPitch = _targetPitch;

            // Calculate window size in samples
            _windowSize = (int)(_sampleRate * (windowSizeMs / 1000f));

            // Force ring buffer size to the next power of 2 for ultra-fast wrapping (Bitwise AND)
            int size = 1;
            while (size < _windowSize * 2) size <<= 1;

            _ringBuffer = new float[size];
            _bufferMask = size - 1;

            _writeIndex = 0;
            _phase = 0f;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 2)
                return;

            for (int i = 0; i < length; i++)
            {
                // 1. Slower, smoother pitch transition (avoids zipper noise)
                _smoothPitch += 0.001f * (_targetPitch - _smoothPitch);

                // 2. Write input to the ring buffer
                _ringBuffer[_writeIndex] = pcm[i];

                // 3. Calculate phase increment based on pitch ratio.
                // d(Delay)/dt = 1 - Pitch
                float phaseInc = (1f - _smoothPitch) / _windowSize;
                _phase += phaseInc;

                // Wrap phase strictly between 0.0 and 1.0
                while (_phase >= 1f) _phase -= 1f;
                while (_phase < 0f) _phase += 1f;

                // 4. Calculate delay times (in samples) for the two read heads (180 degrees out of phase)
                float delayA = _phase * _windowSize;
                float phaseB = (_phase + 0.5f) % 1f;
                float delayB = phaseB * _windowSize;

                // 5. Read from both heads using Cubic Hermite Spline interpolation
                float tapA = ReadCubic(delayA);
                float tapB = ReadCubic(delayB);

                // 6. Calculate crossfade weights using a Hann window for constant power
                float weightA = 0.5f - 0.5f * (float)Math.Cos(_phase * Pi2);
                float weightB = 0.5f - 0.5f * (float)Math.Cos(phaseB * Pi2);

                // 7. Sum the output and write directly to PCM array
                pcm[i] = (tapA * weightA) + (tapB * weightB);

                // 8. Advance the write head using bitwise mask for zero-cost wrapping
                _writeIndex = (_writeIndex + 1) & _bufferMask;
            }
        }

        /// <summary>
        /// Updates the target pitch at runtime.
        /// </summary>
        public void SetPitch(float pitch)
        {
            _targetPitch = Clamp(pitch, 0.25f, 4f);
        }

        /// <summary>
        /// Reads a fractional delay from the ring buffer using 4-point cubic interpolation.
        /// Completely eliminates the metallic/aliasing sound of linear interpolation.
        /// </summary>
        private float ReadCubic(float delay)
        {
            // Calculate absolute read position
            float readPos = _writeIndex - delay;

            // Handle negative wrap-around
            if (readPos < 0f)
                readPos += _ringBuffer.Length;

            int i0 = (int)readPos;
            float frac = readPos - i0;

            // Get 4 adjacent samples
            int idxM1 = (i0 - 1) & _bufferMask;
            int idx0 = i0 & _bufferMask;
            int idx1 = (i0 + 1) & _bufferMask;
            int idx2 = (i0 + 2) & _bufferMask;

            float yM1 = _ringBuffer[idxM1];
            float y0 = _ringBuffer[idx0];
            float y1 = _ringBuffer[idx1];
            float y2 = _ringBuffer[idx2];

            // Evaluate Cubic Hermite Spline
            float a = -0.5f * yM1 + 1.5f * y0 - 1.5f * y1 + 0.5f * y2;
            float b = yM1 - 2.5f * y0 + 2f * y1 - 0.5f * y2;
            float c = -0.5f * yM1 + 0.5f * y1;
            float d = y0;

            return ((a * frac + b) * frac + c) * frac + d;
        }
    }
}