using LabApi.Extensions;
using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using SCP_Immersive_Voice.Managers;
using SCP_Immersive_Voice.VoiceProfiles;
using System;
using Logger = LabApi.Extensions.Misc.iLogger;

namespace SCP_Immersive_Voice.Decoders
{
    public static class ScpVoiceDecoder
    {
        #region Core Decoding & Encoding Pipelines (Session-Insulated)
        /// <summary>
        /// Decodes an incoming Opus compressed byte payload using the caller session's private isolated hardware decoder codec.
        /// </summary>
        public static int Decode(VoiceSession session, byte[] rawOpusData, int dataLength, float[] targetBuffer)
        {
            if (session is null || rawOpusData is null || dataLength <= 0 || targetBuffer is null)
                return 0;

            int samples = session.SessionDecoder.Decode(rawOpusData, dataLength, targetBuffer);
            return samples > 0 ? samples : 0;
        }

        /// <summary>
        /// Encodes a raw float frame using the caller session's private isolated hardware encoder codec.
        /// </summary>
        public static int EncodeToOpus(VoiceSession session, float[] pcm, int length, byte[] targetBuffer)
        {
            if (session is null || pcm is null || length <= 0 || targetBuffer is null)
                return 0;

            int encodedLen = session.SessionEncoder.Encode(pcm, targetBuffer, length);
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