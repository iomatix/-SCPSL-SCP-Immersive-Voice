using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Cybernetic Data Burst and Diagnostic Transmission Engine for digital entities.
    /// Employs a high-frequency metallic silicon resonator driven by asymmetric binary square-waves.
    /// </summary>
    public class DigitalDataBurstEffect : IAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private readonly float _amount;
        private readonly float _sampleRate;

        private readonly float _voiceEnvAttackCoef;
        private readonly float _voiceEnvReleaseCoef;
        private readonly float _chirpDecayCoef;

        private BiquadFilter _mainframeResonator;

        // Stateful tracking parameters managed inside stack register windows
        private float _chirpEnvelope;
        private float _chirpPhase;
        private float _voiceEnvelope;
        private float _currentSweepFreq;
        private uint _lcgState;
        #endregion

        #region Public Metadata Properties
        public string Name => "Digital Data Burst";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalDataBurstEffect"/> class.
        /// </summary>
        /// <param name="amount">The intensity profile mix coefficient (0.0f to 1.0f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public DigitalDataBurstEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the argument payload
            _amount = amount.Clamp(0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Rigid metallic PCB trace network band resonator (centered at 5800Hz)
            _mainframeResonator.ConfigureBandPass(5800f, _sampleRate, 7.0f);

            _voiceEnvAttackCoef = Mathf.Exp(-1000f / (2f * _sampleRate));  // Fast 2ms response
            _voiceEnvReleaseCoef = Mathf.Exp(-1000f / (45f * _sampleRate)); // 45ms release
            _chirpDecayCoef = Mathf.Exp(-1000f / (14f * _sampleRate));      // 14ms snappy burst

            _chirpEnvelope = 0f;
            _chirpPhase = 0f;
            _voiceEnvelope = 0f;
            _currentSweepFreq = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            float baseTriggerChance = (0.018f * _amount) / _sampleRate;
            uint triggerThreshold = (uint)(baseTriggerChance * uint.MaxValue);

            // Caching instance fields locally to insulate variables within the CPU register bank.
            // Eradicates L1/L2 cache pointer chasing overhead during high-frequency real-time packet streaming loops.
            float localVoiceEnvelope = _voiceEnvelope;
            float localChirpEnvelope = _chirpEnvelope;
            float localChirpPhase = _chirpPhase;
            float localSweepFreq = _currentSweepFreq;
            uint localLcgState = _lcgState;

            float attCoef = _voiceEnvAttackCoef;
            float relCoef = _voiceEnvReleaseCoef;
            float decayCoef = _chirpDecayCoef;
            float rate = _sampleRate;
            float amt = _amount;
            BiquadFilter localResonator = _mainframeResonator;

            for (int i = 0; i < length; i++)
            {
                float dryInput = pcm[i];

                // 1. Voice envelope follower (modulates diagnostic trigger density based on voice activity)
                float absInput = dryInput.Abs();
                if (absInput > localVoiceEnvelope)
                {
                    localVoiceEnvelope = attCoef * localVoiceEnvelope + (1f - attCoef) * absInput;
                }
                else
                {
                    localVoiceEnvelope = relCoef * localVoiceEnvelope + (1f - relCoef) * absInput;
                }

                // 2. Advance the local hardware-isolated LCG sequence
                localLcgState = localLcgState * 1103515245 + 12345;

                // 3. Evaluate random digital transmission burst injection constraints
                if (localChirpEnvelope <= 0.001f)
                {
                    uint dynamicThreshold = (uint)(triggerThreshold * (0.1f + localVoiceEnvelope * 2.2f));

                    if (localLcgState < dynamicThreshold)
                    {
                        localChirpEnvelope = 1f;
                        localChirpPhase = 0f;

                        float randVal = (float)(localLcgState & 0xFFFF) / 65535f;
                        localSweepFreq = 5000f + (randVal * 1500f); // High-frequency cybernetic pitch entry point
                    }
                }

                float synthesizedDataNode = 0f;

                // 4. If data burst is active, calculate asymmetric step quantization
                if (localChirpEnvelope > 0.001f)
                {
                    localChirpPhase += (TwoPi * localSweepFreq) / rate;
                    if (localChirpPhase > TwoPi)
                        localChirpPhase -= TwoPi;

                    // PERFORMANCE FIX: Replaced high-overhead double Math.Sin with float-native Mathf.Sin
                    float pureSine = Mathf.Sin(localChirpPhase);

                    // Asymmetric binary square-wave truncation modeling rigid transistor square clipping
                    float jaggedDigitalWave = pureSine > 0.0f ? 0.65f : -0.65f;

                    synthesizedDataNode = jaggedDigitalWave * (localChirpEnvelope * localChirpEnvelope) * amt * 0.35f;

                    // Rapid pitch sweep decay execution trajectory
                    localSweepFreq = 2200f + (localSweepFreq - 2200f) * decayCoef;
                    localChirpEnvelope *= decayCoef;
                }

                // 5. Excite the rigid trace PCB resonator and map through a rational saturator
                float acousticChirp = localResonator.Process(synthesizedDataNode);
                float drivenChirp = acousticChirp * 2.5f;
                float saturatedChirp = drivenChirp / (1f + drivenChirp.Abs());

                pcm[i] = dryInput + saturatedChirp;
            }


            // Flush localized stack register context structures back into object tracking instance layout fields.
            _voiceEnvelope = localVoiceEnvelope;
            _chirpEnvelope = localChirpEnvelope;
            _chirpPhase = localChirpPhase;
            _currentSweepFreq = localSweepFreq;
            _lcgState = localLcgState;
            _mainframeResonator = localResonator;
        }
        #endregion

        #region Internal High-Performance Data Substructures
        /// <summary>
        /// High-performance, stack-allocated 2nd order IIR filter structure.
        /// </summary>
        private struct BiquadFilter
        {
            private float _b0, _b1, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public void ConfigureBandPass(float centerFrequency, float sampleRate, float q)
            {
                float w0 = TwoPi * centerFrequency / sampleRate;
                float alpha = Mathf.Sin(w0) / (2f * q);
                float cosW0 = Mathf.Cos(w0);

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
        #endregion
    }
}