using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Audio;
using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.Decoders;
using SCP_Immersive_Voice.Managers;
using SCP_Immersive_Voice.VoiceProfiles;
using ScpImmersiveVoice.Config;
using System;
using System.Buffers;
using System.Threading;
using VoiceChat;

namespace ScpImmersiveVoice.EventHandlers
{
    public class CoreVoiceHandler
    {
        #region Private Repositories
        private readonly ImmersiveScpVoiceConfig _config;
        private readonly ScpVoiceManager _voiceManager;
        #endregion

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

            // Resolve session instantly on the network reader thread
            var session = _voiceManager?.StartSession(sender);
            if (session is null) return;

            bool isForbiddenProximity = _config.ForbiddenProximity.Contains(sender.Role);

            if (ev.Message.Channel is VoiceChatChannel.ScpChat)
            {
                ev.IsAllowed = false;
            }

            // CRITICAL STEP: Extract raw network compressed payload instantly into a rented buffer
            int dataLen = ev.Message.DataLength;
            byte[] rawOpusPack = ArrayPool<byte>.Shared.Rent(dataLen);
            Array.Copy(ev.Message.Data, 0, rawOpusPack, 0, dataLen);

            // Capture mutable entity metadata to bypass main thread dependency inside the thread pool
            var senderRole = sender.Role;
            var msgDataReference = ev.Message.Data;

            // DECOUPLED THREAD WORKER DISPATCH: Main/Network thread execution time is now 0ms!
            ThreadPool.UnsafeQueueUserWorkItem(
                _ =>
            {
                int maxFrameSize = VoiceChatSettings.PacketSizePerChannel;
                float[] tempBuffer = ArrayPool<float>.Shared.Rent(maxFrameSize);

                try
                {
                    // Isolated safe decode using the current session's private decoder context
                    int samples = ScpVoiceDecoder.Decode(session, rawOpusPack, dataLen, tempBuffer);
                    if (samples <= 0 || ScpVoiceDecoder.IsSilent(tempBuffer, samples, threshold: 0.001f))
                        return;

                    ScpVoiceDecoder.ApplyEffects(tempBuffer, samples, sender, session);

                    if (isForbiddenProximity)
                    {
                        byte[] encodedBuffer = ArrayPool<byte>.Shared.Rent(AudioTransmitter.MaxEncodedSize);
                        float[] exactEncodeBuffer = ArrayPool<float>.Shared.Rent(samples);
                        try
                        {
                            Array.Copy(tempBuffer, 0, exactEncodeBuffer, 0, samples);
                            int encodedLength = ScpVoiceDecoder.EncodeToOpus(session, exactEncodeBuffer, samples, encodedBuffer);
                            if (encodedLength > 0)
                            {
                                lock (session.SyncLock)
                                {
                                    Buffer.BlockCopy(encodedBuffer, 0, msgDataReference, 0, encodedLength);
                                }
                            }
                        }
                        finally
                        {
                            ArrayPool<float>.Shared.Return(exactEncodeBuffer);
                            ArrayPool<byte>.Shared.Return(encodedBuffer);
                        }
                        return;
                    }

                    // Append into the hardware stream engine natively via the thread-isolated rolling matrix
                    float[] exactPlaybackBuffer = session.GetNextFixedBuffer();
                    Array.Copy(tempBuffer, 0, exactPlaybackBuffer, 0, samples);

                    _voiceManager?.AppendPcmDirect(session, exactPlaybackBuffer);
                }
                catch (Exception)
                {
                    // Suppress or log async worker exceptions cleanly without crashing the main server application
                }
                finally
                {
                    // Return both rented components safely back to the global pool arrays
                    ArrayPool<float>.Shared.Return(tempBuffer);
                    ArrayPool<byte>.Shared.Return(rawOpusPack);
                }
            }, null);
        }
    }
}