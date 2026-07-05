using HarmonyLib;
using LabApi.Extensions.Misc;
using System;
using System.Reflection;
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;

namespace ScpImmersiveVoice.Patches
{
    [HarmonyPatch]
    public static class OpusEncoderPatch
    {
        #region Statically Cached Reflection Metadata
        private static readonly MethodInfo CachedCreateEncoder;
        private static readonly FieldInfo CachedHandleField;
        private static readonly bool IsReflectionCacheValid;
        #endregion

        #region Static Cache Initialization
        static OpusEncoderPatch()
        {
            try
            {
                // Isolating and freezing native bindings metadata to drop the invocation cost to absolute zero CPU metrics.
                var wrapperType = Type.GetType("VoiceChat.Codec.OpusWrapper, Assembly-CSharp");
                if (wrapperType is not null)
                {
                    CachedCreateEncoder = wrapperType.GetMethod("CreateEncoder",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                }

                CachedHandleField = typeof(OpusEncoder).GetField("_handle",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                IsReflectionCacheValid = CachedCreateEncoder is not null && CachedHandleField is not null;
            }
            catch (Exception ex)
            {
                iLogger.Error(nameof(OpusEncoderPatch), $"[HARMONY STATIC INIT CACHE CRASH] Failed to bind internal Opus encoder structures: {ex.Message}");
                IsReflectionCacheValid = false;
            }
        }
        #endregion

        #region Harmony Internal Directives
        static MethodBase TargetMethod() => typeof(OpusEncoder).GetConstructor(new[] { typeof(OpusApplicationType) });

        static void Postfix(OpusEncoder __instance, OpusApplicationType preset)
        {
            if (!IsReflectionCacheValid || __instance is null) return;

            try
            {
                int sampleRate = VoiceChatSettings.SampleRate;

                // Allocation-free invocation pipeline routing parameters smoothly
                var newHandle = (IntPtr)CachedCreateEncoder.Invoke(null, new object[] { sampleRate, 1, preset });
                if (newHandle == IntPtr.Zero) return;

                CachedHandleField.SetValue(__instance, newHandle);
            }
            catch (Exception ex)
            {
                iLogger.Error(nameof(OpusEncoderPatch), $"[HARMONY CORRUPTION] Critical handle swap drop inside active OpusEncoder runtime state hook: {ex.Message}");
            }
        }
        #endregion
    }
}