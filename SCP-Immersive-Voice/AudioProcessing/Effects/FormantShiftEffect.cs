namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Natural formant shifting inspired by vocal tract tilt.
    /// Designed to preserve intelligibility while altering timbre.
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
            // keep formant shift in a natural range
            _target = Clamp(formant, 0.5f, 2.0f);
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // formant movement reacts to loudness, but smoothly
                float abs = Math.Abs(dry);
                _env += 0.01f * (abs - _env);

                // subtle drift to avoid static timbre
                _phase += 0.0003f;
                float drift = 0.5f + 0.5f * (float)Math.Sin(_phase * 0.9f);

                // dynamic but controlled formant target
                float shift = Lerp(1f, _target, drift * (0.4f + _env * 0.3f));

                // gentle tilt-EQ to simulate vocal tract shape
                float lowCut = 0.03f * shift;
                float highCut = 0.03f * (2f - shift);

                _low += lowCut * (dry - _low);
                _high += highCut * (dry - _high);

                float tilted = _low * (2f - shift) + _high * shift;

                // mild nonlinear shaping for organic tone
                tilted *= 0.95f + 0.05f * tilted;

                // preserve intelligibility by keeping strong dry component
                float mixed = dry * 0.65f + tilted * 0.35f;

                // prevent harshness without altering tone
                pcm[i] = (float)Math.Tanh(mixed * 1.01f);
            }
        }
    }
}
