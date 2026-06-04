namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Adds reactive breath noise based on input amplitude. Produces airy,
    /// organic textures ideal for whispers, SCP‑939 mimicry, or subtle realism.
    /// Fully float‑native with smooth filtering and soft mixing.
    /// </summary>
    public class BreathNoiseEffect : IAudioEffect
    {
        public string Name => "Breath Noise";

        private readonly float _intensity;
        private static readonly Random _rng = new Random();

        // Filter memory
        private float _bpLast;
        private float _smoothLast;
        private float _t;

        public BreathNoiseEffect(float intensity)
        {
            _intensity = Clamp(intensity, 0f, 2f);
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // 1. White noise (-1..1)
                float noise = (float)(_rng.NextDouble() * 2.0 - 1.0);

                // 2. Simple resonant band-pass (gives breath "air")
                float bp = noise - _bpLast * 0.98f;
                _bpLast = bp;

                // 3. Smooth edges (softens harsh noise)
                float smooth = _smoothLast + 0.1f * (bp - _smoothLast);
                _smoothLast = smooth;

                // 4. Slow amplitude modulation (breathing motion)
                _t += 0.002f;
                float mod = 0.85f + 0.15f * (float)Math.Sin(_t);

                // 5. Final breath noise (scaled)
                float breath = smooth * mod * _intensity * 0.25f;

                // 6. Mix with original PCM (float-native)
                float mixed = pcm[i] + breath;

                // 7. Soft clip to avoid harsh peaks
                mixed = (float)Math.Tanh(mixed);

                pcm[i] = mixed;
            }
        }
    }
}