namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA Avian Syrinx Modeling Engine for creature chirps and flamingo vocalizations.
    /// Features vocal envelope-driven stochastic triggering, sample-rate independent 
    /// exponential FM down-sweeps, and an acoustic avian bio-resonator biquad filter.
    /// </summary>
    public class ChirpEffect : IAudioEffect
    {
        public string Name => "Chirp";

        private readonly float _amount;
        private readonly float _sampleRate;

        // Custom biquad filter to reshape pure synthetic tones into bird skull resonances
        private BiquadFilter _avianBioResonator;

        // Stateful tracking parameters across buffer blocks
        private float _chirpEnvelope = 0f;
        private float _chirpPhase = 0f;
        private float _voiceEnvelope = 0f;
        private float _currentSweepFreq = 0f;

        // Thread-isolated local high-speed LCG seed
        private uint _lcgState;

        private readonly float _voiceEnvAttackCoef;
        private readonly float _voiceEnvReleaseCoef;
        private readonly float _chirpDecayCoef;

        /// <summary>
        /// Initializes the Chirp effect.
        /// </summary>
        /// <param name="amount">Density and penetration depth of creature chirps (0.0f to 1.0f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public ChirpEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Configure avian skull acoustic resonance band (centered at 2600Hz)
            _avianBioResonator.ConfigureBandPass(2600f, _sampleRate, q: 2.8f);

            // Sample-rate decoupled coefficients
            _voiceEnvAttackCoef = (float)Math.Exp(-1000.0 / (4f * _sampleRate));   // 4ms attack
            _voiceEnvReleaseCoef = (float)Math.Exp(-1000.0 / (60f * _sampleRate)); // 60ms release

            // Structural chirp amplitude decay speed (approx. 35ms total bird click duration)
            _chirpDecayCoef = (float)Math.Exp(-1000.0 / (35f * _sampleRate));
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            // Scaled dynamic trigger parameter
            float baseTriggerChance = (0.012f * _amount) / _sampleRate;
            uint triggerThreshold = (uint)(baseTriggerChance * uint.MaxValue);
            float pi2 = 2f * (float)Math.PI;

            for (int i = 0; i < length; i++)
            {
                float dryInput = pcm[i];

                // 1. Voice envelope follower (Chirps are biologically driven by player vocalization effort)
                float absInput = Math.Abs(dryInput);
                if (absInput > _voiceEnvelope)
                    _voiceEnvelope = _voiceEnvAttackCoef * _voiceEnvelope + (1f - _voiceEnvAttackCoef) * absInput;
                else
                    _voiceEnvelope = _voiceEnvReleaseCoef * _voiceEnvelope + (1f - _voiceEnvReleaseCoef) * absInput;

                // 2. Advance high-speed local LCG state (1 CPU cycle cost)
                _lcgState = _lcgState * 1103515245 + 12345;

                // 3. Evaluate stochastic syrinx trigger mechanism
                if (_chirpEnvelope <= 0.001f)
                {
                    // Scale trigger probability based on real-time speech energy levels
                    uint dynamicThreshold = (uint)(triggerThreshold * (0.05f + _voiceEnvelope * 1.95f));

                    if (_lcgState < dynamicThreshold)
                    {
                        _chirpEnvelope = 1f; // Max initial strike power
                        _chirpPhase = 0f;    // Reset oscillator f0 boundary

                        // Randomize initial frequency sweep boundaries (High starting frequency between 3.4kHz and 4.2kHz)
                        float randVal = (float)(_lcgState & 0xFFFF) / 65535f;
                        _currentSweepFreq = 3400f + (randVal * 800f);
                    }
                }

                float synthesizedChirpNode = 0f;

                // 4. Execute active exponential syrinx FM down-sweep loop
                if (_chirpEnvelope > 0.001f)
                {
                    // Advance internal phase register using current sample-rate independent frequency
                    _chirpPhase += (pi2 * _currentSweepFreq) / _sampleRate;
                    if (_chirpPhase > pi2) _chirpPhase -= pi2;

                    // Fast polynomial phase wrapping for clean sine extraction
                    float sinVal = (float)Math.Sin(_chirpPhase);

                    // Combine primary wave with high quadratic envelope shaping
                    synthesizedChirpNode = sinVal * (_chirpEnvelope * _chirpEnvelope) * _amount * 0.45f;

                    // Execute the exponential FM down-sweep acceleration towards baseline register (1300Hz)
                    _currentSweepFreq = 1300f + (_currentSweepFreq - 1300f) * _chirpDecayCoef;

                    // Decay the active syrinx volume envelope node
                    _chirpEnvelope *= _chirpDecayCoef;
                }

                // 5. Run the raw synthetic node through the bio-resonance filter array
                float acousticChirp = _avianBioResonator.Process(synthesizedChirpNode);

                // 6. Fast polynomial soft-clipping for biological skin/beak crunch
                float drivenChirp = acousticChirp * 1.8f;
                float saturatedChirp = drivenChirp / (1f + Math.Abs(drivenChirp));

                // 7. Inject the pristine modeled syrinx node directly into the primary VoIP stream
                pcm[i] = dryInput + saturatedChirp;
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