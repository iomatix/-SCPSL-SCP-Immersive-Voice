namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Stateful Noise Gate with energy-based RMS envelope detection.
    /// Integrates signal power over time to prevent clipping during zero-crossings and sibilants.
    /// </summary>
    public class NoiseGateEffect : IAudioEffect
    {
        public string Name => "Noise Gate";

        private readonly float _thresholdLinearSquared; // Optimized to bypass Math.Sqrt in loop
        private readonly float _attackCoef;
        private readonly float _releaseCoef;
        private readonly int _holdSamples;

        private float _envelope = 0f;
        private float _currentGain = 1f;
        private int _holdCounter = 0;

        public NoiseGateEffect(float thresholdDb, float sampleRate)
        {
            float clampedDb = Clamp(thresholdDb, -96f, 0f);
            float thresholdLinear = (float)Math.Pow(10, clampedDb / 20.0);

            // INTENT: Store the squared threshold to evaluate raw power directly, saving CPU cycles.
            _thresholdLinearSquared = thresholdLinear * thresholdLinear;

            // Studio standard integration constraints: Attack=2ms, Hold=120ms (slightly expanded for sibilants), Release=200ms
            _attackCoef = (float)Math.Exp(-1000.0 / (2f * sampleRate));
            _releaseCoef = (float)Math.Exp(-1000.0 / (200f * sampleRate));
            _holdSamples = (int)(sampleRate * (120f / 1000f));
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1) return;

            for (int i = 0; i < length; i++)
            {
                // INTENT: Square the sample to capture total instantaneous power instead of absolute raw voltage peaks.
                float samplePower = pcm[i] * pcm[i];

                // Smooth the power envelope using leaky integrator time constants
                if (samplePower > _envelope)
                    _envelope = _attackCoef * _envelope + (1f - _attackCoef) * samplePower;
                else
                    _envelope = _releaseCoef * _envelope + (1f - _releaseCoef) * samplePower;

                float targetGain;
                // Evaluate power directly against the squared threshold matrix
                if (_envelope >= _thresholdLinearSquared)
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

                // Smooth the gain transition to eliminate digital waveform steps (clicks)
                if (targetGain > _currentGain)
                    _currentGain = _attackCoef * _currentGain + (1f - _attackCoef) * targetGain;
                else
                    _currentGain = _releaseCoef * _currentGain + (1f - _releaseCoef) * targetGain;

                pcm[i] *= _currentGain;
            }
        }
    }
}