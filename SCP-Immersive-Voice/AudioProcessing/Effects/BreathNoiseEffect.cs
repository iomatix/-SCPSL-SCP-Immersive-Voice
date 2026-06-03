namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Adds reactive breath noise based on input amplitude. Produces airy, breathy
    /// textures ideal for whispers, SCP‑939 mimicry, or subtle vocal realism.
    /// </summary>

    public class BreathNoiseEffect : IAudioEffectShort
    {
        private readonly float _intensity;
        private static readonly Random _rng = new Random();

        // Filter memory (still float, because filter state must be high precision)
        private float _bpLast;
        private float _smoothLast;
        private float _t;

        public BreathNoiseEffect(float intensity)
        {
            _intensity = Clamp(intensity, 0f, 2f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // 1. Generate white noise in range -1..1
                float noise = (float)(_rng.NextDouble() * 2.0 - 1.0);

                // 2. Simple resonant band-pass filter
                float bp = noise - _bpLast * 0.98f;
                _bpLast = bp;

                // 3. Smooth the signal to avoid harsh edges
                float smooth = _smoothLast + 0.1f * (bp - _smoothLast);
                _smoothLast = smooth;

                // 4. Slow amplitude modulation (breathing feel)
                _t += 0.002f;
                float mod = 0.85f + 0.15f * (float)Math.Sin(_t);

                // 5. Final breath noise value (scaled)
                float breath = smooth * mod * _intensity * 0.25f;

                // 6. Convert breath noise to PCM amplitude range
                int breathPcm = (int)(breath * 32767f);

                // 7. Mix with original PCM sample
                int mixed = pcm[i] + breathPcm;

                // 8. Clamp to valid PCM range
                if (mixed > short.MaxValue) mixed = short.MaxValue;
                if (mixed < short.MinValue) mixed = short.MinValue;

                pcm[i] = (short)mixed;
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
