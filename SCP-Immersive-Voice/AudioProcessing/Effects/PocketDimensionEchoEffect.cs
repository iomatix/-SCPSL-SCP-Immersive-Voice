namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using LabApi.Features.Audio;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Generates unstable, modulated echo with nonlinear reflections. Simulates
    /// impossible geometry and dimensional distortion. Signature effect for
    /// SCP‑106 and other extradimensional anomalies.
    /// </summary>
    public class PocketDimensionEchoEffect : IAudioEffectShort
    {
        private readonly float _amount;

        // Delay buffer (float for stability)
        private readonly float[] _buffer;
        private int _index;

        // Modulation phases
        private float _timePhase;
        private float _fbPhase;

        public PocketDimensionEchoEffect(float amount)
        {
            // amount 0 → no echo
            // amount 1.5 → strong pocket dimension echo
            _amount = Clamp(amount, 0f, 1.5f);

            // ~0.5s buffer at 48kHz
            _buffer = new float[24000];
            _index = 0;
        }

        public void Process(short[] pcm, int length)
        {
            int bufLen = _buffer.Length;

            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1
                float x = pcm[i] / 32768f;

                // 1. Modulated delay time (nonlinear geometry simulation)
                _timePhase += 0.0008f;
                float tMod = 0.5f + 0.5f * (float)Math.Sin(_timePhase * 1.7f);

                int delaySamples = 4000 + (int)(tMod * 8000); // ~80–250 ms

                int readIndex = _index - delaySamples;
                if (readIndex < 0)
                    readIndex += bufLen;

                float delayed = _buffer[readIndex];

                // 2. Modulated feedback (irregular reflections)
                _fbPhase += 0.0013f;
                float fb = 0.35f + 0.25f * (float)Math.Sin(_fbPhase * 2.3f);

                // Apply soft saturation to feedback path
                float saturated = (float)Math.Tanh(delayed * 2.0f);

                // Write into buffer
                float write = x + saturated * fb;
                _buffer[_index] = write;

                // Advance buffer index
                _index++;
                if (_index >= bufLen)
                    _index = 0;

                // 3. Smearing (dimension smear)
                float smeared = delayed * 0.75f + write * 0.25f;

                // 4. Wet/dry mix
                float wet = smeared * (_amount * 0.7f);
                float mixed = x * (1f - _amount * 0.4f) + wet;

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