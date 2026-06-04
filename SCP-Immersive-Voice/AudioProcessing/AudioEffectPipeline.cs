namespace SCP_Immersive_Voice.AudioProcessing
{
    using LabApi.Features.Console;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    using System.Collections.Generic;

    public class AudioEffectPipeline
    {
        private readonly List<IAudioEffect> _effects = new List<IAudioEffect>();

        public void Add(IAudioEffect effect) => _effects.Add(effect);

        public void Process(float[] pcm, int samples)
        {
            foreach (var effect in _effects)
            {
                // 1. Statystyki przed efektem
                var before = Analyze(pcm);

                // 2. Przetwarzanie efektu
                effect.Process(pcm, samples);

                // 3. Statystyki po efekcie
                var after = Analyze(pcm);

                // 4. Log diagnostyczny
                DspProfiler.Log(effect.Name, before, after);
            }
        }

        // -----------------------------
        // DSP ANALYZER
        // -----------------------------
        public struct DspStats
        {
            public float Rms;
            public float Peak;
            public float NoiseFloor;
            public float Snr;
        }

        public static DspStats Analyze(float[] pcm)
        {
            float sum = 0f;
            float peak = 0f;

            for (int i = 0; i < pcm.Length; i++)
            {
                float v = pcm[i];
                sum += v * v;

                float a = Math.Abs(v);
                if (a > peak)
                    peak = a;
            }

            float rms = (float)Math.Sqrt(sum / pcm.Length);
            float noise = rms * 0.1f; // przybliżenie
            float snr = 20f * (float)Math.Log10((rms + 1e-6f) / (noise + 1e-6f));

            return new DspStats
            {
                Rms = rms,
                Peak = peak,
                NoiseFloor = noise,
                Snr = snr
            };
        }

        // -----------------------------
        // DSP PROFILER
        // -----------------------------
        public static class DspProfiler
        {
            public static void Log(string name, DspStats before, DspStats after)
            {
                if (after.Snr < 5)
                    Logger.Warn($"[DSP] {name}: CRITICAL – SNR={after.Snr:F1} dB (noise)");
                else if (after.Rms < before.Rms * 0.2f)
                    Logger.Warn($"[DSP] {name}: WARNING – RMS drop {before.Rms:F3} → {after.Rms:F3}");
                else
                    Logger.Debug($"[DSP] {name}: OK");
            }
        }
    }
}
