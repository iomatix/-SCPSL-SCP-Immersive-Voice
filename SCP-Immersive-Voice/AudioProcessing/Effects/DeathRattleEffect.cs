namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// Stateful physical-modeling DSP effect simulating liquid pooling in a decaying trachea.
    /// Employs a voice-envelope driven stochastic LCG bubble generator and a modulated viscous 
    /// comb filter to synthesize realistic death rattle choking textures. Zero heap allocations.
    /// </summary>
    public class DeathRattleEffect : IAudioEffect
    {
        public string Name => "Death Rattle";

        private readonly float _amount;
        private readonly float _sampleRate;

        // Ultra-short comb filter ring buffer modeling viscous throat fluid reflections (Max 4ms delay)
        private const int CombSize = 256;
        private const int CombMask = CombSize - 1;
        private readonly float[] _combBuffer = new float[CombSize];
        private int _writePtr = 0;

        // Stateful tracking parameters
        private float _envelope = 0f;
        private float _bubblePhase = 0f;
        private uint _lcgState;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;
        private const float TwoPi = (float)(Math.PI * 2.0);

        /// <summary>
        /// Initializes a new instance of the <see cref="DeathRattleEffect"/> class.
        /// </summary>
        /// <param name="amount">The wet mix intensity of the choking rattle (0.0f to 1.0f).</param>
        /// <param name="sampleRate">The audio engine sample rate from VoiceChatSettings.</param>
        public DeathRattleEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            _envAttackCoef = (float)Math.Exp(-1000.0 / (6f * _sampleRate));   // Fast 6ms tracking
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (55f * _sampleRate)); // 55ms release smoothing
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            float wetMix = _amount * 0.75f;
            float dryMix = 1f - (wetMix * 0.35f);

            // Baseline viscous comb delay boundaries (0.5ms to 2.2ms)
            float baseDelaySamples = _sampleRate * 0.0005f;
            float modDepthSamples = _sampleRate * 0.0017f;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // 1. Track voice amplitude envelope
                float absInput = Math.Abs(dry);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                // 2. Advance fast local LCG sequence
                _lcgState = _lcgState * 1103515245 + 12345;
                float jitter = ((float)(_lcgState & 0xFFFF) / 65535f) * 0.15f;

                // 3. Dynamic Stochastic LFO (Liquid Bubbling Engine)
                // Bubbling frequency scales exponentially with vocal intensity (14Hz up to 38Hz)
                float bubbleFrequency = 14f + (_envelope * 24f) + (jitter * 6f);
                _bubblePhase += (TwoPi * bubbleFrequency) / _sampleRate;
                if (_bubblePhase > TwoPi) _bubblePhase -= TwoPi;

                // Generate a chaotic amplitude modulation bubble wave
                float bubbleWave = (float)Math.Sin(_bubblePhase);

                // 4. Store current sample inside the viscous comb circular buffer
                _combBuffer[_writePtr] = dry;

                // 5. Compute dynamic fractional delay based on bubble wave state
                float targetDelay = baseDelaySamples + ((0.5f + 0.5f * bubbleWave) * modDepthSamples);
                float readPos = _writePtr - targetDelay;
                while (readPos < 0f) readPos += CombSize;

                // Sub-sample linear interpolation to prevent click artifacts
                int i0 = (int)readPos;
                int i1 = (i0 + 1) & CombMask;
                float fraction = readPos - i0;
                float delayedSample = _combBuffer[i0 & CombMask] * (1f - fraction) + _combBuffer[i1] * fraction;

                // 40% feedback coefficient models viscous fluid dampening and acoustic scattering
                float combFilteredOutput = dry + (delayedSample * 0.40f);

                // Commit comb node back to circular memory
                _combBuffer[_writePtr] = combFilteredOutput;
                _writePtr = (_writePtr + 1) & CombMask;

                // 6. Dynamic Amplitude Shuttering: The bubble wave physically blocks the voice amplitude
                float shutteredWet = combFilteredOutput * (0.45f + (0.55f * bubbleWave));

                // 7. Fast polynomial soft-clipping to prevent gain spikes
                float driven = shutteredWet * 1.35f;
                float saturatedWet = driven / (1f + Math.Abs(driven));

                pcm[i] = (dry * dryMix) + (saturatedWet * wetMix);
            }
        }
    }
}