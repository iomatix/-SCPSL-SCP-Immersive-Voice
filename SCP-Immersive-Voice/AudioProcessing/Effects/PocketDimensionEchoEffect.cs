namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    using VoiceChat;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;

    /// <summary>
    /// Nonlinear extradimensional echo with modulated delay, unstable feedback
    /// and dimensional smear. Signature SCP-106 spatial distortion.
    /// </summary>
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
            _buffer = new float[VoiceChatSettings.SampleRate / 2]; // ~0.5s @ 48kHz
        }

        public void Process(float[] pcm, int length)
        {
            int bufLen = _buffer.Length;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Modulated delay time (impossible geometry)
                _timePhase += 0.00078f;
                float tMod = 0.5f + 0.5f * (float)Math.Sin(_timePhase * 1.68f);

                int delay = 4000 + (int)(tMod * 8000);
                int readIndex = _index - delay;
                if (readIndex < 0) readIndex += bufLen;

                float delayed = _buffer[readIndex];

                // Modulated feedback (unstable reflections)
                _fbPhase += 0.00128f;
                float fb = 0.34f + 0.24f * (float)Math.Sin(_fbPhase * 2.25f);

                // Feedback limiter (prevents runaway)
                fb = Clamp(fb, 0f, 0.58f);

                // Nonlinear shaping in feedback path
                float shaped = delayed * (0.86f + 0.14f * delayed);
                float saturated = (float)Math.Tanh(shaped * 1.95f);

                // Write into buffer
                float write = dry + saturated * fb;
                _buffer[_index] = write;

                _index++;
                if (_index >= bufLen) _index = 0;

                // Dimensional smear
                float smear = delayed * 0.74f + write * 0.26f;

                // Wet/dry mix
                float wet = smear * (_amount * 0.68f);
                float mixed = dry * (1f - _amount * 0.38f) + wet;

                // Final soft clip
                pcm[i] = (float)Math.Tanh(mixed * 1.08f);
            }
        }
    }
}