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
        #region Stateful AGC Ephemeral Context
        /// <summary>
        /// Context class to persist AGC state and target gain across discrete packet boundaries.
        /// </summary>
        private class AgcState
        {
            public float CurrentGain = 1.0f;
        }

        /// <summary>
        /// Thread-safe weak association table to dynamically map AGC states to VoiceSessions without modifying their structure.
        /// </summary>
        private static readonly ConditionalWeakTable<VoiceSession, AgcState> SessionAgcStates = new();
        #endregion

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
                // 1. Pre-Processing Hardware Gate
                // Prevents ambient or cable static hum from bypassing the processing chain during microphone idle states.
                if (IsSilent(pcm, length, 0.0035f))
                {
                    Array.Clear(pcm, 0, length);
                    return;
                }

                // 2. Stateful Look-Ahead Smooth AGC Processing
                // Stabilizes signal amplitude with zero boundary phase splits.
                ApplyAgcStateful(session, pcm, length, targetPeak: 0.65f, maxGain: 2.5f);

                // 3. Core DSP Pipeline Execution
                pipeline.Process(pcm, length);

                // 4. Consolidated Single-Pass Post-Processing & Soft-Clipping
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

                // Soft-knee brickwall analog emulation clipping
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

        private static void ApplyAgcStateful(VoiceSession session, float[] pcm, int length, float targetPeak, float maxGain)
        {
            var state = SessionAgcStates.GetOrCreateValue(session);

            float peak = 0f;
            for (int i = 0; i < length; i++)
            {
                float absVal = Math.Abs(pcm[i]);
                if (absVal > peak) peak = absVal;
            }

            // Target gain resolution logic based on current envelope peak
            float targetGain = state.CurrentGain;
            if (peak > 0.001f)
            {
                targetGain = targetPeak / peak;
                if (targetGain > maxGain) targetGain = maxGain;
                if (targetGain < 0.15f) targetGain = 0.15f;
            }
            else
            {
                // Smoothly decay back to standard unity gain when input drops to absolute silence
                targetGain = 1.0f;
            }

            // Attack/Release asymmetric response smoothing coefficients
            float smoothingFactor = (targetGain < state.CurrentGain) ? 0.25f : 0.04f;
            float endGain = state.CurrentGain + (targetGain - state.CurrentGain) * smoothingFactor;
            float startGain = state.CurrentGain;

            // Sample-by-sample linear interpolation of the gain multiplier across the audio frame buffer.
            // This guarantees flawless continuous wave boundaries, permanently eradicating the 50Hz switching buzz.
            for (int i = 0; i < length; i++)
            {
                float interpolationFactor = (float)i / length;
                float currentSampleGain = startGain + (endGain - startGain) * interpolationFactor;
                pcm[i] *= currentSampleGain;
            }

            state.CurrentGain = endGain;
        }
        #endregion
    }
}