namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA Physical Modeling Stone Fracture Generator tailored for SCP-173.
    /// Replaces white noise loops with discrete stochastic impulse streams (Dirac deltas) 
    /// exciting a dual-band high-Q modal concrete resonator matrix. Zero allocations.
    /// </summary>
    public class StoneCrackEffect : IAudioEffect
    {
        public string Name => "Stone Crack";

        private readonly float _intensity;
        private readonly float _sampleRate;

        // Dual-band modal resonators simulating the acoustic body of structural concrete
        private BiquadFilter _stoneBodyResonator;
        private BiquadFilter _brittleSurfaceResonator;

        // Stateful parameters for fracture cascade propagation
        private int _cascadeSamplesRemaining = 0;
        private int _ticksUntilNextSnap = 0;
        private float _cascadeEnergy = 0f;
        private float _envelope = 0f;

        // Fast thread-safe local LCG seed
        private uint _lcgState;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        /// <summary>
        /// Initializes the Stone Crack effect.
        /// </summary>
        /// <param name="intensity">Density and acoustic power of stone structural fractures (0.0f to 2.0f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public StoneCrackEffect(float intensity, float sampleRate)
        {
            _intensity = Clamp(intensity, 0f, 2.0f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Mode 1: Low-mid concrete mass resonance (1000Hz) with extremely high Q for rigid ring-decay
            _stoneBodyResonator.ConfigureBandPass(1000f, _sampleRate, q: 12.0f);

            // Mode 2: High-frequency brittle stone surface snapping (2800Hz) with moderate dampening
            _brittleSurfaceResonator.ConfigureBandPass(2800f, _sampleRate, q: 6.0f);

            // Sample-rate independent envelope follower constants
            _envAttackCoef = (float)Math.Exp(-1000.0 / (3f * _sampleRate));   // 3ms attack
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (80f * _sampleRate)); // 80ms release
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _intensity < 0.01f) return;

            // Scale the probability based on the configured preset intensity
            float baseTriggerChance = (0.006f * _intensity) / _sampleRate;
            uint triggerThreshold = (uint)(baseTriggerChance * uint.MaxValue);

            for (int i = 0; i < length; i++)
            {
                float dryInput = pcm[i];

                // 1. Voice envelope tracking (vocal energy represents mechanical stress applied to the stone)
                float absInput = Math.Abs(dryInput);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                // 2. Advance local LCG sequence (1 CPU cycle cost)
                _lcgState = _lcgState * 1103515245 + 12345;

                float structuralImpulse = 0f;

                // 3. Evaluate state machine for fracture cascade propagation
                if (_cascadeSamplesRemaining <= 0)
                {
                    // Scale the activation chance dynamically based on vocal stress
                    uint dynamicThreshold = (uint)(triggerThreshold * (0.1f + _envelope * 1.9f));

                    if (_lcgState < dynamicThreshold)
                    {
                        // Initialize a fresh structural crack sequence (lasting between 40ms and 120ms)
                        float randVal = (float)(_lcgState & 0xFFFF) / 65535f;
                        float cascadeDurationMs = 40f + (randVal * 80f * _intensity);

                        _cascadeSamplesRemaining = (int)(_sampleRate * (cascadeDurationMs / 1000f));
                        _cascadeEnergy = 0.4f + (randVal * 0.6f);
                        _ticksUntilNextSnap = 0;
                    }
                }

                // 4. Execute the active fracture propagation cascade
                if (_cascadeSamplesRemaining > 0)
                {
                    if (_ticksUntilNextSnap <= 0)
                    {
                        // EMIT AN INSTANTANEOUS DISCRETE DELTA IMPULSE (A true physical micro-crack)
                        float snapSign = ((_lcgState & 0x2000) != 0) ? 1f : -1f;
                        structuralImpulse = snapSign * _cascadeEnergy;

                        // Calculate time delay until the next internal crystal lattice snapping (2ms to 12ms)
                        uint lcgBits = _lcgState * 1103515245 + 12345;
                        float deltaMod = (float)(lcgBits & 0xFFFF) / 65535f;
                        float timeGapMs = 2f + (deltaMod * 10f);

                        _ticksUntilNextSnap = (int)(_sampleRate * (timeGapMs / 1000f));

                        // Decay the cascade energy exponentially as stress releases through the fault line
                        _cascadeEnergy *= 0.82f;
                    }

                    _ticksUntilNextSnap--;
                    _cascadeSamplesRemaining--;
                }

                // 5. Excite the dual-band concrete modal resonators using the sparse impulse
                float bodyResonance = _stoneBodyResonator.Process(structuralImpulse);
                float surfaceResonance = _brittleSurfaceResonator.Process(structuralImpulse);

                // Blend modes: 70% deep structural body energy + 30% brittle surface snap
                float combinedStoneCrack = (bodyResonance * 0.7f) + (surfaceResonance * 0.3f);

                // 6. Fast polynomial soft-clipping to add physical hardness and grit to the stone impact
                float drivenCrack = combinedStoneCrack * 3.5f;
                float hardShapedCrack = drivenCrack / (1f + Math.Abs(drivenCrack));

                // 7. Inject the modeled litosferyczne fracture layer into the live stream array
                float wetMix = _intensity * 0.4f;
                if (wetMix > 0.75f) wetMix = 0.75f;

                pcm[i] = dryInput + (hardShapedCrack * wetMix);
            }
        }

        // High-performance, stack-allocated 2nd order IIR filter structure
        private struct BiquadFilter
        {
            private float _b0, _b1, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public void ConfigureBandPass(float centerFrequency, float sampleRate, float q)
            {
                float w0 = 2f * (float)Math.PI * centerFrequency / sampleRate;
                float alpha = (float)Math.Sin(w0) / (2f * q);
                float cosW0 = (float)Math.Cos(w0);

                float a0 = 1f + alpha;
                _b0 = alpha / a0;
                _b1 = 0f;
                _b2 = -alpha / a0;
                _a1 = (-2f * cosW0) / a0;
                _a2 = (1f - alpha) / a0;
            }

            public float Process(float input)
            {
                float output = _b0 * input + _b1 * _x1 + _b2 * _x2 - _a1 * _y1 - _a2 * _y2;

                _x2 = _x1;
                _x1 = input;
                _y2 = _y1;
                _y1 = output;

                return output;
            }
        }
    }
}