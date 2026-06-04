namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Organic subharmonic generator simulating a secondary throat cavity.
    /// Features amplitude‑locked oscillation, nonlinear modulation, throat
    /// resonance smoothing and soft‑clipped growl shaping. Ideal for SCP‑049‑2,
    /// SCP‑939, zombies, demons or any deep monstrous undertone.
    /// </summary>
    public class SubharmonicGrowlEffect : IAudioEffect
    {
        public string Name => "Subharmonic Growl";

        private readonly float _amount;

        private float _phase;
        private float _env;
        private float _smooth;
        private float _modPhase;

        public SubharmonicGrowlEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float x = pcm[i];

                // 1. Envelope follower (voice amplitude → growl intensity)
                _env += 0.05f * (Math.Abs(x) - _env);

                // 2. Subharmonic oscillator (half‑frequency growl)
                _phase += 0.015f + _env * 0.02f;
                float osc = (float)Math.Sin(_phase * 0.5f);

                // 3. Organic modulation (secondary throat instability)
                _modPhase += 0.0018f + _env * 0.0012f;
                float mod = 0.75f + 0.25f * (float)Math.Sin(_modPhase * 1.1f);

                // 4. Combine oscillator + modulation + amplitude
                float growl = osc * mod * _env * (_amount * 0.65f);

                // 5. Throat resonance smoothing (heavy, organic movement)
                _smooth += 0.22f * (growl - _smooth);

                // 6. Nonlinear shaping (flesh/organic hardness)
                float shaped = _smooth * (0.85f + 0.15f * _smooth);

                // 7. Soft saturation (deep guttural tone)
                float saturated = (float)Math.Tanh(shaped * 2.4f);

                // 8. Mix with original
                float mixed = x + saturated;

                pcm[i] = mixed;
            }
        }
    }
}