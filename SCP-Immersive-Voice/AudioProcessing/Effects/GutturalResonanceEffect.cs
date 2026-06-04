namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Deep guttural resonance with envelope-driven feedback, organic modulation
    /// and nonlinear shaping. Ideal for SCP-049-2, SCP-939 or any creature
    /// requiring heavy throat resonance.
    /// </summary>
    public class GutturalResonanceEffect : IAudioEffect
    {
        private readonly float _amount;

        private float _res;
        private float _env;
        private float _phase;

        public GutturalResonanceEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Envelope follower (resonance reacts to vocal intensity)
                float abs = Math.Abs(dry);
                float attack = 0.045f;
                float release = 0.020f;
                _env += (abs - _env) * (abs > _env ? attack : release);

                // Organic throat modulation (slow cavity movement)
                _phase += 0.0019f + _env * 0.00035f;
                float wobble = 0.6f + 0.4f * (float)Math.Sin(_phase * 1.12f);

                // Dynamic resonant feedback (core guttural tone)
                float target = dry - _res * (0.32f + _env * 0.18f) * _amount;
                _res += wobble * 0.115f * (target - _res);

                // Nonlinear shaping (deep throat coloration)
                float shaped = _res * (0.87f + 0.13f * _res);

                // Saturation (organic throat compression)
                float saturated = (float)Math.Tanh(shaped * 2.05f);

                // Wet/dry mix
                float mixed = dry * (1f - _amount * 0.48f) + saturated * (_amount * 0.48f);

                // Final soft clip
                pcm[i] = (float)Math.Tanh(mixed * 1.06f);
            }
        }
    }
}