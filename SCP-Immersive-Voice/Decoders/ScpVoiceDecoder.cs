namespace SCP_Immersive_Voice.Decoders
{
    using LabApi.Features.Wrappers;
    using System;
    using VoiceChat.Networking;

    public static class ScpVoiceDecoder
    {
        public static short[] Decode(VoiceMessage msg)
        {
            // TODO: tu później damy prawdziwe dekodowanie Opus → PCM
            return Array.Empty<short>();
        }

        public static short[] ApplyEffects(short[] pcm, Player scp)
        {
            // Na razie bez DSP
            return pcm;
        }
    }


}
