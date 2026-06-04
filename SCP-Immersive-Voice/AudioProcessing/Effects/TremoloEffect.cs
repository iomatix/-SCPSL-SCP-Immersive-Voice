namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using LabApi.Features.Audio;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Organic amplitude‑warp tremolo with dual‑phase modulation, micro‑jitter,
    /// nonlinear shaping and soft‑clipped output. Ideal for SCP‑049‑2, SCP‑939,
    /// zombie voices, radio modulation or dimensional instability.
    /// Float‑native, smooth and alias‑free.
    /// </summary>
    public class TremoloEffect : IAudioEffect
    {
        private readonly float _frequency;

        private float _phase;
        private float _jitterPhase;
        private float _smooth;

        public TremoloEffect(float frequency)
        {
            _frequency = Clamp(frequency, 0.1f, 20f);
        }

        public void Process(float[] pcm, int length)
        {
            float sampleRate = AudioTransmitter.SampleRate;

            for (int i = 0; i < length; i++)
            {
                float x = pcm[i];

                // 1. Base LFO (primary tremolo movement)
                _phase += (2f * (float)Math.PI * _frequency) / sampleRate;
                if (_phase > 10000f) _phase = 0f;

                float lfo = (float)Math.Sin(_phase);

                // 2. Micro‑jitter (organic instability)
                _jitterPhase += 0.0012f;
                float jitter = 0.03f * (float)Math.Sin(_jitterPhase * 17.3f);

                // 3. Combine modulation layers
                float mod = 0.5f * (1f + lfo + jitter);

                // 4. Nonlinear shaping (natural amplitude curve)
                float shaped = mod * (0.85f + 0.15f * mod);

                // 5. Smooth amplitude transitions (avoid digital stepping)
                _smooth += 0.12f * (shaped - _smooth);

                // 6. Soft saturation (organic tremolo character)
                float trem = (float)Math.Tanh(_smooth * 1.6f);

                // 7. Apply tremolo
                float outSample = x * trem;

                pcm[i] = outSample;
            }
        }
    }
}