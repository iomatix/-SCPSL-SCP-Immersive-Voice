namespace SCP_Immersive_Voice.AudioProcessing
{
    using LabApi.Features.Console;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    using System.Collections.Generic;

    public class AudioEffectPipeline
    {
        // Internal effects storage backed by a dedicated synchronization lock
        private readonly List<IAudioEffect> _effects = new List<IAudioEffect>();
        private readonly object _pipelineLock = new object();

        /// <summary>
        /// AAA Performance Switch: Set to true ONLY during debugging. 
        /// When false, the profiling loop overhead is completely bypassed (0 CPU cost).
        /// </summary>
        public static bool IsProfilingEnabled { get; set; } = false;

        public List<IAudioEffect> Effects => _effects;

        public void Add(IAudioEffect effect)
        {
            if (effect == null) return;
            lock (_pipelineLock)
            {
                _effects.Add(effect);
            }
        }

        public void Clear()
        {
            lock (_pipelineLock)
            {
                _effects.Clear();
            }
        }

        /// <summary>
        /// Main audio processing loop.
        /// </summary>
        public void Process(float[] pcm, int samples)
        {
            if (pcm == null || samples < 1) return;

            // Thread-safe isolation of the processing loop
            lock (_pipelineLock)
            {
                int count = _effects.Count;
                for (int e = 0; e < count; e++)
                {
                    var effect = _effects[e];

                    if (IsProfilingEnabled)
                    {
                        // Conditional profiling executed only on explicit demand
                        DspStats before = Analyze(pcm, samples);
                        try
                        {
                            effect.Process(pcm, samples);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[DSP Pipeline] Exception in effect '{effect.Name}': {ex.Message}");
                        }
                        DspStats after = Analyze(pcm, samples);

                        DspProfiler.Log(effect.Name, before, after);
                    }
                    else
                    {
                        // Ultra-fast direct execution path for standard production matches
                        try
                        {
                            effect.Process(pcm, samples);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[DSP Pipeline] Exception in effect '{effect.Name}': {ex.Message}");
                        }
                    }
                }
            }
        }

        public struct DspStats
        {
            public float Rms;
            public float Peak;
            public float NoiseFloor;
            public float Snr;
        }

        /// <summary>
        /// Mathematically accurate real-time PCM analyzer bounded strictly by valid sample count.
        /// </summary>
        public static DspStats Analyze(float[] pcm, int samples)
        {
            if (pcm == null || samples < 1) return default;

            // Protect against bounds overflow
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

            // True Noise Floor approximation utilizing lowest active discrete state energy
            float estimatedNoiseFloor = absoluteMinNonZero * 0.5f;
            if (estimatedNoiseFloor > rms) estimatedNoiseFloor = rms * 0.1f;

            // Mathematically valid Signal-to-Noise ratio formulation: 20 * log10(RMS / Noise)
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
                // Warn about structural DC audio dropouts or full pipeline mutes
                if (before.Rms > 0.01f && after.Rms < before.Rms * 0.05f)
                {
                    Logger.Warn($"[DSP Profiler] '{name}' triggered severe signal degradation! RMS Drop: {before.Rms:F4} -> {after.Rms:F4}");
                    return;
                }

                // Check for dynamic clipping or processing overloads
                if (after.Peak > 1.0f)
                {
                    Logger.Error($"[DSP Profiler] '{name}' caused digital CLIPPING! Peak reached: {after.Peak:F2}");
                }
            }
        }
    }
}