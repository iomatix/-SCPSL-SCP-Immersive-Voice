using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Audio;
using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.Decoders;
using SCP_Immersive_Voice.Managers;
using SCP_Immersive_Voice.VoiceProfiles;
using ScpImmersiveVoice.Config;
using System;
using System.Buffers;
using VoiceChat;

namespace ScpImmersiveVoice.EventHandlers
{
    public class CoreVoiceHandler
    {
        private readonly ImmersiveScpVoiceConfig _config;
        private readonly ScpVoiceManager _voiceManager;

        public CoreVoiceHandler(ImmersiveScpVoiceConfig config, ScpVoiceManager voiceManager)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _voiceManager = voiceManager ?? throw new ArgumentNullException(nameof(voiceManager));
        }

        public void OnSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
        {
            if (!ImmersiveScpVoicePlugin.IsEnabled || ev is null || ev.Player is null)
                return;

            Player sender = ev.Player;
            var preset = ScpVoiceProfiles.GetPreset(sender);
            if (preset is null || !preset.Enable)
                return;

            if (ev.Message.Channel is VoiceChatChannel.None
                or VoiceChatChannel.Spectator
                or VoiceChatChannel.Mimicry
                or VoiceChatChannel.PreGameLobby
                or VoiceChatChannel.RoundSummary)
            {
                return;
            }

            var session = _voiceManager?.StartSession(sender);
            if (session is null) return;

            bool isForbiddenProximity = _config.ForbiddenProximity.Contains(sender.Role);

            if (ev.Message.Channel is VoiceChatChannel.ScpChat)
            {
                ev.IsAllowed = false;
            }

            int maxFrameSize = VoiceChatSettings.PacketSizePerChannel > 0 ? VoiceChatSettings.PacketSizePerChannel : 960;
            float[] tempBuffer = ArrayPool<float>.Shared.Rent(maxFrameSize);

            try
            {
                int samples = ScpVoiceDecoder.Decode(session, ev.Message.Data, ev.Message.DataLength, tempBuffer);

                if (samples <= 0)
                    return;

                ScpVoiceDecoder.ApplyEffects(tempBuffer, samples, sender, session);

                if (isForbiddenProximity)
                {
                    byte[] encodedBuffer = ArrayPool<byte>.Shared.Rent(AudioTransmitter.MaxEncodedSize);
                    try
                    {
                        int encodedLength = ScpVoiceDecoder.EncodeToOpus(session, tempBuffer, samples, encodedBuffer);
                        if (encodedLength > 0)
                        {
                            Buffer.BlockCopy(encodedBuffer, 0, ev.Message.Data, 0, encodedLength);
                            ev.Message.DataLength = encodedLength;
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(encodedBuffer);
                    }
                    return;
                }

                float[] exactPlaybackBuffer = session.GetNextFixedBuffer();
                Array.Copy(tempBuffer, 0, exactPlaybackBuffer, 0, samples);

                _voiceManager?.AppendPcmDirect(session, exactPlaybackBuffer);
            }
            catch (Exception ex)
            {
                LabApi.Extensions.Misc.iLogger.Error(nameof(CoreVoiceHandler), $"Audio Processing Error: {ex.Message}");
            }
            finally
            {
                ArrayPool<float>.Shared.Return(tempBuffer);
            }
        }
    }
}