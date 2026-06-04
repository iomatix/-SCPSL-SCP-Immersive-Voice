namespace ScpImmersiveVoice.Patches
{
    using System;
    using System.Reflection;
    using HarmonyLib;
    using VoiceChat.Codec;
    using VoiceChat.Codec.Enums;
    using VoiceChat;

    [HarmonyPatch]
    public static class OpusEncoderPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(OpusEncoder).GetConstructor(new Type[] { typeof(OpusApplicationType) });
        }

        static void Postfix(OpusEncoder __instance, OpusApplicationType preset)
        {
            try
            {
                int sr = VoiceChatSettings.SampleRate;

                var wrapperType = Type.GetType("VoiceChat.Codec.OpusWrapper, Assembly-CSharp");
                if (wrapperType == null)
                    return;

                var createEncoder = wrapperType.GetMethod("CreateEncoder",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                if (createEncoder == null)
                    return;

                var newHandle = (IntPtr)createEncoder.Invoke(null, new object[] { sr, 1, preset });
                if (newHandle == IntPtr.Zero)
                    return;

                var handleField = typeof(OpusEncoder).GetField("_handle",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                handleField?.SetValue(__instance, newHandle);
            }
            catch { }
        }
    }
}
