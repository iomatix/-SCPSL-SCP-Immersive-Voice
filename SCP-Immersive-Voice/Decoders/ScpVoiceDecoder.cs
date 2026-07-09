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
        /// <summary>
        /// Context class to persist DSP and filter states across discrete frame boundaries.
        /// </summary>
        private class SessionDspContext
        {
            public float CurrentGain = 1.0f;
            public float LastX = 0.0f;
            public float LastY = 0.0f;
        }

        /// <summary>
        /// Thread-safe weak association table dynamically mapping DSP contexts to active VoiceSessions.
        /// </summary>
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
                // This stops dynamic gain modulators from converting flat voltage offsets into an audible buzz.
                ApplyDcBlocker(pcm, length, dspContext);

                // 2. Pre-Processing Gate (Executed on CLEANED audio data)
                // Now accurately identifies genuine environmental silence since hardware hum has been stripped.
                if (IsSilent(pcm, length, 0.004f))
                {
                    Array.Clear(pcm, 0, length);
                    return;
                }

                // 3. Stateful Predictive Smooth AGC Processing
                ApplyAgcStateful(pcm, length, dspContext, targetPeak: 0.65f, maxGain: 2.5f);

                // 4. Core DSP Effect Pipeline Execution
                pipeline.Process(pcm, length);

                // 5. Consolidated Post-Processing Master Out & Soft Brickwall Limiter
                ExecutePostDspPipeline(pcm, length, preset.OutputGain, threshold: 0.98f);
            }
        }
        #endregion

        #region Advanced Mathematical DSP Sub-Filters
        private static void ApplyDcBlocker(float[] pcm, int length, SessionDspContext context)
        {
            const float R = 0.995f; // Pole coefficient tuning cutoff to ~25Hz at 48kHz sample rate
            float lastX = context.LastX;
            float lastY = context.LastY;

            for (int i = 0; i < length; i++)
            {
                float x = pcm[i];

                // y[n] = x[n] - x[n-1] + R * y[n-1]
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

        private static void ApplyAgcStateful(float[] pcm, int length, SessionDspContext context, float targetPeak, float maxGain)
        {
            float peak = 0.0f;
            for (int i = 0; i < length; i++)
            {
                float absVal = Math.Abs(pcm[i]);
                if (absVal > peak) peak = absVal;
            }

            float targetGain = context.CurrentGain;
            if (peak > 0.001f)
            {
                targetGain = targetPeak / peak;
                if (targetGain > maxGain) targetGain = maxGain;
                if (targetGain < 0.15f) targetGain = 0.15f;
            }
            else
            {
                targetGain = 1.0f;
            }

            // Asymmetric smoothing window coefficients (Fast attack, relaxed release decay)
            float smoothingFactor = (targetGain < context.CurrentGain) ? 0.20f : 0.03f;
            float startGain = context.CurrentGain;
            float endGain = startGain + (targetGain - startGain) * smoothingFactor;

            // Sample-accurate linear interpolation over the discrete frame buffer block
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / length;
                float currentSampleGain = startGain + (endGain - startGain) * t;
                pcm[i] *= currentSampleGain;
            }

            context.CurrentGain = endGain;
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
                    v = 0.0f;
                }
                else if (!skipGain)
                {
                    v *= gain;
                }

                // Smooth analog emulation transfer curve clip
                float absV = Math.Abs(v);
                if (absV > 0.8f)
                {
                    float excess = absV - 0.8f;
                    absV = 0.8f + excess / (1.0f + excess * excess);

                    if (absV > t) absV = t;
                    v = Math.Sign(v) * absV;
                }

                pcm[i] = v;
            }
        }
        #endregion
    }
}