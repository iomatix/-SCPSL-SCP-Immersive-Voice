using LabApi.Extensions;
using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.Managers;
using SCP_Immersive_Voice.VoiceProfiles;
using System;

namespace SCP_Immersive_Voice.Decoders
{
    public static class ScpVoiceDecoder
    {
        #region Core Decoding & Encoding Pipelines (Session-Insulated)
        public static int Decode(VoiceSession session, byte[] rawOpusData, int dataLength, float[] targetBuffer)
        {
            if (session is null || rawOpusData is null || dataLength <= 0 || targetBuffer is null)
                return 0;

            int samples = session.SessionDecoder.Decode(rawOpusData, dataLength, targetBuffer);
            return samples > 0 ? samples : 0;
        }

        public static int EncodeToOpus(VoiceSession session, float[] pcm, int length, byte[] targetBuffer)
        {
            if (session is null || pcm is null || length <= 0 || targetBuffer is null)
                return 0;

            int encodedLen = session.SessionEncoder.Encode(pcm, targetBuffer, length);
            return encodedLen > 0 ? encodedLen : 0;
        }
        #endregion

        #region DSP Graph Application Core
        public static void ApplyEffects(float[] pcm, int length, Player scp, VoiceSession session)
        {
            if (pcm is null || length <= 0 || scp is null || session is null)
                return;

            var (pipeline, preset) = ScpVoiceProfiles.ResolvePipelineContext(scp, session);
            if (pipeline is null || preset is null)
                return;

            lock (session.SyncLock)
            {
                // 1. Pre-Processing
                ApplyAgc(pcm, length, targetPeak: 0.7f, maxGain: 3f);

                // 2. Core DSP Pipeline Execution
                pipeline.Process(pcm, length);

                // 3. Consolidated Single-Pass Post-Processing
                ExecutePostDspPipeline(pcm, length, preset.OutputGain, threshold: 0.98f);
            }
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

        private static void ExecutePostDspPipeline(float[] pcm, int length, float gain, float threshold)
        {
            float t = Math.Abs(threshold);
            gain = gain.Clamp(0.0f, 3.0f);
            bool skipGain = Math.Abs(gain - 1.0f) < 0.001f;

            for (int i = 0; i < length; i++)
            {
                float v = pcm[i];

                if (float.IsNaN(v) || float.IsInfinity(v))
                {
                    v = 0f;
                }
                else if (!skipGain)
                {
                    v *= gain;
                }

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
        #endregion
    }
}