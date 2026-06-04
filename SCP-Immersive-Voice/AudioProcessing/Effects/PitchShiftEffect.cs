namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Natural pitch shifting using float‑native resampling.
    /// Designed for creature voices: stable, smooth and artifact‑controlled.
    /// </summary>
    public class PitchShiftEffect : IAudioEffect
    {
        public string Name => "Pitch Shift";

        private float _targetPitch;
        private float _smoothPitch;

        // Persistent buffer to avoid allocations
        private float[] _buffer = Array.Empty<float>();

        public PitchShiftEffect(float pitch)
        {
            // keep pitch in a safe, natural range
            _targetPitch = Clamp(pitch, 0.25f, 4f);
            _smoothPitch = _targetPitch;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 2)
                return;

            // smooth pitch transitions to avoid zipper artifacts
            _smoothPitch += 0.05f * (_targetPitch - _smoothPitch);

            // Ensure buffer capacity
            if (_buffer.Length < length)
                _buffer = new float[length];

            for (int i = 0; i < length; i++)
            {
                // correct pitch direction (pitch < 1 = lower, pitch > 1 = higher)
                float src = i * _smoothPitch;

                // Clamp source index
                if (src < 0f) src = 0f;
                if (src > length - 1) src = length - 1;

                int i0 = (int)src;
                int i1 = (i0 + 1 < length) ? i0 + 1 : i0;

                float frac = src - i0;

                // Linear interpolation
                float s0 = pcm[i0];
                float s1 = pcm[i1];
                float shifted = s0 + (s1 - s0) * frac;

                // soften harsh transients and reduce aliasing
                shifted = (float)Math.Tanh(shifted * 1.05f);

                // preserve natural tone by mixing a small dry component
                float dry = pcm[i];
                float mixed = dry * 0.15f + shifted * 0.85f;

                _buffer[i] = mixed;
            }

            // Copy back
            Array.Copy(_buffer, pcm, length);
        }
    }
}
