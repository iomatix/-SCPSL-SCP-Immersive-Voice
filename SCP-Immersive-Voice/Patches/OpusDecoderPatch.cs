namespace ScpImmersiveVoice.Patches
{
    using System;
    using System.Reflection;
    using HarmonyLib;
    using VoiceChat.Codec;
    using VoiceChat;

    [HarmonyPatch]
    public static class OpusDecoderPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(OpusDecoder).GetConstructor(Type.EmptyTypes);
        }

        static void Postfix(OpusDecoder __instance)
        {
            try
            {
                int sr = VoiceChatSettings.SampleRate;

                var wrapperType = Type.GetType("VoiceChat.Codec.OpusWrapper, Assembly-CSharp");
                if (wrapperType == null)
                    return;

                var createDecoder = wrapperType.GetMethod("CreateDecoder",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                if (createDecoder == null)
                    return;

                var newHandle = (IntPtr)createDecoder.Invoke(null, new object[] { sr, 1 });
                if (newHandle == IntPtr.Zero)
                    return;

                var handleField = typeof(OpusDecoder).GetField("_handle",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                handleField?.SetValue(__instance, newHandle);
            }
            catch { }
        }
    }
}
