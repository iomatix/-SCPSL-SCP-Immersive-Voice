namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    ///  Stateful Noise Gate with RMS envelope detection.
    /// Features smooth Attack, Hold, and Release to isolate voice and kill tail noise.
    /// </summary>
    public class NoiseGateEffect : IAudioEffect
    {
        public string Name => "Noise Gate";

        private readonly float _thresholdLinear;
        private readonly float _attackCoef;
        private readonly float _releaseCoef;
        private readonly int _holdSamples;

        private float _envelope = 0f;
        private float _currentGain = 1f;
        private int _holdCounter = 0;

        /// <summary>
        /// Initializes the Noise Gate using a dB threshold.
        /// </summary>
        /// <param name="thresholdDb">Threshold in dB. -45f is general VoIP standard, -35f is tighter.</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public NoiseGateEffect(float thresholdDb, float sampleRate)
        {
            float clampedDb = Clamp(thresholdDb, -96f, 0f);
            _thresholdLinear = (float)Math.Pow(10, clampedDb / 20.0);

            // Studio standard presets: Attack=2ms, Hold=100ms, Release=200ms
            _attackCoef = (float)Math.Exp(-1000.0 / (2f * sampleRate));
            _releaseCoef = (float)Math.Exp(-1000.0 / (200f * sampleRate));
            _holdSamples = (int)(sampleRate * (100f / 1000f));
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1) return;

            for (int i = 0; i < length; i++)
            {
                float sampleAbs = Math.Abs(pcm[i]);

                if (sampleAbs > _envelope)
                    _envelope = _attackCoef * _envelope + (1f - _attackCoef) * sampleAbs;
                else
                    _envelope = _releaseCoef * _envelope + (1f - _releaseCoef) * sampleAbs;

                float targetGain;
                if (_envelope >= _thresholdLinear)
                {
                    targetGain = 1f;
                    _holdCounter = _holdSamples;
                }
                else
                {
                    if (_holdCounter > 0)
                    {
                        _holdCounter--;
                        targetGain = 1f;
                    }
                    else
                    {
                        targetGain = 0f;
                    }
                }

                if (targetGain > _currentGain)
                    _currentGain = _attackCoef * _currentGain + (1f - _attackCoef) * targetGain;
                else
                    _currentGain = _releaseCoef * _currentGain + (1f - _releaseCoef) * targetGain;

                pcm[i] *= _currentGain;
            }
        }
    }
}