namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Organic whisper layer simulating breathy, airy, unstable vocal textures.
    /// Transient suppression, envelope-driven softness, micro-modulation and
    /// nonlinear shaping. Ideal for SCP-939 mimicry or SCP-049 whispering.
    /// </summary>
    public class WhisperFilterEffect : IAudioEffect
    {
        public string Name => "Whisper";

        private readonly float _amount;

        private float _smoothLast;
        private float _env;
        private float _phase;

        public WhisperFilterEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Soft envelope follower (breath reactivity)
                float abs = Math.Abs(dry);
                _env += 0.030f * (abs - _env);

                // Transient suppression (soft consonants)
                float suppressed = dry * 0.57f + _smoothLast * 0.43f;

                // High-frequency damping (airy softness)
                float damped = suppressed * (0.83f + _env * 0.065f);

                // Micro-modulation (organic instability)
                _phase += 0.00205f;
                float wobble = 1f + 0.047f * (float)Math.Sin(_phase * 7.0f);

                float modulated = damped * wobble;

                // Nonlinear shaping (breathy saturation)
                float shaped = modulated * (0.89f + 0.11f * modulated);

                // Whisper tail smoothing
                float smooth = _smoothLast + 0.185f * (shaped - _smoothLast);
                _smoothLast = smooth;

                // Wet/dry mix
                pcm[i] = dry * (1f - _amount) + smooth * _amount;
            }
        }
    }
}
