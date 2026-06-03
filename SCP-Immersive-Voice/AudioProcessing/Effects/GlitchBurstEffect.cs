namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Injects short, randomized glitch bursts with digital harshness. Ideal for
    /// SCP‑079, corrupted comms, or unstable dimensional audio.
    /// </summary>
    public class GlitchBurstEffect : IAudioEffectShort
    {
        private readonly float _amount;
        private static readonly Random _rng = new Random();

        // Burst state
        private int _burstSamplesLeft;
        private float _burstPhase;

        public GlitchBurstEffect(float amount)
        {
            // amount 0 → no glitches
            // amount 1.5 → very frequent glitch bursts
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // 1. Random chance to start a glitch burst
                if (_burstSamplesLeft <= 0)
                {
                    if (_rng.NextDouble() < _amount * 0.002)
                    {
                        // Burst lasts 5–25 samples
                        _burstSamplesLeft = _rng.Next(5, 25);
                        _burstPhase = 0f;
                    }
                }

                if (_burstSamplesLeft > 0)
                {
                    // 2. Generate glitch burst sample
                    // Mix of noise + foldback distortion + bitcrush
                    float noise = (float)(_rng.NextDouble() * 2.0 - 1.0);

                    // Foldback distortion for digital glitch character
                    float folded = Math.Abs(noise * 3f % 2f - 1f) * 0.8f;

                    // Envelope for smooth attack/decay
                    float env = 1f - (_burstPhase / _burstSamplesLeft);

                    float glitch = (noise * 0.4f + folded * 0.6f) * env;

                    // Convert to PCM
                    int glitchPcm = (int)(glitch * 32767f);

                    // Mix with original sample
                    int mixed = pcm[i] + glitchPcm;

                    // Clamp to valid PCM range
                    if (mixed > short.MaxValue) mixed = short.MaxValue;
                    if (mixed < short.MinValue) mixed = short.MinValue;

                    pcm[i] = (short)mixed;

                    // Advance burst state
                    _burstPhase++;
                    _burstSamplesLeft--;
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
