namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    ///  Avian Syrinx Modeling Engine for creature chirps and flamingo vocalizations.
    /// Driven by pure harmonic sine wave down-sweeps exciting an organic avian resonator.
    /// </summary>
    public class ChirpEffect : IAudioEffect
    {
        public string Name => "Avian Chirp";

        private readonly float _amount;
        private readonly float _sampleRate;

        private BiquadFilter _avianBioResonator;

        private float _chirpEnvelope = 0f;
        private float _chirpPhase = 0f;
        private float _voiceEnvelope = 0f;
        private float _currentSweepFreq = 0f;
        private uint _lcgState;

        private readonly float _voiceEnvAttackCoef;
        private readonly float _voiceEnvReleaseCoef;
        private readonly float _chirpDecayCoef;

        public ChirpEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Resonator fine-tuned to simulate an organic bird skull bone expansion cavity (2600Hz)
            _avianBioResonator.ConfigureBandPass(2600f, _sampleRate, q: 2.8f);

            _voiceEnvAttackCoef = (float)Math.Exp(-1000.0 / (4f * _sampleRate));   // 4ms smooth attack
            _voiceEnvReleaseCoef = (float)Math.Exp(-1000.0 / (60f * _sampleRate));
            _chirpDecayCoef = (float)Math.Exp(-1000.0 / (35f * _sampleRate));     // 35ms natural decay duration
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            float baseTriggerChance = (0.012f * _amount) / _sampleRate;
            uint triggerThreshold = (uint)(baseTriggerChance * uint.MaxValue);
            float pi2 = 2f * (float)Math.PI;

            for (int i = 0; i < length; i++)
            {
                float dryInput = pcm[i];

                float absInput = Math.Abs(dryInput);
                if (absInput > _voiceEnvelope)
                    _voiceEnvelope = _voiceEnvAttackCoef * _voiceEnvelope + (1f - _voiceEnvAttackCoef) * absInput;
                else
                    _voiceEnvelope = _voiceEnvReleaseCoef * _voiceEnvelope + (1f - _voiceEnvReleaseCoef) * absInput;

                _lcgState = _lcgState * 1103515245 + 12345;

                if (_chirpEnvelope <= 0.001f)
                {
                    uint dynamicThreshold = (uint)(triggerThreshold * (0.05f + _voiceEnvelope * 1.95f));

                    if (_lcgState < dynamicThreshold)
                    {
                        _chirpEnvelope = 1f;
                        _chirpPhase = 0f;

                        float randVal = (float)(_lcgState & 0xFFFF) / 65535f;
                        _currentSweepFreq = 3400f + (randVal * 800f); // High organic avian pitch entry
                    }
                }

                float synthesizedChirpNode = 0f;

                if (_chirpEnvelope > 0.001f)
                {
                    _chirpPhase += (pi2 * _currentSweepFreq) / _sampleRate;
                    if (_chirpPhase > pi2) _chirpPhase -= pi2;

                    // Pristine, smooth sine wave extraction for natural animalistic tone generations
                    float sinVal = (float)Math.Sin(_chirpPhase);

                    synthesizedChirpNode = sinVal * (_chirpEnvelope * _chirpEnvelope) * _amount * 0.45f;
                    _currentSweepFreq = 1300f + (_currentSweepFreq - 1300f) * _chirpDecayCoef;
                    _chirpEnvelope *= _chirpDecayCoef;
                }

                float acousticChirp = _avianBioResonator.Process(synthesizedChirpNode);

                float drivenChirp = acousticChirp * 1.8f;
                float saturatedChirp = drivenChirp / (1f + Math.Abs(drivenChirp));

                pcm[i] = dryInput + saturatedChirp;
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
                _b0 = alpha / a0; _b1 = 0f; _b2 = -alpha / a0;
                _a1 = (-2f * cosW0) / a0; _a2 = (1f - alpha) / a0;
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