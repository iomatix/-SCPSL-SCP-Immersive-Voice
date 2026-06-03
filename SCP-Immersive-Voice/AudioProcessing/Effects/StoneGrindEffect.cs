namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Generates continuous stone‑grinding friction noise. 
    /// Ideal for SCP‑173 movement, stone creatures, or heavy surface scraping.
    /// Uses band‑pass shaping, micro‑modulation, smoothing, and saturation 
    /// to create an organic stone‑on‑stone grinding texture.
    /// </summary>
    public class StoneGrindEffect : IAudioEffectShort
    {
        private readonly float _intensity;

        // Band‑pass memory
        private float _bpState;

        // Smoothing for natural decay
        private float _smooth;

        // Random generator
        private static readonly Random _rng = new Random();

        public StoneGrindEffect(float intensity = 1.0f)
        {
            // intensity 0 → no grind
            // intensity 2 → very aggressive stone friction
            _intensity = Clamp(intensity, 0f, 2f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1
                float x = pcm[i] / 32768f;

                // 1. Base noise (raw friction energy)
                float noise = (float)(_rng.NextDouble() * 2.0 - 1.0);

                // 2. Band‑pass shaping (stone friction lives in 200–800 Hz)
                float bp = noise - _bpState * 0.82f;
                _bpState = bp;

                // 3. Micro‑modulation (irregular friction movement)
                float mod = 0.7f + 0.3f * (float)Math.Sin(i * 0.002f);

                // 4. Combine and scale
                float grind = bp * mod * _intensity * 0.45f;

                // 5. Smooth to avoid harsh digital edges
                _smooth += 0.18f * (grind - _smooth);

                // 6. Soft saturation for natural stone hardness
                float saturated = (float)Math.Tanh(_smooth * 2.2f);

                // 7. Mix with original signal
                float mixed = x + saturated;

                // Convert back to PCM
                int sample = (int)(mixed * 32767f);

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
