namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Shifts vocal formants without altering pitch. Produces altered identity,
    /// creature‑like timbre, or uncanny voice transformations.
    /// </summary>
    public class FormantShiftEffect : IAudioEffectShort
    {
        private readonly float _formant;

        // Filter states for tilt EQ
        private float _lowState;
        private float _highState;

        public FormantShiftEffect(float formant)
        {
            // formant 1.0 = neutral
            // <1.0 = darker / lower formants
            // >1.0 = brighter / higher formants
            _formant = Clamp(formant, 0.5f, 2.0f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1 for processing
                float x = pcm[i] / 32768f;

                // Time-based modulation (smooth drift across the buffer)
                float t = (float)i / length;
                float shift = 1f + (_formant - 1f) * t;

                // Tilt EQ coefficients
                // shift > 1 → boost highs
                // shift < 1 → boost lows
                float lowCut = 0.05f * shift;
                float highCut = 0.05f * (2f - shift);

                // Low shelf (smooth low-frequency emphasis)
                _lowState += lowCut * (x - _lowState);

                // High shelf (smooth high-frequency emphasis)
                _highState += highCut * (x - _highState);

                // Tilt mix: blend low and high emphasis
                float tilted = _lowState * (2f - shift) + _highState * shift;

                // Mix original + tilted
                float mixed = x * 0.5f + tilted * 0.5f;

                // Convert back to PCM
                int sample = (int)(mixed * 32767f);

                // Clamp to valid PCM range
                if (sample > short.MaxValue) sample = short.MaxValue;
                if (sample < short.MinValue) sample = short.MinValue;

                pcm[i] = (short)sample;
            }
        }
        private static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }
    }
}
