using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Stateful Noise Gate utilizing energy-based RMS envelope tracking.
    /// Integrates signal power over time to smoothly transition states and protect vocal sibilants.
    /// </summary>
    namespace SCP_Immersive_Voice.AudioProcessing.Effects
    {
        public class NoiseGateEffect : IAdjustableAudioEffect
        {
            #region Private Operational Properties
            private float _thresholdLinearSquared;

            private readonly float _attackCoef;
            private readonly float _releaseCoef;
            private readonly int _holdSamples;

            private float _envelope;
            private float _currentGain;
            private int _holdCounter;
            #endregion

            public string Name => "Noise Gate";

            public NoiseGateEffect(float thresholdDb, float sampleRate)
            {
                float rate = sampleRate > 0f ? sampleRate : 48000f;

                // Softened baseline initialization
                float thresholdLinear = thresholdDb.Clamp(-96f, 0f).DbToLinear();
                _thresholdLinearSquared = thresholdLinear * thresholdLinear;

                _attackCoef = Mathf.Exp(-1000f / (2f * rate));
                _releaseCoef = Mathf.Exp(-1000f / (200f * rate));

                // OPTIMIZATION: Expanded hardware hold constraint to 180ms to guarantee complete protection for sibilants and quiet trails
                _holdSamples = (int)(rate * (180f / 1000f));

                _envelope = 0f;
                _currentGain = 1f;
                _holdCounter = 0;
            }

            public void Process(float[] pcm, int length)
            {
                if (pcm is null || length < 1) return;

                float localEnvelope = _envelope;
                float localCurrentGain = _currentGain;
                int localHoldCounter = _holdCounter;

                float att = _attackCoef;
                float rel = _releaseCoef;
                float threshSq = _thresholdLinearSquared;
                int holdSampl = _holdSamples;

                for (int i = 0; i < length; i++)
                {
                    float samplePower = pcm[i] * pcm[i];

                    if (samplePower > localEnvelope)
                    {
                        localEnvelope = att * localEnvelope + (1f - att) * samplePower;
                    }
                    else
                    {
                        localEnvelope = rel * localEnvelope + (1f - rel) * samplePower;
                    }

                    float targetGain;
                    if (localEnvelope >= threshSq)
                    {
                        targetGain = 1f;
                        localHoldCounter = holdSampl;
                    }
                    else
                    {
                        if (localHoldCounter > 0)
                        {
                            localHoldCounter--;
                            targetGain = 1f;
                        }
                        else
                        {
                            targetGain = 0f;
                        }
                    }

                    if (targetGain > localCurrentGain)
                    {
                        localCurrentGain = att * localCurrentGain + (1f - att) * targetGain;
                    }
                    else
                    {
                        localCurrentGain = rel * localCurrentGain + (1f - rel) * targetGain;
                    }

                    pcm[i] *= localCurrentGain;
                }

                _envelope = localEnvelope;
                _currentGain = localCurrentGain;
                _holdCounter = localHoldCounter;
            }

            public void AdjustParameter(float value)
            {
                float thresholdLinear = value.Clamp(-96f, 0f).DbToLinear();
                _thresholdLinearSquared = thresholdLinear * thresholdLinear;
            }
        }
    }
}