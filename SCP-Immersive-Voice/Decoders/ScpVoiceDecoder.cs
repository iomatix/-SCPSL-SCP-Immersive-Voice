using LabApi.Extensions;
using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using SCP_Immersive_Voice.VoiceProfiles;
using System;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;
using Logger = LabApi.Extensions.Misc.iLogger;

namespace SCP_Immersive_Voice.Decoders
{
    /// <summary>
    /// Thread-safe lock-free object pool for exact-sized audio buffers preventing trailing garbage in native API streams.
    /// </summary>
    internal static class ExactAudioBufferPool
    {
        private static readonly System.Collections.Concurrent.ConcurrentBag<float[]> Pool = new();

        public static float[] Rent(int size)
        {
            if (Pool.TryTake(out var arr) && arr.Length == size)
                return arr;
            return new float[size];
        }

        public static void Return(float[] arr)
        {
            if (arr is null) return;
            Pool.Add(arr);
        }
    }

    /// <summary>
    /// Handles Opus decoding/encoding and applies the SCP voice DSP pipeline on float PCM audio. 
    /// Fully float-native, zero-copy, allocation-free via external memory pool buffer bindings.
    /// </summary>
    public static class ScpVoiceDecoder
    {
        #region Private Static Audio Hardware Resources
        private static readonly OpusDecoder Decoder = new();
        private static readonly OpusEncoder Encoder = new(OpusApplicationType.Voip);
        #endregion

        #region Core Decoding & Encoding Pipelines
        public static int Decode(VoiceMessage msg, float[] targetBuffer)
        {
            if (msg.Data is null || msg.DataLength <= 0 || targetBuffer is null)
                return 0;

            int samples = Decoder.Decode(msg.Data, msg.DataLength, targetBuffer);
            return samples > 0 ? samples : 0;
        }

        public static int EncodeToOpus(float[] pcm, int length, byte[] targetBuffer)
        {
            if (pcm is null || length <= 0 || targetBuffer is null)
                return 0;

            int encodedLen = Encoder.Encode(pcm, targetBuffer, length);
            return encodedLen > 0 ? encodedLen : 0;
        }
        #endregion

        #region DSP Graph Application Core
        public static void ApplyEffects(float[] pcm, int length, Player scp)
        {
            if (pcm is null || length <= 0 || scp is null)
                return;

            var activePreset = ScpVoiceProfiles.GetPreset(scp);
            if (activePreset is null || !activePreset.Enable)
                return;

            ApplyAgc(pcm, length, targetPeak: 0.7f, maxGain: 3f);

            var pipeline = ScpVoiceProfiles.GetPipelineFor(scp, activePreset);
            if (pipeline is not null)
            {
                IAudioEffect[] localEffects = pipeline.Effects;
                int effectCount = localEffects.Length;

                for (int i = 0; i < effectCount; i++)
                {
                    var effect = localEffects[i];
                    if (effect is null) continue;

                    try
                    {
                        effect.Process(pcm, length);

                        for (int sampleIdx = 0; sampleIdx < length; sampleIdx++)
                        {
                            float sample = pcm[sampleIdx];
                            if (float.IsNaN(sample) || float.IsInfinity(sample))
                            {
                                Logger.Error(nameof(ScpVoiceDecoder), $"[DSP-CRASH] Effect '{effect.GetType().Name}' generated an invalid float value ({sample}) at sample index {sampleIdx}! Defensively silencing the pipeline.");
                                Array.Clear(pcm, 0, length);
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(nameof(ScpVoiceDecoder), $"[DSP-EXCEPTION] Exception thrown by audio effect block '{effect.GetType().Name}': {ex.Message}");
                    }
                }
            }

            ApplyOutputGain(pcm, length, activePreset.OutputGain);
            ApplyLimiter(pcm, length, threshold: 0.98f);
        }
        #endregion

        #region Advanced Mathematical DSP Sub-Filters
        public static bool IsSilent(float[] pcm, int length, float threshold = 0.01f)
        {
            if (pcm is null || length <= 0)
                return true;

            float absThr = Math.Abs(threshold);

            for (int i = 0; i < length; i++)
            {
                if (Math.Abs(pcm[i]) > absThr)
                    return false;
            }

            return true;
        }

        private static void ApplyLimiter(float[] pcm, int length, float threshold = 0.98f)
        {
            float t = Math.Abs(threshold);

            for (int i = 0; i < length; i++)
            {
                float v = pcm[i];
                float absV = Math.Abs(v);

                if (absV > 0.8f)
                {
                    float excess = absV - 0.8f;
                    absV = 0.8f + excess / (1f + excess * excess);

                    if (absV > t) absV = t;
                    v = Math.Sign(v) * absV;
                }

                pcm[i] = v;
            }
        }

        private static void ApplyAgc(float[] pcm, int length, float targetPeak, float maxGain)
        {
            float peak = 0f;

            for (int i = 0; i < length; i++)
            {
                float a = Math.Abs(pcm[i]);
                if (a > peak) peak = a;
            }

            if (peak < 0.0001f)
                return;

            float gain = targetPeak / peak;
            if (gain > maxGain)
                gain = maxGain;

            for (int i = 0; i < length; i++)
                pcm[i] *= gain;
        }

        private static void ApplyOutputGain(float[] pcm, int length, float gain)
        {
            gain = gain.Clamp(0.0f, 3.0f);
            if (Math.Abs(gain - 1.0f) < 0.001f) return;

            for (int i = 0; i < length; i++)
            {
                pcm[i] *= gain;
            }
        }
        #endregion
    }
}