namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using System;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;

    /// <summary>
    /// SCP-939 biological vocal camouflage layer.
    ///
    /// Simulates:
    /// - turbulent airflow
    /// - throat cavity friction
    /// - wet tissue resonance
    /// - predatory breathing texture
    ///
    /// Designed specifically for SCP-939.
    /// Not a whisper effect.
    /// </summary>
    public sealed class PredatoryCamouflageEffect : IAudioEffect
    {
        public string Name => "Predatory Camouflage";

        private float _amount;

        private readonly float _sampleRate;

        private uint _noiseState;

        private float _envelope;

        private readonly float _attackCoef;
        private readonly float _releaseCoef;

        private float _pink1;
        private float _pink2;
        private float _pink3;

        private BiquadFilter _throatBand;
        private BiquadFilter _salivaBand;
        private BiquadFilter _airBand;

        public PredatoryCamouflageEffect(
            float amount,
            float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1f);

            _sampleRate =
                sampleRate > 0f
                    ? sampleRate
                    : 48000f;

            _noiseState =
                (uint)Guid.NewGuid().GetHashCode();

            _attackCoef =
                (float)Math.Exp(-1000.0 / (4f * _sampleRate));

            _releaseCoef =
                (float)Math.Exp(-1000.0 / (80f * _sampleRate));

            // Deep throat friction

            _throatBand.ConfigureBandPass(
                850f,
                _sampleRate,
                0.7f);

            // Wet mouth tissue

            _salivaBand.ConfigureBandPass(
                1800f,
                _sampleRate,
                0.8f);

            // Air turbulence

            _airBand.ConfigureBandPass(
                4200f,
                _sampleRate,
                0.6f);
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.001f)
                return;

            float amount =
                Clamp(_amount, 0f, 1f);

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                float absInput =
                    Math.Abs(dry);

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

                _noiseState =
                    _noiseState * 1664525u +
                    1013904223u;

                float white =
                    (((_noiseState >> 8) & 0x00FFFFFF)
                    / 8388607.5f) - 1f;

                _pink1 =
                    0.9975f * _pink1 +
                    white * 0.099f;

                _pink2 =
                    0.9850f * _pink2 +
                    white * 0.050f;

                _pink3 =
                    0.9500f * _pink3 +
                    white * 0.020f;

                float pink =
                    (_pink1 + _pink2 + _pink3 + white * 0.15f)
                    * 0.55f;

                float throat =
                    _throatBand.Process(pink);

                float saliva =
                    _salivaBand.Process(pink);

                float air =
                    _airBand.Process(pink);

                float creatureLayer =
                    throat * 0.45f +
                    saliva * 0.25f +
                    air * 0.30f;

                creatureLayer *=
                    (_envelope * 1.6f);

                // Preserve speech identity

                float speechResidual =
                    dry * 0.35f;

                pcm[i] =
                    dry +
                    (creatureLayer + speechResidual)
                    * amount;
            }
        }

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
                float w0 =
                    2f * (float)Math.PI *
                    centerFrequency /
                    sampleRate;

                float alpha =
                    (float)Math.Sin(w0) /
                    (2f * q);

                float cosW0 =
                    (float)Math.Cos(w0);

                float a0 =
                    1f + alpha;

                _b0 = alpha / a0;
                _b1 = 0f;
                _b2 = -alpha / a0;

                _a1 = (-2f * cosW0) / a0;
                _a2 = (1f - alpha) / a0;
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