namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using LabApi.Features.Audio;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using UnityEngine;

    public class LowPassEffect : IAudioEffect
    {
        private readonly float _cutoff;

        public LowPassEffect(float cutoffHz)
        {
            _cutoff = cutoffHz;
        }

        public void Process(float[] pcm, int samples)
        {
            float rc = 1.0f / (2 * Mathf.PI * _cutoff);
            float dt = 1.0f / AudioTransmitter.SampleRate;
            float alpha = dt / (rc + dt);

            float prev = pcm[0];
            for (int i = 1; i < samples; i++)
            {
                prev = prev + alpha * (pcm[i] - prev);
                pcm[i] = prev;
            }
        }
    }
}
