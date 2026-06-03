namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Slowly modulates vocal formants to create unstable, shifting timbre.
    /// Suitable for SCP‑939 mimicry, demonic voices, or identity‑distorted speech.
    /// </summary>
    public class FormantDriftEffect : IAudioEffectShort
    {
        private readonly float _amount;

        // Filter states (float is required for stability and precision)
        private float _lpState;
        private float _hpState;
        private float _phase;

        private static readonly Random _rng = new Random();

        public FormantDriftEffect(float amount)
        {
            // amount 0 → no drift
            // amount 1.5 → very unstable formant movement
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1 for filter processing
                float x = pcm[i] / 32768f;

                // 1. Noise-modulated LFO controlling formant drift
                _phase += 0.0007f;
                float noise = (float)(_rng.NextDouble() * 2.0 - 1.0);
                float drift = (float)Math.Sin(_phase * 1.3f + noise * 0.5f);

                // 2. Dynamic LP/HP cutoff modulation
                float lpCut = 0.15f + 0.25f * drift; // 0.15–0.40
                float hpCut = 0.85f + 0.10f * drift; // 0.75–0.95

                // Low-pass filter (smooth formant body)
                _lpState += lpCut * (x - _lpState);

                // High-pass filter (formant edge)
                _hpState = x - _lpState * hpCut;

                // 3. Mix original and shifted signal
                float shifted = _hpState;
                float mixed = x * (1f - _amount * 0.5f) + shifted * (_amount * 0.5f);

                // Convert back to PCM
                int sample = (int)(mixed * 32767f);

                // Clamp to valid PCM range
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
