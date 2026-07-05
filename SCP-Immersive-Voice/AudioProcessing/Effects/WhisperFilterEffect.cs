namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Spectral Whisper Synthesizer.
    ///
    /// Design goals:
    /// - Remove vocal fold dominance while preserving articulation.
    /// - Generate breath-like unvoiced energy instead of random digital static.
    /// - Preserve speech intelligibility through envelope tracking.
    /// - Remain fully real-time safe and allocation-free during Process().
    /// - Maintain internal state between calls for smooth continuity.
    /// </summary>
    public sealed class WhisperFilterEffect : IAudioEffect
    {
        public string Name => "Whisper Filter";

        // Updated live through FastInjectScalarField()
        private float _amount;

        private readonly float _sampleRate;

        // Voice activity tracking
        private float _envelope;

        private readonly float _attackCoef;
        private readonly float _releaseCoef;

        // Local RNG state
        private uint _noiseState;

        // Pink-noise approximation states
        private float _pink1;
        private float _pink2;
        private float _pink3;

        // Whisper articulation filters
        private BiquadFilter _presenceBand;
        private BiquadFilter _airBand;

        /// <summary>
        /// Initializes the whisper synthesizer.
        /// </summary>
        /// <param name="amount">
        /// Whisper intensity.
        /// 0.0 = bypass.
        /// 1.0 = maximum whisper transformation.
        /// </param>
        /// <param name="sampleRate">Engine sample rate.</param>
        public WhisperFilterEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _noiseState = (uint)Guid.NewGuid().GetHashCode();

            // Fast speech tracking attack
            _attackCoef = (float)Math.Exp(-1000.0 / (5f * _sampleRate));

            // Slower release preserves articulation tails
            _releaseCoef = (float)Math.Exp(-1000.0 / (60f * _sampleRate));

            // Main articulation band
            _presenceBand.ConfigureBandPass(
                centerFrequency: 2800f,
                sampleRate: _sampleRate,
                q: 0.8f);

            // Secondary air turbulence band
            _airBand.ConfigureBandPass(
                centerFrequency: 4500f,
                sampleRate: _sampleRate,
                q: 0.6f);
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount <= 0.001f)
                return;

            float amount = Clamp(_amount, 0f, 1f);

            // Constant-power blend
            float dryGain = (float)Math.Sqrt(1f - amount);
            float wetGain = (float)Math.Sqrt(amount);

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // ---------------------------------------------------------
                // Envelope Tracking
                // ---------------------------------------------------------

                float absInput = Math.Abs(dry);

                if (absInput > _envelope)
                {
                    _envelope =
                        _attackCoef * _envelope +
                        (1f - _attackCoef) * absInput;
                }
                else
                {
                    _envelope =
                        _releaseCoef * _envelope +
                        (1f - _releaseCoef) * absInput;
                }

                // ---------------------------------------------------------
                // White Noise Generator
                // ---------------------------------------------------------

                _noiseState = _noiseState * 1664525u + 1013904223u;

                float white =
                    (((_noiseState >> 8) & 0x00FFFFFF) / 8388607.5f) - 1f;

                // ---------------------------------------------------------
                // Lightweight Pink Noise Approximation
                // ---------------------------------------------------------

                _pink1 = 0.9975f * _pink1 + white * 0.0990f;
                _pink2 = 0.9850f * _pink2 + white * 0.0500f;
                _pink3 = 0.9500f * _pink3 + white * 0.0200f;

                float pink =
                    (_pink1 + _pink2 + _pink3 + white * 0.150f) * 0.55f;

                // ---------------------------------------------------------
                // Spectral Whisper Construction
                // ---------------------------------------------------------

                float articulation =
                    _presenceBand.Process(pink);

                float turbulence =
                    _airBand.Process(pink);

                // Blend articulation and air layers
                float whisperCore =
                    articulation * 0.70f +
                    turbulence * 0.30f;

                // Voice-reactive modulation
                float modulatedWhisper =
                    whisperCore * (_envelope * 1.35f);

                // Preserve a tiny amount of speech residue.
                // This dramatically improves intelligibility compared
                // to full speech destruction.
                float residualSpeech =
                    dry * (1f - amount) * 0.18f;

                float whisperSignal =
                    modulatedWhisper +
                    residualSpeech;

                // ---------------------------------------------------------
                // Constant-Power Output Blend
                // ---------------------------------------------------------

                pcm[i] =
                    dry * dryGain +
                    whisperSignal * wetGain;
            }
        }

        /// <summary>
        /// High-performance biquad band-pass filter.
        /// Stateful and allocation-free.
        /// </summary>
        private struct BiquadFilter
        {
            private float _b0;
            private float _b1;
            private float _b2;
            private float _a1;
            private float _a2;

            private float _x1;
            private float _x2;
            private float _y1;
            private float _y2;

            public void ConfigureBandPass(
                float centerFrequency,
                float sampleRate,
                float q)
            {
                centerFrequency =
                    Math.Max(40f,
                    Math.Min(centerFrequency, sampleRate * 0.45f));

                q = Math.Max(0.1f, q);

                float w0 =
                    2f * (float)Math.PI *
                    centerFrequency /
                    sampleRate;

                float sinW0 = (float)Math.Sin(w0);
                float cosW0 = (float)Math.Cos(w0);

                float alpha =
                    sinW0 / (2f * q);

                float a0 =
                    1f + alpha;

                _b0 =
                    alpha / a0;

                _b1 =
                    0f;

                _b2 =
                    -alpha / a0;

                _a1 =
                    (-2f * cosW0) / a0;

                _a2 =
                    (1f - alpha) / a0;
            }

            public float Process(float input)
            {
                float output =
                    _b0 * input +
                    _b1 * _x1 +
                    _b2 * _x2 -
                    _a1 * _y1 -
                    _a2 * _y2;

                _x2 = _x1;
                _x1 = input;

                _y2 = _y1;
                _y1 = output;

                return output;
            }
        }
    }
}