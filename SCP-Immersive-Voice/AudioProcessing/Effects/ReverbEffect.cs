namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using LabApi.Features.Audio;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Lightweight diffusion‑based reverb with micro‑modulation. Adds spatial
    /// smear and soft reflections ideal for tunnels, chambers, or SCP‑049 ambience.
    /// </summary>
    public class ReverbEffect : IAudioEffectShort
    {
        private readonly float _mix;

        // Delay buffer (~200 ms at 48 kHz)
        private readonly float[] _buffer;
        private int _index;

        // Diffusion memory
        private float _d1, _d2, _d3;

        // Micro-modulation phase
        private float _phase;

        public ReverbEffect(float mix)
        {
            // mix 0 → dry
            // mix 1.5 → very wet
            _mix = Clamp(mix, 0f, 1.5f);

            _buffer = new float[9600]; // 200 ms at 48kHz
            _index = 0;
        }

        public void Process(short[] pcm, int length)
        {
            int bufLen = _buffer.Length;

            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1
                float x = pcm[i] / 32768f;

                // 1. Micro-modulated delay time (prevents metallic ringing)
                _phase += 0.0009f;
                float mod = 0.5f + 0.5f * (float)Math.Sin(_phase * 1.3f);
                int delay = 300 + (int)(mod * 400); // 6–14 ms

                int readIndex = _index - delay;
                if (readIndex < 0)
                    readIndex += bufLen;

                float delayed = _buffer[readIndex];

                // 2. Diffusion (3‑tap mini‑Schroeder)
                _d1 += 0.4f * (delayed - _d1);
                _d2 += 0.4f * (_d1 - _d2);
                _d3 += 0.4f * (_d2 - _d3);

                float diffused = (_d1 + _d2 + _d3) * 0.33f;

                // 3. Soft saturation in feedback path
                float saturated = (float)Math.Tanh(diffused * 2.0f);

                // 4. Write into buffer (feedback)
                float write = x + saturated * 0.35f;
                _buffer[_index] = write;

                // Advance buffer index
                _index++;
                if (_index >= bufLen)
                    _index = 0;

                // 5. Wet/dry mix
                float wet = diffused * (_mix * 0.7f);
                float mixed = x * (1f - _mix * 0.4f) + wet;

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