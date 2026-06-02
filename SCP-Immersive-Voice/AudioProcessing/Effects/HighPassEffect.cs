namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using LabApi.Features.Audio;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using UnityEngine;

    public class HighPassEffect : IAudioEffect
    {
        private readonly float _cutoff;

        public HighPassEffect(float cutoffHz)
        {
            _cutoff = cutoffHz;
        }

        public void Process(float[] pcm, int samples)
        {
            float rc = 1.0f / (2 * Mathf.PI * _cutoff);
            float dt = 1.0f / AudioTransmitter.SampleRate;
            float alpha = dt / (rc + dt);

            float prevInput = pcm[0];
            float prevLow = pcm[0];

            for (int i = 1; i < samples; i++)
            {
                float low = prevLow + alpha * (pcm[i] - prevLow);
                float high = pcm[i] - low;

                pcm[i] = high;

                prevLow = low;
                prevInput = pcm[i];
            }
        }
    }
}