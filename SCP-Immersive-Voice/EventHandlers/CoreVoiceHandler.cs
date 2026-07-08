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
    /// <summary>
    /// Core pipeline event listener driving the routing, decoding, and DSP synchronization of live VoIP buffers.
    /// Fully thread-safe and allocation-free using session-insulated rolling ring buffers.
    /// </summary>
    public class CoreVoiceHandler
    {
        #region Private Repositories
        private readonly ImmersiveScpVoiceConfig _config;
        private readonly ScpVoiceManager _voiceManager;
        #endregion

        #region Initialization
        public CoreVoiceHandler(ImmersiveScpVoiceConfig config, ScpVoiceManager voiceManager)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _voiceManager = voiceManager ?? throw new ArgumentNullException(nameof(voiceManager));
        }
        #endregion

        #region Core Network Voice Pipeline Routing
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

            int maxFrameSize = VoiceChatSettings.PacketSizePerChannel;
            float[] tempBuffer = ArrayPool<float>.Shared.Rent(maxFrameSize);

            try
            {
                int samples = ScpVoiceDecoder.Decode(ev.Message, tempBuffer);
                if (samples <= 0 || ScpVoiceDecoder.IsSilent(tempBuffer, samples, threshold: 0.001f))
                    return;

                ScpVoiceDecoder.ApplyEffects(tempBuffer, samples, sender);

                // Step 1: Pre-resolve the stateful session container
                var session = _voiceManager?.StartSession(sender);
                if (session is null) return;

                bool isForbiddenProximity = _config.ForbiddenProximity.Contains(sender.Role);

                if (isForbiddenProximity)
                {
                    byte[] encodedBuffer = ArrayPool<byte>.Shared.Rent(AudioTransmitter.MaxEncodedSize);
                    float[] exactEncodeBuffer = ArrayPool<float>.Shared.Rent(samples);
                    try
                    {
                        Array.Copy(tempBuffer, 0, exactEncodeBuffer, 0, samples);
                        int encodedLength = ScpVoiceDecoder.EncodeToOpus(exactEncodeBuffer, samples, encodedBuffer);
                        if (encodedLength > 0)
                        {
                            Buffer.BlockCopy(encodedBuffer, 0, ev.Message.Data, 0, encodedLength);
                        }
                    }
                    finally
                    {
                        ArrayPool<float>.Shared.Return(exactEncodeBuffer);
                        ArrayPool<byte>.Shared.Return(encodedBuffer);
                    }
                    return;
                }

                if (ev.Message.Channel is VoiceChatChannel.ScpChat)
                {
                    ev.IsAllowed = false;
                }

                // Step 2: Rent a non-shared rolling buffer bound exclusively to this player's session
                // Since it's a 32-node ring, it won't be overwritten for another 640ms, leaving AudioManagerAPI fully protected.
                float[] exactPlaybackBuffer = session.GetNextRollingBuffer(samples);

                // Fast in-place block extraction copy
                Array.Copy(tempBuffer, 0, exactPlaybackBuffer, 0, samples);

                // Forward via the optimized direct session pipe
                _voiceManager?.AppendPcmDirect(session, exactPlaybackBuffer);
            }
            finally
            {
                ArrayPool<float>.Shared.Return(tempBuffer);
            }
        }
        #endregion
    }
}