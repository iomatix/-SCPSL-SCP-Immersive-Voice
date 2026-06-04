namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using LabApi.Features.Audio;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Smooth one-pole low-pass filter for muffling high frequencies.
    /// Float-native, stable and alias-free. Ideal for SCP-049 mask muffling,
    /// SCP-106 void dampening or radio bandwidth limitation.
    /// </summary>
    public class LowPassEffect : IAudioEffect
    {
        public string Name => "Low Pass";

        private readonly float _cutoff;
        private float _lp;

        public LowPassEffect(float cutoffHz)
        {
            _cutoff = Clamp(cutoffHz, 20f, 20000f);
        }

        public void Process(float[] pcm, int length)
        {
            float rc = 1f / (2f * (float)Math.PI * _cutoff);
            float dt = 1f / AudioTransmitter.SampleRate;
            float alpha = dt / (rc + dt);

            for (int i = 0; i < length; i++)
            {
                float x = pcm[i];

                // Low-pass smoothing
                _lp += alpha * (x - _lp);

                pcm[i] = _lp;
            }
        }
    }
}