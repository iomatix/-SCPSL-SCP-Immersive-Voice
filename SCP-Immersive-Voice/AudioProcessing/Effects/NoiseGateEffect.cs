using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Stateful Noise Gate utilizing energy-based RMS envelope tracking.
    /// Integrates signal power over time to smoothly transition states and protect vocal sibilants.
    /// </summary>
    public class NoiseGateEffect : IAdjustableAudioEffect
    {
        #region Private Operational Properties
        private float _thresholdLinearSquared;

        private readonly float _attackCoef;
        private readonly float _releaseCoef;
        private readonly int _holdSamples;

        // Stateful parameters for low-level register synchronization managed via local stack windows
        private float _envelope;
        private float _currentGain;
        private int _holdCounter;
        #endregion

        #region Public Metadata Properties
        public string Name => "Noise Gate";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseGateEffect"/> class.
        /// </summary>
        /// <param name="thresholdDb">The logarithmic noise floor cutoff threshold in decibels (-96dB to 0dB).</param>
        /// <param name="sampleRate">The engine sample rate from VoiceChatSettings.</param>
        public NoiseGateEffect(float thresholdDb, float sampleRate)
        {
            float rate = sampleRate > 0f ? sampleRate : 48000f;

            // FLUENT API ALIGNMENT: Utilizing pristine clamping and decibel transforms straight from your extensions matrix
            float thresholdLinear = thresholdDb.Clamp(-96f, 0f).DbToLinear();

            // INTENT: Square the initial threshold to allow direct raw power comparisons, bypassing Math.Sqrt overhead.
            _thresholdLinearSquared = thresholdLinear * thresholdLinear;

            // Studio integration constants: Attack = 2ms, Hold = 120ms (expanded for sibilants), Release = 200ms
            // PERFORMANCE FIX: Shifted coefficient mappings to float-native Mathf execution structures
            _attackCoef = Mathf.Exp(-1000f / (2f * rate));
            _releaseCoef = Mathf.Exp(-1000f / (200f * rate));
            _holdSamples = (int)(rate * (120f / 1000f));

            _envelope = 0f;
            _currentGain = 1f;
            _holdCounter = 0;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1) return;

            // Caching volatile state tracking fields directly onto local CPU stack registers.
            // Eradicates L1/L2 cache pointer chasing to guarantee extreme packet processing speed.
            float localEnvelope = _envelope;
            float localCurrentGain = _currentGain;
            int localHoldCounter = _holdCounter;

            float att = _attackCoef;
            float rel = _releaseCoef;
            float threshSq = _thresholdLinearSquared;
            int holdSampl = _holdSamples;

            for (int i = 0; i < length; i++)
            {
                // INTENT: Square the input sample to track absolute energy metrics (RMS) rather than raw voltage 
                // peaks, preventing gate stutter and chatter during zero-crossings and quieter consonants.
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

                // INTENT: Smooth the control voltage gain adjustments to completely insulate the output from digital click transients.
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

            // Flush computed stack modifications back into object persistent instance trackers atomically post sweep.
            _envelope = localEnvelope;
            _currentGain = localCurrentGain;
            _holdCounter = localHoldCounter;
        }
        #endregion

        #region Operational Parameter Adjustments
        public void AdjustParameter(float value)
        {
            // Safely clamp and convert logarithmic decibel input directly within the capsule boundary
            float thresholdLinear = value.Clamp(-96f, 0f).DbToLinear();
            _thresholdLinearSquared = thresholdLinear * thresholdLinear;
        }
        #endregion
    }
}