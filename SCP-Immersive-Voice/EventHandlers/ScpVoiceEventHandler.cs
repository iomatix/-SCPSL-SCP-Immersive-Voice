namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Features.Audio;
    using PlayerRoles;
    using SCP_Immersive_Voice.VoiceProfiles;
    using ScpImmersiveVoice.Config;
    using System;
    using UnityEngine;
    using VoiceChat;
    using VoiceChat.Codec;
    using VoiceChat.Codec.Enums;

    public class ScpVoiceEventHandler
    {
        private readonly ImmersiveScpVoiceConfig _config;

        private readonly OpusDecoder _decoder = new OpusDecoder();
        private readonly OpusEncoder _encoder = new OpusEncoder(OpusApplicationType.Audio);

        public ScpVoiceEventHandler(ImmersiveScpVoiceConfig config)
        {
            _config = config;
        }

        public void OnSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
        {
            if (ev.Player.Role.GetFaction() == Faction.SCP && _config.EnableScpProximityVoice)
            {
                ev.Message.Channel = VoiceChatChannel.Proximity;

                if (_config.ApplyDistortion)
                {
                    ApplyEffects(ev.Message.Data, ev.Message.DataLength, ev.Player.Role);
                }
            }
        }

        public void OnReceivingVoiceMessage(PlayerReceivingVoiceMessageEventArgs ev)
        {
            if (ev.Sender.Role.GetFaction() == Faction.SCP && _config.EnableScpProximityVoice)
            {
                float distance = Vector3.Distance(ev.Player.Position, ev.Sender.Position);
                if (distance > _config.ProximityDistance)
                    ev.IsAllowed = false;
            }
        }

        private void ApplyEffects(byte[] data, int length, RoleTypeId role)
        {
            float[] pcm = new float[AudioTransmitter.FrameSize];
            int samples = _decoder.Decode(data, length, pcm);

            var pipeline = ScpVoiceProfiles.GetPipelineFor(role);
            pipeline.Process(pcm, samples);

            byte[] encoded = new byte[AudioTransmitter.MaxEncodedSize];
            int encodedLength = _encoder.Encode(pcm, samples, encoded);

            Buffer.BlockCopy(encoded, 0, data, 0, encodedLength);
        }


        private void ApplyPitchShift(float[] pcm, int samples, float pitch)
        {
            float[] temp = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float src = i / pitch;
                int i0 = (int)src;
                int i1 = Math.Min(i0 + 1, samples - 1);
                float frac = src - i0;

                temp[i] = pcm[i0] * (1 - frac) + pcm[i1] * frac;
            }

            Array.Copy(temp, pcm, samples);
        }

        private void ApplyDistortion(float[] pcm, int samples, float drive)
        {
            for (int i = 0; i < samples; i++)
            {
                float x = pcm[i] * drive;

                if (x > 0.8f) x = 0.8f;
                if (x < -0.8f) x = -0.8f;

                pcm[i] = x;
            }
        }
    }
}
