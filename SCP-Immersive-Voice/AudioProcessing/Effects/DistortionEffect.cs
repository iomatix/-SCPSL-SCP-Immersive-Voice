namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Applies soft or hard saturation depending on intensity. Adds aggressive,
    /// gritty harmonic distortion ideal for monstrous voices, SCP‑049‑2, or
    /// corrupted radio transmissions.
    /// </summary>
    public class DistortionEffect : IAudioEffectShort
    {
        private readonly float _drive;

        public DistortionEffect(float drive)
        {
            // drive 1.0 = no change
            // drive 2.0 = strong distortion
            _drive = Clamp(drive, 0f, 5f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1 for processing
                float x = pcm[i] / 32768f;

                // Apply drive (gain)
                x *= _drive;

                // Soft clipping using tanh for analog-style saturation
                float soft = (float)Math.Tanh(x);

                // Convert back to PCM range
                int sample = (int)(soft * 32767f);

                // Hard clamp for safety
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
