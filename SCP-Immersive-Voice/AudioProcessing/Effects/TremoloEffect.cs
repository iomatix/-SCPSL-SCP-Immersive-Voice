namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using LabApi.Features.Audio;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using UnityEngine;
    public class TremoloEffect : IAudioEffect
    {
        private readonly float _frequency;

        public TremoloEffect(float frequency)
        {
            _frequency = frequency;
        }

        public void Process(float[] pcm, int samples)
        {
            float sampleRate = AudioTransmitter.SampleRate;

            for (int i = 0; i < samples; i++)
            {
                float t = i / sampleRate;
                float mod = 0.5f * (1 + Mathf.Sin(2 * Mathf.PI * _frequency * t));
                pcm[i] *= mod;
            }
        }
    }
}
