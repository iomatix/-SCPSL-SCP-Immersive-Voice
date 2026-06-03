namespace SCP_Immersive_Voice.Decoders
{
    using LabApi.Features.Wrappers;
    using System;
    using VoiceChat.Networking;
    using VoiceChat.Codec;
    using VoiceChat.Codec.Enums;

    public static class ScpVoiceDecoder
    {
        // Constant decoder for Opus → PCM (float)
        private static readonly OpusDecoder _decoder = new OpusDecoder();

        // Float buffer (LabAPI uses 48000 Hz, mono, frame size = 1920 float samples)
        private static readonly float[] _floatBuffer = new float[1920];

        // Main decoder: VoiceMessage → short[] PCM
        public static short[] Decode(VoiceMessage msg)
        {

            // Validate input
            try
            {
                if (msg.Data == null || msg.DataLength <= 0) return Array.Empty<short>();
            }
            catch
            {
                return Array.Empty<short>();
            }

            // Decode Opus → float[]
            int samples = _decoder.Decode(msg.Data, msg.DataLength, _floatBuffer);

            if (samples <= 0)
                return Array.Empty<short>();

            // Convert the float samples to short (16-bit PCM)
            short[] pcm = new short[samples];
            for (int i = 0; i < samples; i++)
            {
                float f = _floatBuffer[i];

                // Clamp → convert
                if (f > 1f) f = 1f;
                if (f < -1f) f = -1f;

                pcm[i] = (short)(f * short.MaxValue);
            }

            return pcm;
        }

        // DSP pipeline
        public static short[] ApplyEffects(short[] pcm, Player scp)
        {
            // empty for now, but this is where to apply any audio effects based on the SCP's state
            return pcm;
        }
    }
}
