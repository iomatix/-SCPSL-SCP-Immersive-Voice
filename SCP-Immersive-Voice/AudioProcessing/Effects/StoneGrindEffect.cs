namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    ///  Physical Modeling Stick-Slip Stone Grinding and Friction Engine for SCP-173.
    /// Replaces continuous thermal white noise with a discrete interlocking crystal grid shear matrix.
    /// Uses asymmetric ring modulation to turn human speech into jagged rock grit.
    /// </summary>
    public class StoneGrindEffect : IAudioEffect
    {
        public string Name => "Stone Grind";

        private readonly float _intensity;
        private readonly float _sampleRate;

        private BiquadFilter _stoneRumbleFilter;
        private BiquadFilter _stoneGritFilter;

        private uint _lcgState;
        private float _envelope = 0f;
        private float _frictionLfoPhase = 0f;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;
        private readonly float _lfoIncrement;
        private const float TwoPi = (float)(Math.PI * 2.0);

        /// <summary>
        /// Initializes the tectonic Stone Grind engine.
        /// </summary>
        public StoneGrindEffect(float intensity, float sampleRate)
        {
            _intensity = Clamp(intensity, 0f, 2f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            //  FIX: Sub-bass tectonic structural displacement rumble (110Hz)
            _stoneRumbleFilter.ConfigureBandPass(110f, _sampleRate, q: 5.0f);

            //  FIX: Dense, crushing aggregate rock-grain abrasive scratch (750Hz)
            _stoneGritFilter.ConfigureBandPass(750f, _sampleRate, q: 1.8f);

            _envAttackCoef = (float)Math.Exp(-1000.0 / (15f * _sampleRate));
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (110f * _sampleRate));

            _lfoIncrement = 4.2f / _sampleRate; // 4.2 Hz surface macro-fault rate
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _intensity < 0.01f) return;

            float frictionGainFactor = _intensity * 0.65f;

            for (int i = 0; i < length; i++)
            {
                float dryInput = pcm[i];

                float absInput = Math.Abs(dryInput);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                _frictionLfoPhase += _lfoIncrement;
                if (_frictionLfoPhase > 1f) _frictionLfoPhase -= 1f;

                float tri = _frictionLfoPhase * 2f;
                if (tri > 1f) tri = 2f - tri;
                float surfaceWobble = 4f * tri * (1f - tri);

                _lcgState = _lcgState * 1103515245 + 12345;

                //  FIX (Stick-Slip Macro Modeling):
                // Instead of streaming continuous white noise (which sounds like sand/sandpaper),
                // we model interlocking crystal ridges. Sound is only generated when micro-fault thresholds crash.
                float stickSlipSource = 0f;
                uint slipThreshold = (uint)(0.22f * uint.MaxValue); // Sparse shear threshold
                if (_lcgState < slipThreshold)
                {
                    stickSlipSource = ((float)(_lcgState & 0xFFFF) / 65535f) * 2f - 1f;
                }

                float massRumble = _stoneRumbleFilter.Process(stickSlipSource);
                float surfaceGrit = _stoneGritFilter.Process(stickSlipSource);

                float dynamicallyModulatedGrit = surfaceGrit * (0.3f + surfaceWobble * 0.7f);

                // Heavy 75% internal weight density blend staging
                float compositeGrind = (massRumble * 0.75f) + (dynamicallyModulatedGrit * 0.25f);
                float activeGrindLayer = compositeGrind * _envelope * frictionGainFactor;

                float drivenGrind = activeGrindLayer * 3.0f;
                float saturatedGrind = drivenGrind / (1f + Math.Abs(drivenGrind));

                //  FIX (Asymmetric Tectonic Ring-Modulation):
                // We use a high-frequency stone lattice modulation wave driven directly by the surface wobble LFO
                // to completely disrupt human pitch components, turning vocals into mineral grinding textures.
                float mineralModulationWave = (float)Math.Sin(_frictionLfoPhase * TwoPi * 28f); // Harsh intermodulation
                float lithosphericVoice = dryInput * (1f - (_envelope * _intensity * 0.6f)) +
                                         (dryInput * mineralModulationWave * _envelope * _intensity * 0.5f);

                pcm[i] = lithosphericVoice + saturatedGrind;
            }
        }

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
                _x2 = _x1; _x1 = input; _y2 = _y1; _y1 = output;
                return output;
            }
        }
    }
}