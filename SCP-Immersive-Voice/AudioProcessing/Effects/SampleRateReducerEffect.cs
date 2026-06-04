namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Organic sample-rate degradation with jitter, spectral smear and nonlinear
    /// reconstruction. Produces SCP-079-style corrupted audio and unstable
    /// retro-digital textures.
    /// </summary>
    public class SampleRateReducerEffect : IAudioEffect
    {
        public string Name => "Sample Rate Reducer";

        private readonly float _amount;

        private float _hold;
        private float _smooth;
        private float _phase;

        // Per-instance RNG for stable, isolated jitter
        private readonly Random _rng;

        public SampleRateReducerEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1f);
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Process(float[] pcm, int length)
        {
            int baseSkip = 1 + (int)(_amount * 14f);

            for (int i = 0; i < length; i++)
            {
                float x = pcm[i];

                // Organic jitter (skip instability)
                int skip = baseSkip;
                if (_amount > 0.15f)
                {
                    skip += _rng.Next(-1, 2);
                    if (skip < 1) skip = 1;
                }

                // Sample hold (core sample-rate reduction)
                if (i % skip == 0)
                    _hold = x;

                // Spectral smear (softens stair-stepping)
                _smooth += 0.20f * (_hold - _smooth);

                // Nonlinear reconstruction (digital-organic hybrid)
                _phase += 0.0032f + _amount * 0.0021f;
                float warp = _smooth * (0.86f + 0.14f * (float)Math.Sin(_phase));

                // Soft saturation (lo-fi warmth)
                float outSample = (float)Math.Tanh(warp * 1.45f);

                pcm[i] = outSample;
            }
        }
    }
}