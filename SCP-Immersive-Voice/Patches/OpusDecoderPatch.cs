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
        private static readonly FieldInfo CachedHandleField;
        private static readonly bool IsReflectionCacheValid;
        #endregion

        #region Static Cache Initialization
        static OpusDecoderPatch()
        {
            try
            {
                // Parsing reflection strings and assembly structures EXACTLY ONCE during startup.
                // This strips out high-overhead metadata traversal loops from the execution hot path.
                var wrapperType = Type.GetType("VoiceChat.Codec.OpusWrapper, Assembly-CSharp");
                if (wrapperType is not null)
                {
                    CachedCreateDecoder = wrapperType.GetMethod("CreateDecoder",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                }

                CachedHandleField = typeof(OpusDecoder).GetField("_handle",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                IsReflectionCacheValid = CachedCreateDecoder is not null && CachedHandleField is not null;
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
                int sampleRate = VoiceChatSettings.SampleRate;

                // Allocation-free parameters array invocation
                var newHandle = (IntPtr)CachedCreateDecoder.Invoke(null, new object[] { sampleRate, 1 });
                if (newHandle == IntPtr.Zero) return;

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