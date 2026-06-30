namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Stateful Noise Gate utilizing energy-based RMS envelope tracking.
    /// Integrates signal power over time to smoothly transition states and protect vocal sibilants.
    /// </summary>
    public class NoiseGateEffect : IAudioEffect
    {
        public string Name => "Noise Gate";

        // INTENT: Field made non-readonly to permit high-frequency scalar reflection injections from the session pipeline.
        private float _thresholdLinearSquared;
        private readonly float _attackCoef;
        private readonly float _releaseCoef;
        private readonly int _holdSamples;

        private float _envelope = 0f;
        private float _currentGain = 1f;
        private int _holdCounter = 0;

        public NoiseGateEffect(float thresholdDb, float sampleRate)
        {
            float clampedDb = thresholdDb < -96f ? -96f : (thresholdDb > 0f ? 0f : thresholdDb);
            float thresholdLinear = (float)Math.Pow(10, clampedDb / 20.0);

            // INTENT: Square the initial threshold to allow direct raw power comparisons, bypassing Math.Sqrt overhead.
            _thresholdLinearSquared = thresholdLinear * thresholdLinear;

            // Studio integration constants: Attack = 2ms, Hold = 120ms (expanded for sibilants), Release = 200ms
            _attackCoef = (float)Math.Exp(-1000.0 / (2f * sampleRate));
            _releaseCoef = (float)Math.Exp(-1000.0 / (200f * sampleRate));
            _holdSamples = (int)(sampleRate * (120f / 1000f));
        }

        public void Process(float[] pcm, int length)
        {
            if (pcm == null || length < 1) return;

            for (int i = 0; i < length; i++)
            {
                // INTENT: Square the input sample to track absolute energy metrics (RMS) rather than raw voltage 
                // peaks, preventing gate stutter and chatter during zero-crossings and quieter consonants.
                float samplePower = pcm[i] * pcm[i];

                if (samplePower > _envelope)
                    _envelope = _attackCoef * _envelope + (1f - _attackCoef) * samplePower;
                else
                    _envelope = _releaseCoef * _envelope + (1f - _releaseCoef) * samplePower;

                float targetGain;
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

                // INTENT: Smooth the control voltage gain adjustments to completely insulate the output from digital click transients.
                if (targetGain > _currentGain)
                    _currentGain = _attackCoef * _currentGain + (1f - _attackCoef) * targetGain;
                else
                    _currentGain = _releaseCoef * _currentGain + (1f - _releaseCoef) * targetGain;

                pcm[i] *= _currentGain;
            }
        }
    }
}