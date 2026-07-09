using HarmonyLib;
using LabApi.Extensions.Misc;
using System;
using System.Reflection;
using VoiceChat;
using VoiceChat.Codec;

namespace ScpImmersiveVoice.Patches
{
    [HarmonyPatch]
    public static class OpusDecoderPatch
    {
        #region Statically Cached Reflection Metadata
        private static readonly MethodInfo CachedCreateDecoder;
        private static readonly MethodInfo CachedDestroyDecoder;
        private static readonly FieldInfo CachedHandleField;
        private static readonly bool IsReflectionCacheValid;
        #endregion

        #region Static Cache Initialization
        static OpusDecoderPatch()
        {
            try
            {
                var wrapperType = Type.GetType("VoiceChat.Codec.OpusWrapper, Assembly-CSharp");
                if (wrapperType is not null)
                {
                    CachedCreateDecoder = wrapperType.GetMethod("CreateDecoder",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    CachedDestroyDecoder = wrapperType.GetMethod("DestroyDecoder",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                }

                CachedHandleField = typeof(OpusDecoder).GetField("_handle",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                IsReflectionCacheValid = CachedCreateDecoder is not null &&
                                         CachedDestroyDecoder is not null &&
                                         CachedHandleField is not null;
            }
            catch (Exception ex)
            {
                iLogger.Error(nameof(OpusDecoderPatch), $"[HARMONY STATIC INIT CACHE CRASH] Failed to bind internal Opus native methods: {ex.Message}");
                IsReflectionCacheValid = false;
            }
        }
        #endregion

        #region Harmony Internal Directives
        static MethodBase TargetMethod() => typeof(OpusDecoder).GetConstructor(Type.EmptyTypes);

        static void Postfix(OpusDecoder __instance)
        {
            if (!IsReflectionCacheValid || __instance is null) return;

            try
            {
                // Capture the original unmanaged handle pointer instantiated by vanilla constructor execution
                IntPtr oldHandle = (IntPtr)CachedHandleField.GetValue(__instance);

                int sampleRate = VoiceChatSettings.SampleRate;
                IntPtr newHandle = (IntPtr)CachedCreateDecoder.Invoke(null, new object[] { sampleRate, 1 });

                if (newHandle == IntPtr.Zero) return;

                // CRITICAL FIX: Safely deallocate the previous native handle to explicitly prevent severe heap memory leaks
                if (oldHandle != IntPtr.Zero)
                {
                    CachedDestroyDecoder.Invoke(null, new object[] { oldHandle });
                }

                CachedHandleField.SetValue(__instance, newHandle);
            }
            catch (Exception ex)
            {
                iLogger.Error(nameof(OpusDecoderPatch), $"[HARMONY CORRUPTION] Critical memory injection dropout inside OpusDecoder runtime hook: {ex.Message}");
            }
        }
        #endregion
    }
}