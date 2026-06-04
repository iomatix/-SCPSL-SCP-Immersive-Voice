namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA Phase-Locked Subharmonic Generator.
    /// Tracks the voice fundamental frequency (f0) via zero-crossing detection 
    /// and synthesizes a perfect sub-octave (f0 / 2) sub-bass layer.
    /// </summary>
    public class SubharmonicGrowlEffect : IAudioEffect
    {
        public string Name => "Subharmonic Growl";

        private readonly float _amount;

        // Dynamic Biquad filters for tracking and reconstruction
        private BiquadFilter _inputAnalysisLp;
        private BiquadFilter _subharmonicSmoothLp;

        // Stateful tracking variables
        private float _prevFilteredSample;
        private float _flipFlopState = 1f;
        private float _envelope = 0f;
        private float _envAttackCoef;
        private float _envReleaseCoef;

        /// <summary>
        /// Initializes the Subharmonic Growl Effect.
        /// </summary>
        /// <param name="amount">Intensity of the subharmonic growl (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public SubharmonicGrowlEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            // Filter 1: Isolate vocal fundamental frequency (f0) below 130Hz
            _inputAnalysisLp.ConfigureLowPass(130f, sr, q: 0.707f);

            // Filter 2: Smooth out generated raw square sub-edges below 75Hz into pure sub-bass
            _subharmonicSmoothLp.ConfigureLowPass(75f, sr, q: 0.85f);

            // Envelope follower coefficients (Fast attack, medium release)
            _envAttackCoef = (float)Math.Exp(-1000.0 / (4f * sr));
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (45f * sr));
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            for (int i = 0; i < length; i++)
            {
                float inputSample = pcm[i];

                // 1. Track global amplitude envelope for dynamic scaling
                float absInput = Math.Abs(inputSample);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                // 2. Isolate the low fundamental using the first Biquad filter
                float fundamentalZone = _inputAnalysisLp.Process(inputSample);

                // 3. Time-domain frequency divider (Zero-Crossing Flip-Flop)
                // Every full cycle of fundamentalZone triggers half a cycle of _flipFlopState (f0 / 2)
                if (fundamentalZone > 0f && _prevFilteredSample <= 0f)
                {
                    _flipFlopState = -_flipFlopState;
                }
                _prevFilteredSample = fundamentalZone;

                // 4. Shape and reconstruct the subharmonic wave
                float rawSub = _flipFlopState * _envelope;
                float cleanSubBass = _subharmonicSmoothLp.Process(rawSub);

                // 5. Apply polynomial soft-clipping for a warm, guttural monster growl
                // Fast emulation of saturation without using expensive Math.Tanh
                float drivenSub = cleanSubBass * 1.6f;
                float saturatedSub = drivenSub / (1f + Math.Abs(drivenSub));

                // 6. Mix the synthesized cinematic sub-bass back into the primary signal
                pcm[i] = inputSample + (saturatedSub * _amount * 0.75f);
            }
        }

        // High-performance, stack-allocated 2nd order IIR filter structure
        private struct BiquadFilter
        {
            private float _b0, _b1, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public void ConfigureLowPass(float cutoffFrequency, float sampleRate, float q)
            {
                float w0 = 2f * (float)Math.PI * cutoffFrequency / sampleRate;
                float alpha = (float)Math.Sin(w0) / (2f * q);
                float cosW0 = (float)Math.Cos(w0);

                float a0 = 1f + alpha;
                _b0 = ((1f - cosW0) / 2f) / a0;
                _b1 = (1f - cosW0) / a0;
                _b2 = ((1f - cosW0) / 2f) / a0;
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