namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Organic formant drift with slow modulation, jitter and nonlinear shaping.
    /// Ideal for SCP-939 mimicry, demonic timbre or identity distortion.
    /// </summary>
    public class FormantDriftEffect : IAudioEffect
    {
        public string Name => "Formant Drift";

        private readonly float _amount;

        private float _lp;
        private float _hp;
        private float _phase;

        private readonly Random _rng;

        public FormantDriftEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Slow drift with jitter (organic throat movement)
                _phase += 0.00068f;
                float jitter = ((float)_rng.NextDouble() * 2f - 1f) * 0.12f;
                float drift = (float)Math.Sin(_phase * 1.28f + jitter);

                // Dynamic LP/HP morphing
                float lpCut = 0.15f + 0.25f * drift;
                float hpCut = 0.85f + 0.10f * drift;

                // Low-pass body
                _lp += lpCut * (dry - _lp);

                // High-pass edge
                _hp = dry - _lp * hpCut;

                // Nonlinear shaping
                float shifted = _hp * (0.89f + 0.11f * _hp);

                // Wet/dry mix
                float mixed = dry * (1f - _amount * 0.5f) + shifted * (_amount * 0.5f);

                // Soft clip
                pcm[i] = (float)Math.Tanh(mixed * 1.02f);
            }
        }
    }
}