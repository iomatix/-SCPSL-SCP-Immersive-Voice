namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Clean, stable pitch shifting using float-native resampling.
    /// Zero-alloc, smooth, anti-zipper and soft-clipped. Ideal for
    /// creature voices, SCP mimicry or identity distortion.
    /// </summary>
    public class PitchShiftEffect : IAudioEffect
    {
        public string Name => "Pitch Shift";

        private float _pitch;
        private float _smoothPitch;

        // Persistent buffer to avoid allocations
        private float[] _buffer = Array.Empty<float>();

        public PitchShiftEffect(float pitch)
        {
            _pitch = Clamp(pitch, 0.25f, 4f);
            _smoothPitch = _pitch;
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 2)
                return;

            // Anti-zipper smoothing
            _smoothPitch += 0.05f * (_pitch - _smoothPitch);

            // Ensure buffer capacity (zero-alloc during processing)
            if (_buffer.Length < length)
                _buffer = new float[length];

            for (int i = 0; i < length; i++)
            {
                float src = i / _smoothPitch;

                // Clamp source index
                if (src < 0f) src = 0f;
                if (src > length - 1) src = length - 1;

                int i0 = (int)src;
                int i1 = (i0 + 1 < length) ? i0 + 1 : i0;

                float frac = src - i0;

                // Linear interpolation
                float s0 = pcm[i0];
                float s1 = pcm[i1];
                float interp = s0 + (s1 - s0) * frac;

                // Soft clip
                interp = (float)Math.Tanh(interp * 1.08f);

                _buffer[i] = interp;
            }

            // Copy back
            Array.Copy(_buffer, pcm, length);
        }
    }
}