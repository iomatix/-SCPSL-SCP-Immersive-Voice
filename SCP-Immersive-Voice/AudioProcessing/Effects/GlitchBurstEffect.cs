using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Digital Glitch-Burst and Buffer Fracture Engine tailored for SCP-079.
    /// Employs sample-rate independent millisecond timing, a thread-safe local LCG, 
    /// sample-and-hold stutter quantization, and an optimized digital foldback waveshaper.
    /// </summary>
    public class GlitchBurstEffect : IAudioEffect
    {
        #region Private Execution Vectors
        private readonly float _amount;
        private readonly float _sampleRate;

        // Stateful parameters for block tracking managed inside stack register windows
        private int _burstSamplesRemaining;
        private int _burstTotalSamples;
        private float _glitchHoldSample;
        private int _stutterHoldCounter;

        // Local thread-isolated high-speed LCG state
        private uint _lcgState;
        #endregion

        #region Public Metadata Properties
        public string Name => "Glitch Burst";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Glitch Burst effect.
        /// </summary>
        /// <param name="amount">Probability and brutality of digital frame fractures (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public GlitchBurstEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the argument payload
            _amount = amount.Clamp(0f, 1.5f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            _burstSamplesRemaining = 0;
            _burstTotalSamples = 0;
            _glitchHoldSample = 0f;
            _stutterHoldCounter = 0;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            // Sample-rate independent trigger probability (scallop adjustments per second)
            float triggerProbabilityPerSample = (_amount * 1.8f) / _sampleRate;
            uint triggerThreshold = (uint)(triggerProbabilityPerSample * uint.MaxValue);

            // Extracted the wet mix calculation and boundary limitations out of the sample sweep block.
            // Prevents thousands of redundant scaling computations per voice buffer packet.
            float wetMix = (_amount * 0.45f).Clamp(0f, 0.65f);
            float dryMix = 1f - wetMix;

            // Caching volatile instance state variables onto stack registers to cut off RAM/Sterta reference chasing.
            // Accelerates hot-path loop throughput execution down to raw hardware execution profiles.
            int localRemaining = _burstSamplesRemaining;
            int localTotal = _burstTotalSamples;
            float localHoldSample = _glitchHoldSample;
            int localStutterCounter = _stutterHoldCounter;
            uint localLcgState = _lcgState;

            float rate = _sampleRate;
            float amtScalar = _amount;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Advance fast LCG random state sequence (1 CPU cycle cost execution)
                localLcgState = localLcgState * 1103515245 + 12345;

                // 2. Check for fresh glitch burst trigger if system is currently operating normally
                if (localRemaining <= 0)
                {
                    if (localLcgState < triggerThreshold)
                    {
                        // Calculate hardware lockup window in milliseconds (between 15ms and 65ms)
                        float randomMod = (float)(localLcgState & 0xFFFF) / 65535f;
                        float burstDurationMs = 15f + (randomMod * 50f * amtScalar);

                        localTotal = (int)(rate * (burstDurationMs / 1000f));
                        localRemaining = localTotal;
                        localStutterCounter = 0;
                    }
                }

                // 3. Process the active digital fracture loop block
                if (localRemaining > 0)
                {
                    // Calculate a pristine, non-inverted linear decay envelope across execution window
                    float burstProgress = 1f - ((float)localRemaining / localTotal);
                    float env = 1f - burstProgress;

                    // Fetch another raw LCG block for hardware data noise corruption fields
                    uint lcgData = localLcgState * 1103515245 + 12345;
                    float rawNoise = ((float)(lcgData & 0xFFFF) / 65535f) * 2f - 1f;

                    // 4. Sample-and-Hold Stutter Quantization (Simulates clock transmission drop)
                    // Freezes and replicates a micro-grain segment of the audio wave
                    int holdPeriodSamples = 4 + (int)(amtScalar * 12f);
                    if (localStutterCounter <= 0)
                    {
                        // Blend real data with corrupt white noise nodes
                        localHoldSample = (drySample * 0.4f) + (rawNoise * 0.6f);
                        localStutterCounter = holdPeriodSamples;
                    }
                    localStutterCounter--;

                    // 5. High-performance Digital Foldback Distortion (Wave reflection mirroring)
                    // Forces severe digital breakage by flipping clipping points backwards
                    float inputDrive = localHoldSample * (1.5f + amtScalar * 2.5f);

                    // Optimized foldback logic without expensive modulo loops
                    float foldedGlitch;
                    if (inputDrive > 1.0f)
                    {
                        foldedGlitch = 2.0f - inputDrive;
                    }
                    else if (inputDrive < -1.0f)
                    {
                        foldedGlitch = -2.0f - inputDrive;
                    }
                    else
                    {
                        foldedGlitch = inputDrive;
                    }

                    // 6. Crossfade the generated system fracture with original stream based on envelope node
                    float glitchOutput = foldedGlitch * env;
                    pcm[i] = (drySample * dryMix) + (glitchOutput * wetMix);

                    localRemaining--;
                }
            }

            // Write computed local variables back into object instance context fields atomically post execution loop.
            _burstSamplesRemaining = localRemaining;
            _burstTotalSamples = localTotal;
            _glitchHoldSample = localHoldSample;
            _stutterHoldCounter = localStutterCounter;
            _lcgState = localLcgState;
        }
        #endregion
    }
}