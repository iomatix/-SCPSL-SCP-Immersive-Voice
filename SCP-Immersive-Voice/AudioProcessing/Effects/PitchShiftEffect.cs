namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Resamples audio to shift pitch up or down with smoothing. Produces clean,
    /// stable pitch changes suitable for creature voices, SCP mimicry, or identity
    /// distortion.
    /// </summary>
    public class PitchShiftEffect : IAudioEffectShort
    {
        private float _pitch;

        // Smoothed pitch to avoid zipper noise
        private float _smoothPitch;

        public PitchShiftEffect(float pitch)
        {
            // pitch 1.0 = neutral
            // <1.0 = lower pitch
            // >1.0 = higher pitch
            _pitch = Clamp(pitch, 0.25f, 4f);
            _smoothPitch = _pitch;
        }

        public void Process(short[] pcm, int length)
        {
            if (length < 2)
                return;

            // Temporary buffer for processed samples
            short[] temp = new short[length];

            // Smooth pitch to avoid sudden jumps
            _smoothPitch += 0.05f * (_pitch - _smoothPitch);

            for (int i = 0; i < length; i++)
            {
                // Source position in original buffer
                float src = i / _smoothPitch;

                // Clamp source index
                if (src < 0f)
                    src = 0f;
                if (src > length - 1)
                    src = length - 1;

                int i0 = (int)src;
                int i1 = (i0 + 1 < length) ? i0 + 1 : i0;

                float frac = src - i0;

                // Convert to float -1..1
                float s0 = pcm[i0] / 32768f;
                float s1 = pcm[i1] / 32768f;

                // Linear interpolation
                float interp = s0 * (1f - frac) + s1 * frac;

                // Convert back to PCM
                int sample = (int)(interp * 32767f);

                // Clamp
                if (sample > short.MaxValue) sample = short.MaxValue;
                if (sample < short.MinValue) sample = short.MinValue;

                temp[i] = (short)sample;
            }

            // Copy back
            Array.Copy(temp, pcm, length);
        }

        private static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }
    }
}