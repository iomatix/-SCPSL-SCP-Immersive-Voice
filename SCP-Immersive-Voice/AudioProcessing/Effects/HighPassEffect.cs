namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using LabApi.Features.Audio;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Transparent one-pole high-pass filter for removing low-frequency rumble.
    /// Float-native, stable and alias-free. Ideal for radio clarity, SCP-079 comms
    /// or removing proximity boominess.
    /// </summary>
    public class HighPassEffect : IAudioEffect
    {
        private readonly float _cutoff;
        private float _lp;

        public HighPassEffect(float cutoffHz)
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

                // Low-pass tracking (extract low frequencies)
                _lp += alpha * (x - _lp);

                // High-pass = input - low frequencies
                pcm[i] = x - _lp;
            }
        }
    }
}
