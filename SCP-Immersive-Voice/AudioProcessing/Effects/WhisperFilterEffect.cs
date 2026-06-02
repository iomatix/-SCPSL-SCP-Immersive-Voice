namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    public class WhisperFilterEffect : IAudioEffect
    {
        private readonly float _amount;
        private float _smoothLast;

        public WhisperFilterEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(float[] samples, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float x = samples[i];

                // 1. Transient suppression (reduce consonants)
                float suppressed = x * 0.6f + _smoothLast * 0.4f;

                // 2. High-frequency damping
                float damped = suppressed * 0.9f;

                // 3. Soft clipping (adds whisper-like saturation)
                float soft = (float)(Math.Tanh(damped * 1.5f));

                // 4. Smooth output
                float smooth = _smoothLast + 0.15f * (soft - _smoothLast);
                _smoothLast = smooth;

                // 5. Mix with original
                samples[i] = x * (1f - _amount) + smooth * _amount;
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
