namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    public class PocketDimensionEchoEffect : IAudioEffect
    {
        private readonly float _amount;
        private readonly float[] _buffer;
        private int _index;
        private float _timePhase;
        private float _fbPhase;
        public PocketDimensionEchoEffect(float amount)
        {
            _amount = Clamp(amount, 0f, 1.5f);

            // ~0.5s buffer at 48kHz
            _buffer = new float[24000];
            _index = 0;
        }

        public void Process(float[] samples, int length)
        {
            int bufLen = _buffer.Length;

            for (int i = 0; i < length; i++)
            {
                float x = samples[i];

                // 1. Modulated delay time (pseudo "nonlinear geometry")
                _timePhase += 0.0008f;
                float tMod = 0.5f + 0.5f * (float)Math.Sin(_timePhase * 1.7f);
                int delaySamples = 4000 + (int)(tMod * 8000); // ~80–250 ms

                int readIndex = _index - delaySamples;
                if (readIndex < 0) readIndex += bufLen;

                float delayed = _buffer[readIndex];

                // 2. Modulated feedback (irregular reflections)
                _fbPhase += 0.0013f;
                float fb = 0.35f + 0.25f * (float)Math.Sin(_fbPhase * 2.3f);

                float write = x + delayed * fb;
                _buffer[_index] = write;

                _index++;
                if (_index >= bufLen) _index = 0;

                // 3. Subtle smearing (light "dimension smear")
                float smeared = delayed * 0.8f + write * 0.2f;

                // 4. Mix
                float wet = smeared * (_amount * 0.7f);
                samples[i] = x * (1f - _amount * 0.4f) + wet;
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
