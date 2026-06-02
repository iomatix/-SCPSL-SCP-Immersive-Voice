namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    public class PitchShiftEffect : IAudioEffect
    {
        private readonly float _pitch;

        public PitchShiftEffect(float pitch)
        {
            _pitch = pitch;
        }

        public void Process(float[] pcm, int samples)
        {
            float[] temp = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float src = i / _pitch;
                int i0 = (int)src;
                int i1 = Math.Min(i0 + 1, samples - 1);
                float frac = src - i0;

                temp[i] = pcm[i0] * (1 - frac) + pcm[i1] * frac;
            }

            Array.Copy(temp, pcm, samples);
        }
    }

}
