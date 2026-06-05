namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    ///  Wet Organic layer simulating moist, living vocal tract tissue and saliva.
    /// Employs a sub-millisecond fractional micro-delay line, sample-rate independent 
    /// biquad high-pass filtering, and a high-speed bitwise LCG randomizer. Zero allocations.
    /// </summary>
    public class WetOrganicEffect : IAudioEffect
    {
        public string Name => "Wet Organic";

        private readonly float _amount;
        private readonly float _sampleRate;

        // Ultra-short power-of-two ring buffer for micro-delay (128 samples @ 48kHz = ~2.6ms)
        private readonly float[] _microDelayBuffer;
        private readonly int _bufferMask;
        private int _writeIndex;

        // High-pass filter to isolate wet saliva textures from low-end muddy frequencies
        private BiquadFilter _wetHighPass;

        // Stateful parameters
        private uint _lcgState;
        private float _envelope = 0f;
        private float _wobblePhase = 0f;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;
        private readonly float _wobbleIncrement;

        /// <summary>
        /// Initializes the Wet Organic effect.
        /// </summary>
        /// <param name="amount">Intensity of the wet tissue character (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public WetOrganicEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            int size = 128;
            _microDelayBuffer = new float[size];
            _bufferMask = size - 1;
            _writeIndex = 0;

            // Isolate high-mid biological saliva friction zones (around 3200Hz)
            _wetHighPass.ConfigureHighPass(3200f, _sampleRate, q: 0.707f);

            // Sample-rate decoupled tracking coefficients
            _envAttackCoef = (float)Math.Exp(-1000.0 / (5f * _sampleRate));   // 5ms attack
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (40f * _sampleRate)); // 40ms release

            // Fast tissue fluid shift modulation (approx. 9.5 Hz micro-wobble)
            _wobbleIncrement = 9.5f / _sampleRate;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            // Mapping scaling values for wet matrix depth
            float wetMixFactor = _amount * 0.45f;
            if (wetMixFactor > 0.65f) wetMixFactor = 0.65f;

            float baseDelay = _sampleRate * 0.0008f;  // 0.8ms baseline mucous membrane thickness
            float modDepth = _sampleRate * 0.0005f;   // 0.5ms modulation sweep width

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Voice envelope follower
                float absInput = Math.Abs(drySample);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                // 2. Advance micro-wobble phase using polynomial triangle-to-parabola LFO
                _wobblePhase += _wobbleIncrement;
                if (_wobblePhase > 1f) _wobblePhase -= 1f;

                float tri = _wobblePhase * 2f;
                if (tri > 1f) tri = 2f - tri;
                float tissueWobble = 4f * tri * (1f - tri);

                // 3. Store current dry sample into the circular micro-buffer
                _microDelayBuffer[_writeIndex] = drySample;

                // 4. Compute fractional read position for fluid phase shifting
                float targetDelay = baseDelay + (tissueWobble * modDepth * (0.3f + _envelope * 0.7f));
                float readPos = _writeIndex - targetDelay;
                while (readPos < 0f) readPos += _microDelayBuffer.Length;

                int i0 = (int)readPos;
                int i1 = (i0 + 1) & _bufferMask;
                float frac = readPos - i0;
                float delayedSample = _microDelayBuffer[i0 & _bufferMask] * (1f - frac) + _microDelayBuffer[i1] * frac;

                // 5. Ultra-fast local LCG stochastic saliva micro-crackle/bubble generator
                _lcgState = _lcgState * 1103515245 + 12345;
                float bubbleChance = 0.001f + (_envelope * 0.035f * _amount);
                uint maxThreshold = (uint)(bubbleChance * uint.MaxValue);

                float salivaImpulse = 0f;
                if (_lcgState < maxThreshold)
                {
                    float bubbleSign = ((_lcgState & 0x1000) != 0) ? 1f : -1f;
                    salivaImpulse = bubbleSign * 0.15f * _envelope;
                }

                // 6. Combine phase-shifted tissue layer and high-passed micro-bubbles
                float wetCombined = (drySample - delayedSample) + _wetHighPass.Process(salivaImpulse);

                // 7. Fast polynomial soft-clipping saturation
                float drivenWet = wetCombined * 1.4f;
                float saturatedWet = drivenWet / (1f + Math.Abs(drivenWet));

                // Increment write index
                _writeIndex = (_writeIndex + 1) & _bufferMask;

                // 8. In-place buffer interpolation write back
                pcm[i] = (drySample * (1f - wetMixFactor)) + (saturatedWet * wetMixFactor);
            }
        }

        // High-performance, stack-allocated 2nd order IIR filter structure
        private struct BiquadFilter
        {
            private float _b0, _b1, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public void ConfigureHighPass(float cutoffFrequency, float sampleRate, float q)
            {
                float w0 = 2f * (float)Math.PI * cutoffFrequency / sampleRate;
                float alpha = (float)Math.Sin(w0) / (2f * q);
                float cosW0 = (float)Math.Cos(w0);

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
    }
}