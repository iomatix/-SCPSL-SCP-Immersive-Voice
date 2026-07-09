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
        private static readonly MethodInfo CachedDestroyEncoder;
        private static readonly FieldInfo CachedHandleField;
        private static readonly bool IsReflectionCacheValid;
        #endregion

        #region Static Cache Initialization
        static OpusEncoderPatch()
        {
            try
            {
                var wrapperType = Type.GetType("VoiceChat.Codec.OpusWrapper, Assembly-CSharp");
                if (wrapperType is not null)
                {
                    CachedCreateEncoder = wrapperType.GetMethod("CreateEncoder",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    CachedDestroyEncoder = wrapperType.GetMethod("DestroyEncoder",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                }

                CachedHandleField = typeof(OpusEncoder).GetField("_handle",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                IsReflectionCacheValid = CachedCreateEncoder is not null &&
                                         CachedDestroyEncoder is not null &&
                                         CachedHandleField is not null;
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
                // Capture the original unmanaged handle pointer instantiated by vanilla constructor execution
                IntPtr oldHandle = (IntPtr)CachedHandleField.GetValue(__instance);

                int sampleRate = VoiceChatSettings.SampleRate;
                IntPtr newHandle = (IntPtr)CachedCreateEncoder.Invoke(null, new object[] { sampleRate, 1, preset });

                if (newHandle == IntPtr.Zero) return;

                // CRITICAL FIX: Safely deallocate the previous native handle to explicitly prevent severe heap memory leaks
                if (oldHandle != IntPtr.Zero)
                {
                    CachedDestroyEncoder.Invoke(null, new object[] { oldHandle });
                }

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