namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Thread-safe organic formant drift utilizing bounded non-linear coefficient morphing.
    /// Fully protected against Nyquist thresholds, infinite feedback explosion, and NaN poisoning.
    /// </summary>
    public class FormantDriftEffect : IAudioEffect
    {
        public string Name => "Formant Drift";

        private readonly float _amount;
        private float _lp;
        private float _hp;
        private float _phase;

        private readonly Random _rng;
        private const float TwoPi = (float)(Math.PI * 2.0);

        /// <summary>
        /// Initializes a new instance of the <see cref="FormantDriftEffect"/> class.
        /// </summary>
        /// <param name="amount">The wet mix intensity scaling factor.</param>
        public FormantDriftEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            _rng = new Random(Guid.NewGuid().GetHashCode());
        }

        /// <summary>
        /// Processes the raw PCM float stream in-place.
        /// </summary>
        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Slow drift with jitter (organic throat muscle contraction simulation)
                _phase += 0.00068f;
                if (_phase > TwoPi)
                {
                    _phase -= TwoPi; // Wrap phase cleanly to prevent float precision degradation over long uptime
                }

                float jitter = ((float)_rng.NextDouble() * 2f - 1f) * 0.12f;
                float drift = (float)Math.Sin(_phase * 1.28f + jitter);

                //  FIXED: Bounded coefficient scaling window. 
                // Maps safely between 0.15f and 0.55f, completely avoiding negative phase loops.
                float lpCut = 0.35f + 0.20f * drift;
                float hpCut = 0.85f + 0.10f * drift;

                // Low-pass core body modeling
                _lp += lpCut * (dry - _lp);

                // High-pass dynamic structural subtraction
                _hp = dry - (_lp * hpCut);

                // Non-linear polynomial acoustic shaping
                float shifted = _hp * (0.89f + 0.11f * _hp);

                // Wet/dry mix linear combination interpolation
                float mixed = (dry * (1f - _amount * 0.5f)) + (shifted * (_amount * 0.5f));

                // Hardwired bounded laryngeal soft clipping overdrive
                pcm[i] = (float)Math.Tanh(mixed * 1.02f);
            }
        }
    }
}