namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Float-native bitcrusher with TPDF dithering, DC blocking, smooth
    /// quantization and analog-style soft clipping. Warm, gritty and stable.
    /// </summary>
    public class BitcrushEffect : IAudioEffect
    {
        public string Name => "Bitcrush";

        private readonly int _steps;
        private readonly float _invSteps;
        private readonly float _ditherScale;

        private float _dc; // DC blocker state

        // Per-instance RNG
        private readonly Random _rng;

        public BitcrushEffect(float amount)
        {
            amount = Clamp(amount, 0f, 1f);

            _steps = (int)(256 * (1f - amount));
            if (_steps < 2) _steps = 2;

            _invSteps = 1f / _steps;

            // Slightly reduced dithering for warmer tone
            _ditherScale = _invSteps * 0.42f;

            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // TPDF dithering (two independent RNGs)
                float dither =
                    ((float)_rng.NextDouble() - (float)_rng.NextDouble()) * _ditherScale;

                float v = dry + dither;

                // Quantization
                v = (float)Math.Round(v * _steps) * _invSteps;

                // DC blocker (ultra-light)
                // y[n] = x[n] - x[n-1] + 0.995 * y[n-1]
                float dcRemoved = v - _dc;
                _dc = v + dcRemoved * 0.995f;

                // Analog-style soft clip
                float shaped = (float)Math.Tanh(dcRemoved * 1.85f) * 0.54f;

                pcm[i] = shaped;
            }
        }
    }
}
