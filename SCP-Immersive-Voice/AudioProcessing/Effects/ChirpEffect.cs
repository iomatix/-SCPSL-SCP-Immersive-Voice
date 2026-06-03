namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Injects short, randomized chirp bursts with smooth fade‑out. Creates
    /// glitchy, unstable tonal artifacts suitable for SCP‑079 and... Flamingos,
    /// dimensional interference, or corrupted audio streams.
    /// </summary>
    public class ChirpEffect : IAudioEffectShort
    {
        private readonly float _amount;
        private float _phase;
        private static readonly Random _rng = new Random();

        // Envelope state for smooth fade-out
        private float _envelope;

        public ChirpEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // 1. Random chance to trigger a chirp
                if (_rng.NextDouble() < _amount * 0.0008)
                {
                    _phase = 0f;   // reset chirp phase
                    _envelope = 1f; // start at full level
                }

                // 2. If chirp is active (phase still within 0..2π and envelope > 0)
                if (_phase < Math.PI * 2 && _envelope > 0.001f)
                {
                    // Generate chirp tone (scaled sinus)
                    float chirp = (float)Math.Sin(_phase * 20f) * 0.15f * _amount;

                    // Apply envelope for smooth fade-out
                    chirp *= _envelope;

                    // Convert chirp to PCM amplitude range
                    int chirpPcm = (int)(chirp * 32767f);

                    // Mix with original sample
                    int mixed = pcm[i] + chirpPcm;

                    // Clamp to valid PCM range
                    if (mixed > short.MaxValue) mixed = short.MaxValue;
                    if (mixed < short.MinValue) mixed = short.MinValue;

                    pcm[i] = (short)mixed;

                    // Advance chirp phase
                    _phase += 0.15f;

                    // Exponential envelope decay for organic tail
                    _envelope *= 0.97f;
                }
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
