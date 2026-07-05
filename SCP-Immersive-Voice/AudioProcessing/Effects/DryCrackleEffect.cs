namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    ///  Dry, brittle crackle simulating bone friction, snapping joints or decayed tissue.
    /// Employs an ultra-fast bitwise LCG randomizer, amplitude envelope tracking, 
    /// and a high-frequency Biquad High-Pass filter to isolate crisp transients. Zero allocations.
    /// </summary>
    public class DryCrackleEffect : IAudioEffect
    {
        public string Name => "Dry Crackle";

        private readonly float _amount;

        // Stack-allocated High-Pass filter to strip body and leave only crisp, dry snaps
        private BiquadFilter _brittleFilter;

        // Stateful parameters for LCG and envelope tracking
        private float _envelope = 0f;
        private uint _lcgState;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        /// <summary>
        /// Initializes the Dry Crackle effect.
        /// </summary>
        /// <param name="amount">Density and volume of the dry crackle layer (0.0f to 1.5f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public DryCrackleEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1.5f);
            float sr = sampleRate > 0f ? sampleRate : 48000f;

            // Seed the fast LCG using a unique instance hash
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Configure the filter as a High-Pass at 4000Hz to enforce a dusty, brittle texture
            _brittleFilter.ConfigureHighPass(4000f, sr, q: 1.0f);

            // Sample-rate independent envelope tracking
            _envAttackCoef = (float)Math.Exp(-1000.0 / (4f * sr));   // 4ms attack
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (50f * sr)); // 50ms release
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            float crackleIntensity = _amount * 0.28f;

            for (int i = 0; i < length; i++)
            {
                float drySample = pcm[i];

                // 1. Track voice amplitude envelope
                float absInput = Math.Abs(drySample);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                // 2. Ultra-fast bitwise LCG Randomizer (1 CPU cycle cost)
                _lcgState = _lcgState * 1103515245 + 12345;

                // 3. Stochastic trigger threshold driven exponentially by vocal envelope
                float triggerChance = 0.0003f + (_envelope * 0.038f * _amount);
                uint maxThreshold = (uint)(triggerChance * uint.MaxValue);

                float impulse = 0f;
                if (_lcgState < maxThreshold)
                {
                    // Generate a sharp, instantaneous bipolar click
                    float randSign = ((_lcgState & 0x200) != 0) ? 1f : -1f;
                    impulse = randSign * (0.4f + (_envelope * 0.6f));
                }

                // 4. Filter the impulse to retain only the high-frequency brittle "snap"
                float dryTexture = _brittleFilter.Process(impulse);

                // 5. Inject the dry bone/crackle layer into the primary stream
                pcm[i] = drySample + (dryTexture * crackleIntensity);
            }
        }

        // High-performance, stack-allocated 2nd order IIR filter structure
        private struct BiquadFilter
        {
            private float _b0, _b1, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public void ConfigureHighPass(float cutoffFrequency, float sampleRate, float q)
            {
                float w0 = 2f * (float)Math.PI * cutoffFrequency / sampleRate;
                float alpha = (float)Math.Sin(w0) / (2f * q);
                float cosW0 = (float)Math.Cos(w0);

                float a0 = 1f + alpha;
                _b0 = ((1f + cosW0) / 2f) / a0;
                _b1 = -(1f + cosW0) / a0;
                _b2 = ((1f + cosW0) / 2f) / a0;
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