using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Avian Syrinx Modeling Engine for creature chirps and anomalous vocalizations.
    /// Driven by pure harmonic sine wave down-sweeps exciting an organic avian resonator.
    /// </summary>
    public class ChirpEffect : IAudioEffect
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

        private BiquadFilter _avianBioResonator;

        // Stateful tracking parameters for internal register sync
        private float _chirpEnvelope;
        private float _chirpPhase;
        private float _voiceEnvelope;
        private float _currentSweepFreq;
        private uint _lcgState;
        #endregion

        #region Public Metadata Properties
        public string Name => "Avian Chirp";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Avian Syrinx Chirp engine.
        /// </summary>
        /// <param name="amount">The intensity profile mix coefficient (0.0f to 1.0f).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public ChirpEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the argument payload
            _amount = amount.Clamp(0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;
            _lcgState = (uint)Guid.NewGuid().GetHashCode();

            // Resonator fine-tuned to simulate an organic bird skull bone expansion cavity (2600Hz)
            _avianBioResonator.ConfigureBandPass(2600f, _sampleRate, 2.8f);

            _voiceEnvAttackCoef = Mathf.Exp(-1000f / (4f * _sampleRate));   // 4ms smooth attack
            _voiceEnvReleaseCoef = Mathf.Exp(-1000f / (60f * _sampleRate));  // 60ms release
            _chirpDecayCoef = Mathf.Exp(-1000f / (35f * _sampleRate));       // 35ms natural decay duration

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

            float baseTriggerChance = (0.012f * _amount) / _sampleRate;
            uint triggerThreshold = (uint)(baseTriggerChance * uint.MaxValue);


            // Caching volatile instance properties onto stack registers to eliminate L1/L2 cache pointer chasing.
            // This bypasses heap synchronization overhead entirely for the duration of the VoIP buffer frame.
            float localVoiceEnvelope = _voiceEnvelope;
            float localChirpEnvelope = _chirpEnvelope;
            float localChirpPhase = _chirpPhase;
            float localSweepFreq = _currentSweepFreq;
            uint localLcgState = _lcgState;

            float attCoef = _voiceEnvAttackCoef;
            float relCoef = _voiceEnvReleaseCoef;
            float decayCoef = _chirpDecayCoef;
            float rate = _sampleRate;
            BiquadFilter localResonator = _avianBioResonator;

            for (int i = 0; i < length; i++)
            {
                float dryInput = pcm[i];

                // 1. Voice envelope follower (tracks talking activity to scale the chirp trigger chance)
                float absInput = dryInput.Abs();
                if (absInput > localVoiceEnvelope)
                {
                    localVoiceEnvelope = attCoef * localVoiceEnvelope + (1f - attCoef) * absInput;
                }
                else
                {
                    localVoiceEnvelope = relCoef * localVoiceEnvelope + (1f - relCoef) * absInput;
                }

                // 2. Advance the local LCG pseudo-random randomizer cycle
                localLcgState = localLcgState * 1103515245 + 12345;

                // 3. Evaluate random syrinx trigger chance if the previous chirp has decayed
                if (localChirpEnvelope <= 0.001f)
                {
                    uint dynamicThreshold = (uint)(triggerThreshold * (0.05f + localVoiceEnvelope * 1.95f));

                    if (localLcgState < dynamicThreshold)
                    {
                        localChirpEnvelope = 1f;
                        localChirpPhase = 0f;

                        float randVal = (float)(localLcgState & 0xFFFF) / 65535f;
                        localSweepFreq = 3400f + (randVal * 800f); // High organic avian pitch entry point
                    }
                }

                float synthesizedChirpNode = 0f;

                // 4. If syrinx is active, compute exponential sine sweep modulation
                if (localChirpEnvelope > 0.001f)
                {
                    localChirpPhase += (TwoPi * localSweepFreq) / rate;
                    if (localChirpPhase > TwoPi)
                        localChirpPhase -= TwoPi;

                    // Pristine, smooth float-native sine wave extraction for natural animalistic tone generation
                    float sinVal = Mathf.Sin(localChirpPhase);

                    synthesizedChirpNode = sinVal * (localChirpEnvelope * localChirpEnvelope) * _amount * 0.45f;

                    // Down-sweep execution trajectory
                    localSweepFreq = 1300f + (localSweepFreq - 1300f) * decayCoef;
                    localChirpEnvelope *= decayCoef;
                }

                // 5. Excite the skull bio-resonator biquad node and apply smooth rational clipping
                float acousticChirp = localResonator.Process(synthesizedChirpNode);
                float drivenChirp = acousticChirp * 1.8f;
                float saturatedChirp = drivenChirp / (1f + drivenChirp.Abs());

                pcm[i] = dryInput + saturatedChirp;
            }

            // Flush localized register contexts back atomically into object memory state blocks post execution sweep.
            _voiceEnvelope = localVoiceEnvelope;
            _chirpEnvelope = localChirpEnvelope;
            _chirpPhase = localChirpPhase;
            _currentSweepFreq = localSweepFreq;
            _lcgState = localLcgState;
            _avianBioResonator = localResonator;
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