namespace SCP_Immersive_Voice.AudioProcessing
{
    using LabApi.Features.Console;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    using System.Collections.Generic;

    public class AudioEffectPipeline
    {
        // INTENT: Utilizing a volatile array reference snapshot ensures completely lock-free iterations
        // within the high-frequency voice loop, permanently resolving real-time audio thread stalling.
        private volatile IAudioEffect[] _effects = new IAudioEffect[0];
        private readonly object _pipelineLock = new object();

        /// <summary>
        /// Performance Switch: Set to true ONLY during debugging. 
        /// When false, the profiling loop overhead is completely bypassed (0 CPU cost).
        /// </summary>
        public static bool IsProfilingEnabled { get; set; } = false;

        public IAudioEffect[] Effects => _effects;

        public void Add(IAudioEffect effect)
        {
            if (effect == null) return;
            lock (_pipelineLock)
            {
                int currentLength = _effects.Length;
                IAudioEffect[] newEffects = new IAudioEffect[currentLength + 1];
                Array.Copy(_effects, newEffects, currentLength);
                newEffects[currentLength] = effect;
                _effects = newEffects;
            }
        }

        /// <summary>
        /// Atomically replaces the active effect stack under a full pipeline lock 
        /// to prevent concurrent processing threads from executing a partially rebuilt graph.
        /// </summary>
        public void UpdateEffects(IEnumerable<IAudioEffect> newEffects)
        {
            if (newEffects == null) return;
            lock (_pipelineLock)
            {
                List<IAudioEffect> tempList = new List<IAudioEffect>();
                foreach (var item in newEffects)
                {
                    if (item != null)
                    {
                        tempList.Add(item);
                    }
                }
                _effects = tempList.ToArray();
            }
        }

        public void Clear()
        {
            lock (_pipelineLock)
            {
                _effects = new IAudioEffect[0];
            }
        }

        /// <summary>
        /// Main audio processing loop. Executes completely lock-free via local array reference snapshotting.
        /// </summary>
        public void Process(float[] pcm, int samples)
        {
            if (pcm == null || samples < 1) return;

            // INTENT: A local reference snapshot insulates the hot-path execution loop from external graph adjustments,
            // bypassing heavy locks while ensuring thread safety across overlapping VoIP streaming packet contexts.
            IAudioEffect[] localEffects = _effects;
            int count = localEffects.Length;

            for (int e = 0; e < count; e++)
            {
                IAudioEffect effect = localEffects[e];

                if (IsProfilingEnabled)
                {
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
            if (pcm == null || samples < 1) return default(DspStats);

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
                    Logger.Warn($"[DSP Profiler] '{name}' triggered severe signal degradation! RMS Drop: {before.Rms:F4} -> {after.Rms:F4}");
                    return;
                }

                if (after.Peak > 1.0f)
                {
                    Logger.Error($"[DSP Profiler] '{name}' caused digital CLIPPING! Peak reached: {after.Peak:F2}");
                }
            }
        }
    }
}