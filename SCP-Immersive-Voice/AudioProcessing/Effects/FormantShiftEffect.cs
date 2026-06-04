namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Organic formant shifting using tilt-EQ with envelope-driven movement.
    /// Ideal for creature timbre, SCP-939 mimicry or identity distortion.
    /// </summary>
    public class FormantShiftEffect : IAudioEffect
    {

        public string Name => "Formant Shift";

        private readonly float _target;

        private float _low;
        private float _high;
        private float _env;
        private float _phase;

        public FormantShiftEffect(float formant)
        {
            _target = Clamp(formant, 0.5f, 2.0f);
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Envelope (formants react to loudness)
                float abs = Math.Abs(dry);
                _env += 0.03f * (abs - _env);

                // Slow drift (prevents static timbre)
                _phase += 0.0009f;
                float drift = 0.5f + 0.5f * (float)Math.Sin(_phase * 1.4f);

                // Dynamic shift target
                float shift = Lerp(1f, _target, drift * (0.6f + _env * 0.4f));

                // Tilt-EQ coefficients
                float lowCut = 0.05f * shift;
                float highCut = 0.05f * (2f - shift);

                // Low shelf
                _low += lowCut * (dry - _low);

                // High shelf
                _high += highCut * (dry - _high);

                // Tilt mix
                float tilted = _low * (2f - shift) + _high * shift;

                // Nonlinear shaping
                tilted *= 0.89f + 0.11f * tilted;

                // Mix
                float mixed = dry * 0.5f + tilted * 0.5f;

                // Soft clip
                pcm[i] = (float)Math.Tanh(mixed * 1.03f);
            }
        }
    }
}