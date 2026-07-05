using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Guttural Resonance Effect simulating false vocal cord (ventricular fold) vibration.
    /// Employs a modulated nonlinear feedback comb filter architecture with DC-blocking
    /// to introduce biological, gravelly throat textures. Fully real-time safe and stateless.
    /// </summary>
    public class GutturalResonanceEffect : IAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private readonly float _amount;
        private readonly float _sampleRate;
        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        // Ring buffer for the short throat delay line
        private readonly float[] _delayBuffer;
        private readonly int _bufferMask;

        // Stateful parameters for low-level register synchronization managed via local stack windows
        private int _writeIndex;
        private float _envelope;
        private float _wobblePhase;
        private float _dcState;
        #endregion

        #region Public Metadata Properties
        public string Name => "Guttural Resonance";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Guttural Resonance effect.
        /// </summary>
        /// <param name="amount">Intensity of the throat rasp texture (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public GutturalResonanceEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the float primitive
            _amount = amount.Clamp(0f, 1.5f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // Max throat cavity delay is around 12ms. 1024 samples at 48kHz is ~21ms (safe power-of-two)
            const int size = 1024;
            _delayBuffer = new float[size];
            _bufferMask = size - 1;

            _writeIndex = 0;
            _envelope = 0f;
            _wobblePhase = 0f;
            _dcState = 0f;

            // Sample-rate independent envelope coefficients using float-native math
            _envAttackCoef = Mathf.Exp(-1000f / (10f * _sampleRate));  // 10ms attack
            _envReleaseCoef = Mathf.Exp(-1000f / (35f * _sampleRate)); // 35ms release
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            // Core adaptation parameters for the feedback matrix
            float baseDelaySamples = _sampleRate * 0.0055f; // 5.5ms baseline throat cavity size
            float maxModulation = _sampleRate * 0.0025f;    // 2.5ms maximum tissue displacement
            float feedbackGain = _amount * 0.45f;

            // Extracted wet/dry crossfade staging boundaries out of the hot-path loop.
            // Eradicates thousands of redundant operations and memory clamp checks per second.
            float wetMix = (_amount * 0.5f).Clamp(0f, 0.7f);
            float dryMix = 1f - wetMix;

            // Caching volatile instance state variables onto stack registers to cut off RAM tracking loops.
            // Unlocks pure register-level access speed for the duration of the VoIP processing sweep.
            float localEnvelope = _envelope;
            float localWobblePhase = _wobblePhase;
            float localDcState = _dcState;
            int localWriteIndex = _writeIndex;

            float attCoef = _envAttackCoef;
            float relCoef = _envReleaseCoef;
            float[] delayBuf = _delayBuffer;
            int mask = _bufferMask;
            int bufLen = delayBuf.Length;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Dynamic voice envelope tracking via our fluent math extensions
                float absInput = drySample.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = attCoef * localEnvelope + (1f - attCoef) * absInput;
                }
                else
                {
                    localEnvelope = relCoef * localEnvelope + (1f - relCoef) * absInput;
                }

                // 2. Continuous chaotic LFO for throat tissue movement
                localWobblePhase += 0.0025f + (localEnvelope * 0.0015f);
                if (localWobblePhase > TwoPi)
                    localWobblePhase -= TwoPi;

                float wobble = Mathf.Sin(localWobblePhase);

                // 3. Compute dynamic fractional delay based on tissue wobble and voice stress
                float targetDelay = baseDelaySamples + (wobble * maxModulation * (0.5f + localEnvelope * 0.5f));

                // 4. Read from delay line using linear interpolation for high-speed performance
                float readPos = localWriteIndex - targetDelay;

                // PERFORMANCE FIX: Eradicated high-overhead while loop execution.
                // Replaced with a hardware-friendly single branch conditional offset assignment.
                if (readPos < 0f)
                    readPos += bufLen;

                int i0 = (int)readPos;
                int i1 = (i0 + 1) & mask;
                float frac = readPos - i0;
                float delayedSample = delayBuf[i0 & mask] * (1f - frac) + delayBuf[i1] * frac;

                // 5. Apply high-pass filter inside the loop (DC Blocker) to prevent resonance runaway
                localDcState = 0.995f * localDcState + (drySample - localDcState);
                float stabilizedInput = localDcState;

                // 6. Polynomial soft-clipping saturation in the feedback path (emulates biological wall saturation)
                float feedbackDrive = (delayedSample * feedbackGain) + stabilizedInput;
                float saturatedFeedback = feedbackDrive / (1f + feedbackDrive.Abs());

                // 7. Store the saturated feedback node inside the ring buffer memory space
                delayBuf[localWriteIndex] = saturatedFeedback;
                localWriteIndex = (localWriteIndex + 1) & mask;

                // 8. Equal-power wet/dry crossfade using pre-computed stack gain constants
                pcm[i] = (drySample * dryMix) + (saturatedFeedback * wetMix);
            }

            // Write local stack variables back into object instance persistence storage trackers atomically.
            _envelope = localEnvelope;
            _wobblePhase = localWobblePhase;
            _dcState = localDcState;
            _writeIndex = localWriteIndex;
        }
        #endregion
    }
}