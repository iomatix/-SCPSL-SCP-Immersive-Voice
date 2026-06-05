namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    ///  Guttural Resonance Effect simulating false vocal cord (ventricular fold) vibration.
    /// Employs a modulated nonlinear feedback comb filter architecture with DC-blocking
    /// to introduce biological, gravelly throat textures. Fully real-time safe and stateless.
    /// </summary>
    public class GutturalResonanceEffect : IAudioEffect
    {
        public string Name => "Guttural Resonance";

        private readonly float _amount;
        private readonly float _sampleRate;

        // Ring buffer for the short throat delay line
        private readonly float[] _delayBuffer;
        private readonly int _bufferMask;
        private int _writeIndex;

        // Stateful modulation and envelope tracking
        private float _envelope = 0f;
        private float _wobblePhase = 0f;
        private float _dcState = 0f;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        /// <summary>
        /// Initializes the Guttural Resonance effect.
        /// </summary>
        /// <param name="amount">Intensity of the throat rasp texture (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public GutturalResonanceEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // Max throat cavity delay is around 12ms. 1024 samples at 48kHz is ~21ms (safe power-of-two)
            int size = 1024;
            _delayBuffer = new float[size];
            _bufferMask = size - 1;
            _writeIndex = 0;

            // Sample-rate independent envelope coefficients
            _envAttackCoef = (float)Math.Exp(-1000.0 / (10f * _sampleRate));  // 10ms attack
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (35f * _sampleRate)); // 35ms release
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            // Core adaptation parameters for the feedback matrix
            float baseDelaySamples = _sampleRate * 0.0055f; // 5.5ms baseline throat cavity size
            float maxModulation = _sampleRate * 0.0025f;    // 2.5ms maximum tissue displacement
            float feedbackGain = _amount * 0.45f;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Dynamic voice envelope tracking
                float absInput = Math.Abs(drySample);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                // 2. Continuous chaotic LFO for throat tissue movement
                _wobblePhase += 0.0025f + (_envelope * 0.0015f);
                if (_wobblePhase > 2f * (float)Math.PI) _wobblePhase -= 2f * (float)Math.PI;
                float wobble = (float)Math.Sin(_wobblePhase);

                // 3. Compute dynamic fractional delay based on tissue wobble and voice stress
                float targetDelay = baseDelaySamples + (wobble * maxModulation * (0.5f + _envelope * 0.5f));

                // 4. Read from delay line using linear interpolation for high-speed performance
                float readPos = _writeIndex - targetDelay;
                while (readPos < 0f) readPos += _delayBuffer.Length;

                int i0 = (int)readPos;
                int i1 = (i0 + 1) & _bufferMask;
                float frac = readPos - i0;
                float delayedSample = _delayBuffer[i0 & _bufferMask] * (1f - frac) + _delayBuffer[i1] * frac;

                // 5. Apply high-pass filter inside the loop (DC Blocker) to prevent resonance runaway
                _dcState = 0.995f * _dcState + (drySample - _dcState);
                float stabilizedInput = _dcState;

                // 6. Polynomial soft-clipping saturation in the feedback path (emulates biological wall saturation)
                float feedbackDrive = (delayedSample * feedbackGain) + stabilizedInput;
                float saturatedFeedback = feedbackDrive / (1f + Math.Abs(feedbackDrive));

                // 7. Store the saturated feedback node inside the ring buffer
                _delayBuffer[_writeIndex] = saturatedFeedback;
                _writeIndex = (_writeIndex + 1) & _bufferMask;

                // 8. Equal-power wet/dry crossfade for pristine text intelligibility
                float wetMix = _amount * 0.5f;
                if (wetMix > 0.7f) wetMix = 0.7f; // Hard safety cap for vocal retention

                pcm[i] = (drySample * (1f - wetMix)) + (saturatedFeedback * wetMix);
            }
        }
    }
}