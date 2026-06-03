namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using LabApi.Features.Audio;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Applies amplitude modulation (tremolo) using a smooth, organic LFO.
    /// Ideal for zombie voices, SCP‑049‑2, SCP‑939, radio modulation, or
    /// unstable dimensional audio. Includes micro‑modulation and smoothing
    /// to avoid synthetic or harsh digital tremolo artifacts.
    /// </summary>
    public class TremoloEffect : IAudioEffectShort
    {
        private readonly float _frequency;

        // LFO phase
        private float _phase;

        // Micro‑modulation for organic movement
        private float _jitterPhase;

        public TremoloEffect(float frequency)
        {
            // frequency 0.1–20 Hz recommended
            _frequency = Clamp(frequency, 0.1f, 20f);
        }

        public void Process(short[] pcm, int length)
        {
            float sampleRate = AudioTransmitter.SampleRate;

            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1
                float x = pcm[i] / 32768f;

                // 1. Base LFO
                _phase += (2f * (float)Math.PI * _frequency) / sampleRate;
                if (_phase > 10000f) _phase = 0f;

                float lfo = (float)Math.Sin(_phase);

                // 2. Micro‑modulation (prevents synthetic tremolo)
                _jitterPhase += 0.0007f;
                float jitter = 0.03f * (float)Math.Sin(_jitterPhase * 13.7f);

                float mod = 0.5f * (1f + lfo + jitter);

                // 3. Soft saturation for natural amplitude shaping
                float shaped = (float)Math.Tanh(mod * 1.4f);

                // 4. Apply tremolo
                float outSample = x * shaped;

                // Convert back to PCM
                int sample = (int)(outSample * 32767f);

                // Clamp
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
