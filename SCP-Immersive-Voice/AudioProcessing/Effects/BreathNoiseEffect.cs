namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA Voice-Reactive Breath Noise Simulator.
    /// Combines an ultra-fast local LCG randomizer, vocal envelope tracking, and a slow 
    /// physiological LFO into a custom Biquad Bandpass filter to synthesize organic airflow turbulence.
    /// </summary>
    public class BreathNoiseEffect : IAudioEffect
    {
        public string Name => "Breath Noise";

        private readonly float _intensity;
        private readonly float _sampleRate;

        // Stack-allocated Biquad filter representing the physical throat/mouth air cavity
        private BiquadFilter _airCavityFilter;

        // Stateful tracking parameters
        private uint _lcgState;
        private float _envelope = 0f;
        private float _lfoPhase = 0f;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;
        private readonly float _lfoIncrement;

        /// <summary>
        /// Initializes the Breath Noise effect.
        /// </summary>
        /// <param name="intensity">Global scaling factor of the breath layer (0.0f to 2.0f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public BreathNoiseEffect(float intensity, float sampleRate)
        {
            _intensity = Clamp(intensity, 0f, 2f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // Isolate local LCG state per player instance to achieve 100% thread safety
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Configure throat resonance bandpass at 1800Hz with low Q for smooth, wide breath rush
            _airCavityFilter.ConfigureBandPass(1800f, _sampleRate, q: 0.5f);

            // Sample-rate independent smooth envelope multipliers
            _envAttackCoef = (float)Math.Exp(-1000.0 / (8f * _sampleRate));   // 8ms attack
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (70f * _sampleRate)); // 70ms release

            // Slow physiological lung movement LFO (approx. 0.25Hz cycle = 1 breath every 4 seconds)
            _lfoIncrement = (2f * (float)Math.PI * 0.25f) / _sampleRate;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _intensity < 0.01f) return;

            // Base scaling factor optimized for the new pipeline gain staging
            float baseGain = _intensity * 0.15f;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Voice envelope follower (modulates air rush density dynamically during speech)
                float absInput = Math.Abs(drySample);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                // 2. Advance physiological lung LFO cycle
                _lfoPhase += _lfoIncrement;
                if (_lfoPhase > 2f * (float)Math.PI) _lfoPhase -= 2f * (float)Math.PI;
                float lungSwell = 0.7f + 0.3f * (float)Math.Sin(_lfoPhase);

                // 3. Ultra-fast thread-isolated LCG white noise generation
                _lcgState = _lcgState * 1103515245 + 12345;
                float rawNoise = ((float)(_lcgState & 0xFFFF) / 65535f) * 2f - 1f;

                // 4. Shape the white noise through the acoustic throat biquad filter
                float shapedAir = _airCavityFilter.Process(rawNoise);

                // 5. Dual-modulation matrix combining voice effort and underlying breathing rhythm
                float dynamicModulation = _envelope * 0.7f + 0.3f * lungSwell;
                float synthesizedBreath = shapedAir * dynamicModulation * baseGain;

                // 6. Clean summation into the target in-place buffer (Soft-Limiter handles safety later)
                pcm[i] = drySample + synthesizedBreath;
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