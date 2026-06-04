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
                // Log stats before/after processing each effect
                var before = Analyze(pcm);
                effect.Process(pcm, samples);
                var after = Analyze(pcm);

                DspProfiler.Log(effect.Name, before, after);
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
        /// Calculates RMS, peak, noise floor and SNR
        /// </summary>
        /// <param name="pcm">PCM data</param>
        /// <returns>DSP stats</returns>
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
            float noise = rms * 0.1f; // not exactly
            float snr = 20f * (float)Math.Log10((rms + 1e-6f) / (noise + 1e-6f));

            return new DspStats
            {
                Rms = rms,
                Peak = peak,
                NoiseFloor = noise,
                Snr = snr
            };
        }

        
        /// <summary>
        /// Class for logging DSP stats in console
        /// </summary>
        public static class DspProfiler
        {
            public static void Log(string name, DspStats before, DspStats after)
            {
                if (after.Snr < 5)
                    Logger.Error($"[DSP] {name}: CRITICAL – SNR={after.Snr:F1} dB (noise)");
                else if (after.Rms < before.Rms * 0.2f)
                    Logger.Warn($"[DSP] {name}: WARNING – RMS drop {before.Rms:F3} → {after.Rms:F3}");
            }
        }
    }
}
