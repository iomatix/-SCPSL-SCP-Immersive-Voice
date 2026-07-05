namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    ///  Digital Glitch-Burst and Buffer Fracture Engine tailored for SCP-079.
    /// Employs sample-rate independent millisecond timing, a thread-safe local LCG, 
    /// sample-and-hold stutter quantization, and an optimized digital foldback waveshaper.
    /// </summary>
    public class GlitchBurstEffect : IAudioEffect
    {
        public string Name => "Glitch Burst";

        private readonly float _amount;
        private readonly float _sampleRate;

        // Stateful parameters for block tracking
        private int _burstSamplesRemaining = 0;
        private int _burstTotalSamples = 0;
        private float _glitchHoldSample = 0f;
        private int _stutterHoldCounter = 0;

        // Local thread-isolated high-speed LCG
        private uint _lcgState;

        /// <summary>
        /// Initializes the Glitch Burst effect.
        /// </summary>
        /// <param name="amount">Probability and brutality of digital frame fractures (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public GlitchBurstEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            // Sample-rate independent trigger probability (scallop adjustments per second)
            // Higher amount scales how often a system processing interruption occurs
            float triggerProbabilityPerSample = (_amount * 1.8f) / _sampleRate;
            uint triggerThreshold = (uint)(triggerProbabilityPerSample * uint.MaxValue);

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Advance fast LCG random state sequence (1 CPU cycle cost)
                _lcgState = _lcgState * 1103515245 + 12345;

                // 2. Check for fresh glitch burst trigger if system is currently operating normally
                if (_burstSamplesRemaining <= 0)
                {
                    if (_lcgState < triggerThreshold)
                    {
                        // Calculate hardware lockup window in seconds (between 15ms and 65ms)
                        float randomMod = (float)(_lcgState & 0xFFFF) / 65535f;
                        float burstDurationMs = 15f + (randomMod * 50f * _amount);

                        _burstTotalSamples = (int)(_sampleRate * (burstDurationMs / 1000f));
                        _burstSamplesRemaining = _burstTotalSamples;
                        _stutterHoldCounter = 0;
                    }
                }

                // 3. Process the active digital fracture loop block
                if (_burstSamplesRemaining > 0)
                {
                    // Calculate a pristine, non-inverted linear decay envelope across execution window
                    float burstProgress = 1f - ((float)_burstSamplesRemaining / _burstTotalSamples);
                    float env = 1f - burstProgress;

                    // Fetch another raw LCG block for hardware data noise corruption
                    uint lcgData = _lcgState * 1103515245 + 12345;
                    float rawNoise = ((float)(lcgData & 0xFFFF) / 65535f) * 2f - 1f;

                    // 4. Sample-and-Hold Stutter Quantization (Simulates clock transmission drop)
                    // Freezes and replicates a micro-grain segment of the audio wave
                    int holdPeriodSamples = 4 + (int)(_amount * 12f);
                    if (_stutterHoldCounter <= 0)
                    {
                        // Blend real data with corrupt white noise nodes
                        _glitchHoldSample = (drySample * 0.4f) + (rawNoise * 0.6f);
                        _stutterHoldCounter = holdPeriodSamples;
                    }
                    _stutterHoldCounter--;

                    // 5. High-performance Digital Foldback Distortion (Wave reflection mirroring)
                    // Forces severe digital breakage by flipping clipping points backwards
                    float inputDrive = _glitchHoldSample * (1.5f + _amount * 2.5f);

                    // Optimized foldback logic without expensive modulo loops
                    float foldedGlitch;
                    if (inputDrive > 1.0f)
                        foldedGlitch = 2.0f - inputDrive;
                    else if (inputDrive < -1.0f)
                        foldedGlitch = -2.0f - inputDrive;
                    else
                        foldedGlitch = inputDrive;

                    // 6. Crossfade the generated system fracture with original stream based on envelope node
                    float wetMix = _amount * 0.45f;
                    if (wetMix > 0.65f) wetMix = 0.65f;

                    float glitchOutput = foldedGlitch * env;
                    pcm[i] = (drySample * (1f - wetMix)) + (glitchOutput * wetMix);

                    _burstSamplesRemaining--;
                }
            }
        }
    }
}