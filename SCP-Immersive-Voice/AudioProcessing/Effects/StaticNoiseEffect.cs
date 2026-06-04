namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA Organic multi-layer static noise generator for SCP-079 and radio transmission corruption.
    /// Employs a thread-safe local LCG, sample-rate independent biquad filter matrices, 
    /// and fast polynomial LFOs to simulate complex RF interference. Zero-allocation.
    /// </summary>
    public class StaticNoiseEffect : IAudioEffect
    {
        public string Name => "Static Noise";

        private readonly float _amount;
        private readonly float _sampleRate;

        // Dual biquad filters to isolate radio frequency bands
        private BiquadFilter _radioBandpass;
        private BiquadFilter _hfFizzHighpass;

        // Stateful parameters for LCG noise and phase tracking
        private uint _lcgState;
        private float _driftPhase = 0f;
        private float _fizzPhase = 0f;

        private readonly float _driftIncrement;
        private readonly float _fizzIncrement;

        /// <summary>
        /// Initializes the Static Noise effect.
        /// </summary>
        /// <param name="amount">Intensity and mix of the static interference (0.0f to 1.0f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public StaticNoiseEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Configure RF bandpass to emulate intercom/radio chassis bandwidth (1200Hz - 3200Hz)
            _radioBandpass.ConfigureBandPass(1800f, _sampleRate, q: 0.6f);

            // Configure crisp electrical fizz filter at 5500Hz
            _hfFizzHighpass.ConfigureHighPass(5500f, _sampleRate, q: 1.0f);

            // Sample-rate independent modulation speeds (Drift = 0.4 Hz, Fizz Mod = 7.5 Hz)
            _driftIncrement = 0.4f / _sampleRate;
            _fizzIncrement = 7.5f / _sampleRate;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Advance ultra-fast local LCG for raw white noise generation
                _lcgState = _lcgState * 1103515245 + 12345;
                float whiteNoise = ((float)(_lcgState & 0xFFFF) / 65535f) * 2f - 1f;

                // 2. Advance sample-rate decoupled linear phase accumulators
                _driftPhase += _driftIncrement;
                if (_driftPhase > 1f) _driftPhase -= 1f;

                _fizzPhase += _fizzIncrement;
                if (_fizzPhase > 1f) _fizzPhase -= 1f;

                // 3. Fast polynomial triangle-to-parabola approximations (Replaces expensive Math.Sin)
                float driftTri = _driftPhase * 2f;
                if (driftTri > 1f) driftTri = 2f - driftTri;
                float lowFrequencyDrift = 4f * driftTri * (1f - driftTri); // Organic 0.4Hz swell envelope

                float fizzTri = _fizzPhase * 2f;
                if (fizzTri > 1f) fizzTri = 2f - fizzTri;
                float highFrequencyFizzMod = 4f * fizzTri * (1f - fizzTri); // 7.5Hz crackle modulator

                // 4. Process the white noise source through independent spectral shapers
                float baseHiss = _radioBandpass.Process(whiteNoise);
                float rawFizz = _hfFizzHighpass.Process(whiteNoise);

                // 5. Synthesize the multi-layered electromagnetic static node
                float modulatedHiss = baseHiss * (0.6f + lowFrequencyDrift * 0.4f);
                float modulatedFizz = rawFizz * (0.2f + highFrequencyFizzMod * 0.8f) * 0.35f;
                float combinedStatic = modulatedHiss + modulatedFizz;

                // 6. Fast polynomial soft-clipping for analog radio circuit overdrive
                float drivenStatic = combinedStatic * 1.5f;
                float staticOut = drivenStatic / (1f + Math.Abs(drivenStatic));

                // 7. Constant-power style blend injection directly into the live buffer
                pcm[i] = (drySample * (1f - _amount)) + (staticOut * _amount * 0.45f);
            }
        }

        // High-performance, stack-allocated 2nd order IIR filter structure
        private struct BiquadFilter
        {
            private float _b0, _b1, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public void ConfigureBandPass(float centerFrequency, float sampleRate, float q)
            {
                float w0 = 2f * (float)Math.PI * centerFrequency / sampleRate;
                float alpha = (float)Math.Sin(w0) / (2f * q);
                float cosW0 = (float)Math.Cos(w0);

                float a0 = 1f + alpha;
                _b0 = alpha / a0;
                _b1 = 0f;
                _b2 = -alpha / a0;
                _a1 = (-2f * cosW0) / a0;
                _a2 = (1f - alpha) / a0;
            }

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