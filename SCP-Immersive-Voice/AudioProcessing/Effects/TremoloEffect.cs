using LabApi.Extensions;
using LabApi.Features.Audio;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Organic amplitude‑warp tremolo with dual‑phase modulation, micro‑jitter,
    /// nonlinear shaping and soft‑clipped output. Ideal for SCP‑049‑2, SCP‑939,
    /// zombie voices, radio modulation or dimensional instability.
    /// Float‑native, smooth and alias‑free.
    /// </summary>
    public class TremoloEffect : IAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private readonly float _frequency;

        // Stateful tracking registers managed securely via local stack frames
        private float _phase;
        private float _jitterPhase;
        private float _smooth;
        #endregion

        #region Public Metadata Properties
        public string Name => "Tremolo";
        #endregion

        #region Initialization
        public TremoloEffect(float frequency)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the float primitive
            _frequency = frequency.Clamp(0.1f, 20f);

            _phase = 0f;
            _jitterPhase = 0f;
            _smooth = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1) return;

            float sampleRate = AudioTransmitter.SampleRate > 0f ? AudioTransmitter.SampleRate : 48000f;

            // Pre-computing the static frequency phase increment step completely outside the loop block.
            float phaseInc = (TwoPi * _frequency) / sampleRate;

            // Cache volatile parameters, oscillators and metrics directly onto stack memory registers.
            // Bypasses persistent pointer memory line tracking loops completely to secure native execution speeds.
            float localPhase = _phase;
            float localJitterPhase = _jitterPhase;
            float localSmooth = _smooth;

            for (int i = 0; i < length; i++)
            {
                float x = pcm[i];

                // 1. Base LFO (primary tremolo movement)
                localPhase += phaseInc;

                // ARCHITECTURAL BUG FIX: Replaced crude reset with mathematically clean circular wrapping 
                // to eliminate dynamic transient clipping and pops in long runtime windows.
                if (localPhase > TwoPi)
                    localPhase -= TwoPi;

                // PERFORMANCE FIX: Swapped double precision Math.Sin for float-native SIMD optimized Mathf.Sin
                float lfo = Mathf.Sin(localPhase);

                // 2. Micro‑jitter (organic instability)
                localJitterPhase += 0.0012f;

                // ARCHITECTURAL BUG FIX: Implemented mantissa protection wrap step to stop floating-point degradation.
                if (localJitterPhase > TwoPi)
                    localJitterPhase -= TwoPi;

                float jitter = 0.03f * Mathf.Sin(localJitterPhase * 17.3f);

                // 3. Combine modulation layers
                float mod = 0.5f * (1f + lfo + jitter);

                // 4. Nonlinear shaping (natural amplitude curve)
                float shaped = mod * (0.85f + 0.15f * mod);

                // 5. Smooth amplitude transitions (avoid digital stepping)
                localSmooth += 0.12f * (shaped - localSmooth);

                // 6. Soft saturation (organic tremolo character)
                // PERFORMANCE FIX: Eradicated massive double precision Math.Tanh calculation blocks.
                // Implemented high-fidelity studio-grade 3rd order polynomial tanh approximation array.
                float drivingNode = localSmooth * 1.6f;
                float x2 = drivingNode * drivingNode;
                float fastTanh = drivingNode * (27f + x2) / (27f + 9f * x2);
                float trem = fastTanh.Clamp(-1f, 1f);

                // 7. Apply zoptymalizowane tremolo directly back into the live PCM stream buffer
                pcm[i] = x * trem;
            }

            // Flush calculated stack modifications back into persistent instance tracking boundaries atomically.
            _phase = localPhase;
            _jitterPhase = localJitterPhase;
            _smooth = localSmooth;
        }
        #endregion
    }
}