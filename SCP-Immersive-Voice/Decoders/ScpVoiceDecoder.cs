using LabApi.Extensions;
using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.Managers;
using SCP_Immersive_Voice.VoiceProfiles;
using System;
using System.Runtime.CompilerServices;

namespace SCP_Immersive_Voice.Decoders
{
    public static class ScpVoiceDecoder
    {
        #region Stateful DSP Session Context Container
        private class SessionDspContext
        {
            public float LastX = 0.0f;
            public float LastY = 0.0f;
        }

        private static readonly ConditionalWeakTable<VoiceSession, SessionDspContext> SessionContexts = new();
        #endregion

        #region Core Decoding & Encoding Pipelines
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
                var dspContext = SessionContexts.GetOrCreateValue(session);

                // 1. Hardware DC Blocker (Subsonic High-Pass Filter)
                // Removes unmanaged DC hardware offset and microphone baseline electricity.
                ApplyDcBlocker(pcm, length, dspContext);

                // [USUNIĘTO AGC] - Pozwalamy naturalnej dynamice głosu napędzać łańcuch efektów.
                // Wbudowany w potok NoiseGateEffect dba o wycinanie tła bez kompresowania sygnału.

                // 2. Core DSP Effect Pipeline Execution
                pipeline.Process(pcm, length);

                // 3. Consolidated Post-Processing Master Out (Pristine transparent limit)
                ExecutePostDspPipeline(pcm, length, preset.OutputGain);
            }
        }
        #endregion

        #region Advanced Mathematical DSP Sub-Filters
        private static void ApplyDcBlocker(float[] pcm, int length, SessionDspContext context)
        {
            const float R = 0.995f;
            float lastX = context.LastX;
            float lastY = context.LastY;

            for (int i = 0; i < length; i++)
            {
                float x = pcm[i];
                float y = x - lastX + R * lastY;

                if (float.IsNaN(y) || float.IsInfinity(y))
                    y = 0.0f;

                lastX = x;
                lastY = y;
                pcm[i] = y;
            }

            context.LastX = lastX;
            context.LastY = lastY;
        }

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

        private static void ExecutePostDspPipeline(float[] pcm, int length, float gain)
        {
            gain = gain.Clamp(0.0f, 4.0f);
            bool skipGain = Math.Abs(gain - 1.0f) < 0.001f;

            for (int i = 0; i < length; i++)
            {
                float v = pcm[i];

                if (float.IsNaN(v) || float.IsInfinity(v))
                {
                    v = 0.0f;
                }
                else if (!skipGain)
                {
                    v *= gain;
                }

                if (v > 1.0f) v = 1.0f;
                else if (v < -1.0f) v = -1.0f;

                pcm[i] = v;
            }
        }
        #endregion
    }
}