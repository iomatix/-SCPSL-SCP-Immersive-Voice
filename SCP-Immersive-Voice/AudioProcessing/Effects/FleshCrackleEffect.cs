namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    ///  Wet, organic crackle simulating tearing flesh and cellular tissue snapping.
    /// Employs an ultra-fast LCG randomizer, voice amplitude envelope tracking, 
    /// and an excited lossy bandpass resonator matrix. Zero allocations, real-time safe.
    /// </summary>
    public class FleshCrackleEffect : IAudioEffect
    {
        public string Name => "Flesh Crackle";

        private readonly float _amount;

        // Stack-allocated biquad resonator to shape raw impulses into wet squelches
        private BiquadFilter _wetResonator;

        // Stateful parameters for envelope and LCG random seed
        private float _envelope = 0f;
        private uint _lcgState;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        /// <summary>
        /// Initializes the Flesh Crackle effect.
        /// </summary>
        /// <param name="amount">Density and intensity of the flesh crackle layer (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public FleshCrackleEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            // Seed the fast LCG using a unique identifier hash
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Configure the resonator filter to the acoustic zone of wet biological tissue (1600 Hz)
            // High Q creates an organic, fluid-like damped ringing effect
            _wetResonator.ConfigureBandPass(1600f, sr, q: 4.5f);

            // Sample-rate independent envelope coefficients
            _envAttackCoef = (float)Math.Exp(-1000.0 / (5f * sr));   // 5ms attack
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (60f * sr)); // 60ms release
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            // Global intensity scalar based on preset amount
            float crackleIntensity = _amount * 0.35f;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Track voice amplitude envelope
                float absInput = Math.Abs(drySample);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                // 2. Ultra-fast bitwise LCG Random Number Generator (1 CPU cycle cost)
                _lcgState = _lcgState * 1103515245 + 12345;

                // 3. Envelope-driven stochastic trigger threshold
                // Higher vocal volume exponentially scales the probability of a tissue snap
                float triggerChance = 0.0005f + (_envelope * 0.045f * _amount);
                uint maxThreshold = (uint)(triggerChance * uint.MaxValue);

                float impulse = 0f;
                if (_lcgState < maxThreshold)
                {
                    // Generate a rapid, high-energy bidirectional spike
                    // Re-use LCG bits for internal bipolar amplitude decoration
                    float randSign = ((_lcgState & 0x100) != 0) ? 1f : -1f;
                    impulse = randSign * (0.3f + (_envelope * 0.7f));
                }

                // 4. Excite the biological resonator filter with the generated impulse
                float wetTexture = _wetResonator.Process(impulse);

                // 5. Accumulate the wet wet-flesh crackle layer into the original audio stream
                pcm[i] = drySample + (wetTexture * crackleIntensity);
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