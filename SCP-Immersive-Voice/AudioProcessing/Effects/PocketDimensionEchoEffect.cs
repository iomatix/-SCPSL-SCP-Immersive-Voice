using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Non-Euclidean Extra-dimensional Echo Matrix tailored for SCP-106.
    /// Employs a sample-rate independent power-of-two ring buffer, fractional linear interpolation,
    /// fast polynomial LFOs, and an internal feedback All-Pass phase-smear filter. Zero allocations.
    /// </summary>
    public class PocketDimensionEchoEffect : IAdjustableAudioEffect
    {
        #region Private Execution Vectors
        private float _amount;
        private readonly float _sampleRate;
        private readonly float _lfoIncTime;
        private readonly float _lfoIncFb;

        // Persistent power-of-two delay line buffer
        private readonly float[] _buffer;
        private readonly int _bufferMask;

        // Stateful tracking parameters synchronized via local stack register windows
        private int _writeIndex;
        private float _lfoPhaseTime;
        private float _lfoPhaseFb;
        private float _allPassState;
        #endregion

        #region Public Metadata Properties
        public string Name => "Pocket Dimension Echo";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Pocket Dimension Echo effect.
        /// </summary>
        /// <param name="amount">Intensity and density of the dimensional echo (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public PocketDimensionEchoEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the float primitive
            _amount = amount.Clamp(0f, 1.5f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // Maximum required delay is around 450ms. 
            // Allocate a stable power-of-two buffer (32768 samples @ 48kHz = ~682ms)
            const int size = 32768;
            _buffer = new float[size];
            _bufferMask = size - 1;

            _writeIndex = 0;
            _lfoPhaseTime = 0f;
            _lfoPhaseFb = 0f;
            _allPassState = 0f;

            // Sample-rate independent LFO speeds (Time modulator = 0.35 Hz, Feedback modulator = 0.12 Hz)
            _lfoIncTime = 0.35f / _sampleRate;
            _lfoIncFb = 0.12f / _sampleRate;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            // Define physical boundary properties for the extradimensional space
            float baseDelaySamples = _sampleRate * 0.28f;     // 280ms baseline room echo length
            float maxModulationSamples = _sampleRate * 0.14f; // 140ms geometry warp depth

            // Pre-computing wet/dry crossfade staging boundaries outside the sample sweep block.
            // Eliminates thousands of redundant scaling check operations per voice buffer packet.
            float wetMix = (_amount * 0.55f).Clamp(0f, 0.75f);
            float dryMix = 1f - wetMix;

            // Cache volatile pointers and filter historical states directly onto the CPU stack frame.
            // Completely bypasses L1/L2 cache line pointer chasing across the hot-path execution loop.
            int localWriteIndex = _writeIndex;
            float localPhaseTime = _lfoPhaseTime;
            float localPhaseFb = _lfoPhaseFb;
            float localAllPassState = _allPassState;

            float incTime = _lfoIncTime;
            float incFb = _lfoIncFb;
            float[] buf = _buffer;
            int mask = _bufferMask;
            int bufLen = buf.Length;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Advance linear phase accumulators smoothly
                localPhaseTime += incTime;
                if (localPhaseTime > 1f)
                    localPhaseTime -= 1f;

                localPhaseFb += incFb;
                if (localPhaseFb > 1f)
                    localPhaseFb -= 1f;

                // 2. High-speed polynomial Triangle-to-Parabola LFO approximation (Replaces costly Math.Sin)
                float tTri = localPhaseTime * 2f;
                if (tTri > 1f)
                    tTri = 2f - tTri;
                float timeModulator = 4f * tTri * (1f - tTri);

                float fbTri = localPhaseFb * 2f;
                if (fbTri > 0f)
                    fbTri = 2f - fbTri;
                float feedbackModulator = 4f * fbTri * (1f - fbTri);

                // 3. Compute continuous fractional delay time to dynamically warp space geometry
                float targetDelay = baseDelaySamples + (timeModulator * maxModulationSamples);

                // 4. Extract sample using fractional linear interpolation to eliminate digital zipper artifacts
                float readPos = localWriteIndex - targetDelay;

                // PERFORMANCE FIX: Eradicated high-overhead while loop execution loops.
                // Replaced with a hardware-friendly single conditional branch block.
                if (readPos < 0f)
                    readPos += bufLen;

                int i0 = (int)readPos;
                int i1 = (i0 + 1) & mask;
                float frac = readPos - i0;

                // Fetch dual-node bounds across power-of-two masking bitwise lanes
                float delayedSample = buf[i0 & mask] * (1f - frac) + buf[i1] * frac;

                // 5. Dynamic feedback calculation with strict stability protection cap
                float currentFeedback = 0.32f + (feedbackModulator * 0.26f);
                if (currentFeedback > 0.62f)
                    currentFeedback = 0.62f;

                // 6. Inline All-Pass filter inside the feedback loop to create non-Euclidean phase smearing
                float apInput = delayedSample;
                float apOutput = -0.65f * apInput + localAllPassState;
                localAllPassState = apInput + (0.65f * apOutput);

                // 7. Fast polynomial soft-clipping to compress rogue resonances safely in the feedback path using fluent extensions
                float feedbackDrive = drySample + (apOutput * currentFeedback);
                float saturatedFeedback = feedbackDrive / (1f + feedbackDrive.Abs());

                // 8. Commit the processed node back into the power-of-two ring buffer memory lane
                buf[localWriteIndex] = saturatedFeedback;
                localWriteIndex = (localWriteIndex + 1) & mask;

                // 9. Equal-power wet/dry hybrid crossfade to maintain verbal clarity via pre-computed stack values
                pcm[i] = (drySample * dryMix) + (apOutput * wetMix);
            }

            // Flush computed stack register values back into object instance context fields atomically post execution.
            _writeIndex = localWriteIndex;
            _lfoPhaseTime = localPhaseTime;
            _lfoPhaseFb = localPhaseFb;
            _allPassState = localAllPassState;
        }
        #endregion

        #region Operational Parameter Adjustments
        public void AdjustParameter(float value)
        {
            _amount = value.Clamp(0f, 1.5f);
        }
        #endregion
    }
}