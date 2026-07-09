using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using System;
using System.Collections.Generic;
using Logger = LabApi.Extensions.Misc.iLogger;

namespace SCP_Immersive_Voice.AudioProcessing
{
    /// <summary>
    /// Manages an atomic, thread-safe, and lock-free execution graph for float-native digital signal processing (DSP) effects.
    /// </summary>
    public class AudioEffectPipeline
    {
        #region Private Repositories & Execution Tokens
        // INTENT: Utilizing a volatile array reference snapshot ensures completely lock-free iterations
        // within the high-frequency voice loop, permanently resolving real-time audio thread stalling.
        private volatile IAudioEffect[] _effects = Array.Empty<IAudioEffect>();
        private readonly object _pipelineLock = new();
        #endregion

        #region Operational Config Properties
        /// <summary>
        /// Performance Switch: Set to true ONLY during debugging. 
        /// When false, the profiling loop overhead is completely bypassed (0 CPU cost).
        /// </summary>
        public static bool IsProfilingEnabled { get; set; }

        /// <summary>
        /// Gets the current volatile snapshot array of the registered audio effects.
        /// </summary>
        public IAudioEffect[] Effects => _effects;
        #endregion

        #region Graph Mutation Controllers
        public void Add(IAudioEffect effect)
        {
            if (effect is null) return;

            lock (_pipelineLock)
            {
                int currentLength = _effects.Length;
                var newEffects = new IAudioEffect[currentLength + 1];

                Array.Copy(_effects, newEffects, currentLength);
                newEffects[currentLength] = effect;
                _effects = newEffects;
            }
        }

        /// <summary>
        /// Atomically replaces the active effect stack under a full pipeline lock 
        /// to prevent concurrent processing threads from executing a partially rebuilt graph.
        /// </summary>
        public void UpdateEffects(List<IAudioEffect> newEffects)
        {
            if (newEffects is null) return;

            lock (_pipelineLock)
            {
                _effects = newEffects.ToArray();
            }
        }

        public void Clear()
        {
            lock (_pipelineLock)
            {
                _effects = Array.Empty<IAudioEffect>();
            }
        }
        #endregion

        #region High-Frequency Hot-Path Processing Engine
        /// <summary>
        /// Main audio processing loop. Executes completely lock-free via local array reference snapshotting.
        /// </summary>
        public void Process(float[] pcm, int samples)
        {
            if (pcm is null || samples < 1) return;

            // INTENT: A local reference snapshot insulates the hot-path execution loop from external graph adjustments,
            // bypassing heavy locks while ensuring thread safety across overlapping VoIP streaming packet contexts.
            IAudioEffect[] localEffects = _effects;
            int count = localEffects.Length;

            for (int e = 0; e < count; e++)
            {
                IAudioEffect effect = localEffects[e];
                if (effect is null) continue;

                // Deduplicated the heavy dual-path try-catch profiling tree into a clean, linear, branch-weighted pipeline.
                // Eradicated IL instruction bloating while guaranteeing zero runtime execution overhead.
                DspStats before = default;
                if (IsProfilingEnabled)
                {
                    before = Analyze(pcm, samples);
                }

                try
                {
                    effect.Process(pcm, samples);
                }
                catch (Exception ex)
                {
                    Logger.Error(nameof(AudioEffectPipeline), $"[DSP Pipeline] Exception in audio effect execution module '{effect.Name}': {ex.Message}");
                    continue;
                }

                if (IsProfilingEnabled)
                {
                    DspStats after = Analyze(pcm, samples);
                    DspProfiler.Log(effect.Name, before, after);
                }
            }
        }
        #endregion

        #region Telemetry Analyzer & Struct Subnodes
        public readonly struct DspStats
        {
            public float Rms { get; init; }
            public float Peak { get; init; }
            public float NoiseFloor { get; init; }
            public float Snr { get; init; }
        }

        /// <summary>
        /// Mathematically accurate real-time PCM analyzer bounded strictly by valid sample count.
        /// </summary>
        public static DspStats Analyze(float[] pcm, int samples)
        {
            if (pcm is null || samples < 1) return default;

            int activeSamples = Math.Min(samples, pcm.Length);

            float squaredSum = 0f;
            float absolutePeak = 0f;
            float absoluteMinNonZero = 1f;

            for (int i = 0; i < activeSamples; i++)
            {
                float v = pcm[i];
                float absV = Math.Abs(v);

                squaredSum += v * v;

                if (absV > absolutePeak)
                    absolutePeak = absV;

                if (absV > 0.0001f && absV < absoluteMinNonZero)
                    absoluteMinNonZero = absV;
            }

            float rms = (float)Math.Sqrt(squaredSum / activeSamples);

            float estimatedNoiseFloor = absoluteMinNonZero * 0.5f;
            if (estimatedNoiseFloor > rms) estimatedNoiseFloor = rms * 0.1f;

            float snr = 0f;
            if (rms > 1e-5f && estimatedNoiseFloor > 1e-6f)
            {
                snr = 20f * (float)Math.Log10(rms / estimatedNoiseFloor);
            }

            return new DspStats
            {
                Rms = rms,
                Peak = absolutePeak,
                NoiseFloor = estimatedNoiseFloor,
                Snr = snr
            };
        }

        /// <summary>
        /// Multi-tier logging diagnostics for analytical performance tracking.
        /// </summary>
        public static class DspProfiler
        {
            public static void Log(string name, DspStats before, DspStats after)
            {
                if (before.Rms > 0.01f && after.Rms < before.Rms * 0.05f)
                {
                    Logger.Warn(nameof(AudioEffectPipeline), $"[DSP Profiler] '{name}' triggered severe signal degradation! RMS Severe Drop: {before.Rms:F4} -> {after.Rms:F4}");
                    return;
                }

                if (after.Peak > 1.0f)
                {
                    Logger.Error(nameof(AudioEffectPipeline), $"[DSP Profiler] '{name}' caused digital CLIPPING! Critical Peak reached: {after.Peak:F2}");
                }
            }
        }
        #endregion
    }
}