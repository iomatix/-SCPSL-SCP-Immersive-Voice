namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    ///  Physical Modeling Stone Fracture Generator tailored for SCP-173.
    /// Excites a low-frequency high-Q modal concrete resonator matrix using sparse, explosive
    /// impulse streams, while actively cross-modulating the voice buffer to strip humanity.
    /// </summary>
    public class StoneCrackEffect : IAudioEffect
    {
        public string Name => "Stone Crack";

        private readonly float _intensity;
        private readonly float _sampleRate;

        private BiquadFilter _stoneBodyResonator;
        private BiquadFilter _brittleSurfaceResonator;

        private int _cascadeSamplesRemaining = 0;
        private int _ticksUntilNextSnap = 0;
        private float _cascadeEnergy = 0f;
        private float _envelope = 0f;
        private uint _lcgState;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        /// <summary>
        /// Initializes the structural Stone Crack engine.
        /// </summary>
        public StoneCrackEffect(float intensity, float sampleRate)
        {
            _intensity = Clamp(intensity, 0f, 2.0f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            //  FIX: Deep structural concrete mass resonance (220Hz) with extreme Q for solid metallic-rock thud
            _stoneBodyResonator.ConfigureBandPass(220f, _sampleRate, q: 38.0f);

            //  FIX: Mid-range stone surface cleavage fracture line (1100Hz) with high rigidity
            _brittleSurfaceResonator.ConfigureBandPass(1100f, _sampleRate, q: 16.0f);

            _envAttackCoef = (float)Math.Exp(-1000.0 / (3f * _sampleRate));
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (80f * _sampleRate));
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _intensity < 0.01f) return;

            //  FIX: Drastically lowered base frequency to produce rare, massive impacts instead of sandy grain buzz
            float baseTriggerChance = (0.0012f * _intensity) / _sampleRate;
            uint triggerThreshold = (uint)(baseTriggerChance * uint.MaxValue);

            for (int i = 0; i < length; i++)
            {
                float dryInput = pcm[i];

                float absInput = Math.Abs(dryInput);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                _lcgState = _lcgState * 1103515245 + 12345;
                float structuralImpulse = 0f;

                if (_cascadeSamplesRemaining <= 0)
                {
                    uint dynamicThreshold = (uint)(triggerThreshold * (0.05f + _envelope * 2.4f));

                    if (_lcgState < dynamicThreshold)
                    {
                        float randVal = (float)(_lcgState & 0xFFFF) / 65535f;
                        // Long, heavy fault line failure cascades (60ms to 200ms)
                        float cascadeDurationMs = 60f + (randVal * 140f * _intensity);

                        _cascadeSamplesRemaining = (int)(_sampleRate * (cascadeDurationMs / 1000f));
                        _cascadeEnergy = 0.7f + (randVal * 0.5f); // High impact force
                        _ticksUntilNextSnap = 0;
                    }
                }

                if (_cascadeSamplesRemaining > 0)
                {
                    if (_ticksUntilNextSnap <= 0)
                    {
                        float snapSign = ((_lcgState & 0x2000) != 0) ? 1f : -1f;
                        structuralImpulse = snapSign * _cascadeEnergy;

                        //  FIX: Extended time gap (25ms to 115ms) to separate impulses into hard, heavy macroscopic events
                        uint lcgBits = _lcgState * 1103515245 + 12345;
                        float deltaMod = (float)(lcgBits & 0xFFFF) / 65535f;
                        float timeGapMs = 25f + (deltaMod * 90f);

                        _ticksUntilNextSnap = (int)(_sampleRate * (timeGapMs / 1000f));
                        _cascadeEnergy *= 0.78f; // Slow decay rate to let the structural energy echo
                    }

                    _ticksUntilNextSnap--;
                    _cascadeSamplesRemaining--;
                }

                float bodyResonance = _stoneBodyResonator.Process(structuralImpulse);
                float surfaceResonance = _brittleSurfaceResonator.Process(structuralImpulse);

                // Emphasize the deep concrete sub-thud (80% body mass, 20% surface cleave)
                float combinedStoneCrack = (bodyResonance * 0.8f) + (surfaceResonance * 0.2f);

                float drivenCrack = combinedStoneCrack * 5.0f;
                float hardShapedCrack = drivenCrack / (1f + Math.Abs(drivenCrack));

                float wetMix = _intensity * 0.65f;
                if (wetMix > 0.85f) wetMix = 0.85f;

                //  FIX (Vocal Shredder Cross-Modulation): 
                // The physical fracture actively deconstructs and cancels out the phase of the human voice stream.
                // When stone breaks, human vocal identity is suppressed and shredded.
                float voiceDestructionFactor = 1.0f - (Math.Abs(hardShapedCrack) * 1.6f);
                if (voiceDestructionFactor < -0.3f) voiceDestructionFactor = -0.3f;

                pcm[i] = (dryInput * voiceDestructionFactor) + (hardShapedCrack * wetMix);
            }
        }

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
                _x2 = _x1; _x1 = input; _y2 = _y1; _y1 = output;
                return output;
            }
        }
    }
}