namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA Physical Modeling Stone Grinding and Friction Engine for SCP-173.
    /// Employs a local thread-isolated LCG, sample-rate independent dual-band 
    /// biquad resonators (Rumble/Grit), and dynamic vocal envelope coupling. Zero allocations.
    /// </summary>
    public class StoneGrindEffect : IAudioEffect
    {
        public string Name => "Stone Grind";

        private readonly float _intensity;
        private readonly float _sampleRate;

        // Dual-band biquad filters to model stone mass and surface grain properties
        private BiquadFilter _stoneRumbleFilter;
        private BiquadFilter _stoneGritFilter;

        // Stateful tracking parameters
        private uint _lcgState;
        private float _envelope = 0f;
        private float _frictionLfoPhase = 0f;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;
        private readonly float _lfoIncrement;

        /// <summary>
        /// Initializes the Stone Grind effect.
        /// </summary>
        /// <param name="intensity">Weight and roughness of the stone-on-concrete sliding friction (0.0f to 2.0f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public StoneGrindEffect(float intensity, float sampleRate)
        {
            _intensity = Clamp(intensity, 0f, 2f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Band 1: Deep sub-harmonic rumble of heavy internal concrete mass (220Hz)
            _stoneRumbleFilter.ConfigureBandPass(220f, _sampleRate, q: 2.2f);

            // Band 2: Brittle, high-frequency granular scraping of crushed aggregate particles (2400Hz)
            _stoneGritFilter.ConfigureBandPass(2400f, _sampleRate, q: 0.8f);

            // Sample-rate independent tracking lag (Slightly slower reaction to simulate mass inertia)
            _envAttackCoef = (float)Math.Exp(-1000.0 / (15f * _sampleRate));  // 15ms attack
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (110f * _sampleRate)); // 110ms release

            // Continuous surface unevenness simulation LFO (~3.8 Hz micro-fluctuation rate)
            _lfoIncrement = 3.8f / _sampleRate;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _intensity < 0.01f) return;

            // Global gain staging scaler
            float frictionGainFactor = _intensity * 0.45f;

            for (int i = 0; i < length; i++)
            {
                float dryInput = pcm[i];

                // 1. Voice envelope follower (Vocal energy dictates the mechanical friction pressure)
                float absInput = Math.Abs(dryInput);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                // 2. Advance structural surface unevenness LFO via fast polynomial triangle-to-parabola
                _frictionLfoPhase += _lfoIncrement;
                if (_frictionLfoPhase > 1f) _frictionLfoPhase -= 1f;

                float tri = _frictionLfoPhase * 2f;
                if (tri > 1f) tri = 2f - tri;
                float surfaceWobble = 4f * tri * (1f - tri); // 0..1 smooth modulator

                // 3. Generate high-speed thread-safe local LCG white noise impulse stream
                _lcgState = _lcgState * 1103515245 + 12345;
                float rawFrictionNoise = ((float)(_lcgState & 0xFFFF) / 65535f) * 2f - 1f;

                // 4. Process the raw friction source through independent structural resonance bands
                float massRumble = _stoneRumbleFilter.Process(rawFrictionNoise);
                float surfaceGrit = _stoneGritFilter.Process(rawFrictionNoise);

                // Dynamic cross-modulation: surface grit is modulated by the unevenness wobble LFO
                float dynamicallyModulatedGrit = surfaceGrit * (0.4f + surfaceWobble * 0.6f);

                // Accumulate acoustic layers: 65% deep internal mass weight + 35% granular aggregate scrape
                float compositeGrind = (massRumble * 0.65f) + (dynamicallyModulatedGrit * 0.35f);

                // 5. Envelope coupling: grind intensity tracks player voice effort linearly
                float activeGrindLayer = compositeGrind * _envelope * frictionGainFactor;

                // 6. Polynomial soft-clipping saturation to give the concrete scratch a dense, solid boundary
                float drivenGrind = activeGrindLayer * 2.2f;
                float saturatedGrind = drivenGrind / (1f + Math.Abs(drivenGrind));

                // 7. Sum the completed litosferyczne tarcie node directly back into the live PCM stream
                pcm[i] = dryInput + saturatedGrind;
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