namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Enhances low‑frequency throat resonance for deep, monstrous growls.
    /// Perfect for SCP‑049‑2, SCP‑939, or any creature requiring guttural power.
    /// </summary>
    public class GutturalResonanceEffect : IAudioEffectShort
    {
        private readonly float _amount;

        // Resonance state (float required for stability)
        private float _res;

        // LFO phase for slow modulation
        private float _phase;

        public GutturalResonanceEffect(float amount)
        {
            // amount 0 → no resonance
            // amount 1.5 → very deep guttural resonance
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1 for processing
                float x = pcm[i] / 32768f;

                // 1. Slow LFO modulation (simulates throat cavity movement)
                _phase += 0.002f;
                float mod = 0.6f + 0.4f * (float)Math.Sin(_phase);

                // 2. One-pole resonant filter with dynamic feedback
                float target = x - _res * 0.3f * _amount; // feedback shaping
                _res += mod * 0.12f * (target - _res);

                // 3. Soft saturation to avoid harsh digital resonance
                float saturated = (float)Math.Tanh(_res * 2.2f);

                // 4. Mix original + resonant tone
                float mixed = x * (1f - _amount * 0.5f) + saturated * (_amount * 0.5f);

                // 5. Convert back to PCM
                int sample = (int)(mixed * 32767f);

                // 6. Clamp to valid PCM range
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
