namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Produces a soft whisper‑like texture by suppressing transients,
    /// damping high frequencies, adding gentle saturation, and smoothing
    /// the output. Ideal for SCP‑939 mimicry, SCP‑049 whispering, or any
    /// creature requiring breathy, airy, unstable vocal artifacts.
    /// </summary>
    public class WhisperFilterEffect : IAudioEffectShort
    {
        private readonly float _amount;

        // Smoothing memory for transient suppression
        private float _smoothLast;

        // Envelope follower (whisper reacts to voice amplitude)
        private float _env;

        // Micro‑modulation phase (adds organic instability)
        private float _phase;

        public WhisperFilterEffect(float amount)
        {
            // amount 0 → dry
            // amount 1.5 → strong whisper layer
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1
                float x = pcm[i] / 32768f;

                // ---------------------------------------------------------
                // 1. Envelope follower (whisper reacts to loudness)
                // ---------------------------------------------------------
                float abs = Math.Abs(x);
                _env += 0.03f * (abs - _env);

                // ---------------------------------------------------------
                // 2. Transient suppression (softens consonants)
                // ---------------------------------------------------------
                float suppressed = x * 0.55f + _smoothLast * 0.45f;

                // ---------------------------------------------------------
                // 3. High‑frequency damping (breathy softness)
                // ---------------------------------------------------------
                float damped = suppressed * (0.85f + _env * 0.05f);

                // ---------------------------------------------------------
                // 4. Micro‑modulation (adds whisper instability)
                // ---------------------------------------------------------
                _phase += 0.0021f;
                float wobble = 1f + 0.05f * (float)Math.Sin(_phase * 7.3f);

                float modulated = damped * wobble;

                // ---------------------------------------------------------
                // 5. Soft clipping (breathy saturation)
                // ---------------------------------------------------------
                float soft = (float)Math.Tanh(modulated * 1.6f);

                // ---------------------------------------------------------
                // 6. Smooth output (whisper tail)
                // ---------------------------------------------------------
                float smooth = _smoothLast + 0.18f * (soft - _smoothLast);
                _smoothLast = smooth;

                // ---------------------------------------------------------
                // 7. Mix dry + whisper
                // ---------------------------------------------------------
                float mixed = x * (1f - _amount) + smooth * _amount;

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