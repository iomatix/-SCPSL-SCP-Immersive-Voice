namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    using VoiceChat;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;

    /// <summary>
    /// Nonlinear diffusion reverb with micro-modulation and dimensional smear.
    /// Ideal for SCP-049 ambience, tunnels, chambers or supernatural coloration.
    /// </summary>
    public class ReverbEffect : IAudioEffect
    {
        public string Name => "Reverb";

        private readonly float _mix;

        private readonly float[] _buffer;
        private int _index;

        private float _d1, _d2, _d3, _d4;
        private float _phase;

        public ReverbEffect(float mix)
        {
            _mix = Clamp(mix, 0f, 1.5f);
            _buffer = new float[VoiceChatSettings.SampleRate / 5]; // ~200ms @ 48kHz
        }

        public void Process(float[] pcm, int length)
        {
            int bufLen = _buffer.Length;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // Micro-modulated delay (prevents metallic ringing)
                _phase += 0.00088f;
                float mod = 0.5f + 0.5f * (float)Math.Sin(_phase * 1.28f);

                int delay = 300 + (int)(mod * 400);
                int readIndex = _index - delay;
                if (readIndex < 0) readIndex += bufLen;

                float delayed = _buffer[readIndex];

                // Nonlinear diffusion (4-stage)
                _d1 += 0.44f * (delayed - _d1);
                _d2 += 0.44f * (_d1 - _d2);
                _d3 += 0.44f * (_d2 - _d3);
                _d4 += 0.44f * (_d3 - _d4);

                float diffused = (_d1 + _d2 + _d3 + _d4) * 0.25f;

                // Dimensional smear
                float smear = diffused * (0.86f + 0.14f * diffused);

                // Feedback limiter
                float fb = 0.30f;
                fb = Clamp(fb, 0f, 0.45f);

                // Saturation in feedback path
                float saturated = (float)Math.Tanh(smear * 1.95f);

                // Write into buffer
                float write = dry + saturated * fb;
                _buffer[_index] = write;

                _index++;
                if (_index >= bufLen) _index = 0;

                // Wet/dry mix
                float wet = smear * (_mix * 0.68f);
                float mixed = dry * (1f - _mix * 0.38f) + wet;

                // Final soft clip
                pcm[i] = (float)Math.Tanh(mixed * 1.08f);
            }
        }
    }
}