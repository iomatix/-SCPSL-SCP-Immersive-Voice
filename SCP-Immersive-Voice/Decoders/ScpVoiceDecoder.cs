namespace SCP_Immersive_Voice.Decoders
{
    using LabApi.Features.Audio;
    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.VoiceProfiles;
    using System;
    using VoiceChat.Codec;
    using VoiceChat.Codec.Enums;
    using VoiceChat.Networking;

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
            Logger.Debug($"[SCP-VOICE] Decode: data={msg.DataLength}, samples={samples}");


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

        public static byte[] EncodeToOpus(short[] pcm)
        {
            float[] f = ToFloat(pcm);

            var encoder = new OpusEncoder(OpusApplicationType.Voip);
            byte[] encoded = new byte[AudioTransmitter.MaxEncodedSize];

            int len = encoder.Encode(f, encoded, f.Length);
            Array.Resize(ref encoded, len);

            return encoded;
        }


        // DSP pipeline
        public static short[] ApplyEffects(short[] pcm, Player scp)
        {
            // 1. Convert to float
            float[] f = ToFloat(pcm);

            // 2. Get DSP pipeline
            var pipeline = ScpVoiceProfiles.GetPipelineFor(scp);

            // 3. Process float PCM
            pipeline.Process(f, f.Length);

            // 4. Convert back to short
            short[] processed = ToShort(f);

            // 5. Apply OutputGain
            var preset = ScpVoiceProfiles.GetPreset(scp);
            if (preset.OutputGain != 1f)
            {
                for (int i = 0; i < processed.Length; i++)
                    processed[i] = (short)Clamp(processed[i] * preset.OutputGain, short.MinValue, short.MaxValue);
            }

            // 6. Normalize
            processed = Normalize(processed);

            return processed;
        }

        public static bool IsSilent(short[] pcm, int threshold = 200)
        {
            // threshold = max amplitude below which we consider the frame silent
            for (int i = 0; i < pcm.Length; i++)
            {
                if (Math.Abs(pcm[i]) > threshold)
                    return false; // real speech
            }
            return true; // silence
        }

        public static short[] Normalize(short[] pcm, float targetPeak = 0.9f)
        {
            short max = 0;

            // find peak amplitude
            for (int i = 0; i < pcm.Length; i++)
            {
                short abs = (short)Math.Abs(pcm[i]);
                if (abs > max) max = abs;
            }

            if (max < 1)
                return pcm; // silence

            float gain = (targetPeak * 32767f) / max;

            short[] output = new short[pcm.Length];
            for (int i = 0; i < pcm.Length; i++)
            {
                output[i] = (short)Clamp(pcm[i] * gain, short.MinValue, short.MaxValue);
            }

            return output;
        }

        public static float[] ToFloat(short[] pcm)
        {
            float[] f = new float[pcm.Length];
            for (int i = 0; i < pcm.Length; i++)
                f[i] = pcm[i] / 32768f;
            return f;
        }

        public static short[] ToShort(float[] pcm)
        {
            short[] s = new short[pcm.Length];
            for (int i = 0; i < pcm.Length; i++)
                s[i] = (short)Clamp(pcm[i] * 32767f, short.MinValue, short.MaxValue);
            return s;
        }

        private static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

    }
}
