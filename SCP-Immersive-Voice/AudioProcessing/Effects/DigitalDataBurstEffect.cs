namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA Cybernetic Data Burst and Diagnostic Transmission Engine for digital entities.
    /// Employs a high-frequency metallic silicon resonator driven by asymmetric binary square-waves.
    /// </summary>
    public class DigitalDataBurstEffect : IAudioEffect
    {
        public string Name => "Digital Data Burst";

        private readonly float _amount;
        private readonly float _sampleRate;

        private BiquadFilter _mainframeResonator;

        private float _chirpEnvelope = 0f;
        private float _chirpPhase = 0f;
        private float _voiceEnvelope = 0f;
        private float _currentSweepFreq = 0f;
        private uint _lcgState;

        private readonly float _voiceEnvAttackCoef;
        private readonly float _voiceEnvReleaseCoef;
        private readonly float _chirpDecayCoef;

        public DigitalDataBurstEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Rigid metallic PCB trace network band resonator (centered at 5800Hz)
            _mainframeResonator.ConfigureBandPass(5800f, _sampleRate, q: 7.0f);

            _voiceEnvAttackCoef = (float)Math.Exp(-1000.0 / (2f * _sampleRate)); // Fast 2ms response
            _voiceEnvReleaseCoef = (float)Math.Exp(-1000.0 / (45f * _sampleRate));
            _chirpDecayCoef = (float)Math.Exp(-1000.0 / (14f * _sampleRate)); // 14ms snappy burst
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            float baseTriggerChance = (0.018f * _amount) / _sampleRate;
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
                    uint dynamicThreshold = (uint)(triggerThreshold * (0.1f + _voiceEnvelope * 2.2f));

                    if (_lcgState < dynamicThreshold)
                    {
                        _chirpEnvelope = 1f;
                        _chirpPhase = 0f;

                        float randVal = (float)(_lcgState & 0xFFFF) / 65535f;
                        _currentSweepFreq = 5000f + (randVal * 1500f);
                    }
                }

                float synthesizedDataNode = 0f;

                if (_chirpEnvelope > 0.001f)
                {
                    _chirpPhase += (pi2 * _currentSweepFreq) / _sampleRate;
                    if (_chirpPhase > pi2) _chirpPhase -= pi2;

                    float pureSine = (float)Math.Sin(_chirpPhase);
                    float jaggedDigitalWave = pureSine > 0.0f ? 0.65f : -0.65f; // Asymmetric binary square

                    synthesizedDataNode = jaggedDigitalWave * (_chirpEnvelope * _chirpEnvelope) * _amount * 0.35f;
                    _currentSweepFreq = 2200f + (_currentSweepFreq - 2200f) * _chirpDecayCoef;
                    _chirpEnvelope *= _chirpDecayCoef;
                }

                float acousticChirp = _mainframeResonator.Process(synthesizedDataNode);

                float drivenChirp = acousticChirp * 2.5f;
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