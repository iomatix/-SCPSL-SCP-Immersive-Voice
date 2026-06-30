namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.Decoders;
    using SCP_Immersive_Voice.Managers;
    using SCP_Immersive_Voice.VoiceProfiles;
    using ScpImmersiveVoice.Config;
    using System;
    using VoiceChat;

    /// <summary>
    /// Core pipeline event listener driving the routing, decoding, and DSP synchronization of live VoIP buffers.
    /// </summary>
    public class CoreVoiceHandler
    {
        private readonly ImmersiveScpVoiceConfig _config;
        private readonly ScpVoiceManager _voiceManager;

        public CoreVoiceHandler(ImmersiveScpVoiceConfig config, ScpVoiceManager voiceManager)
        {
            _config = config;
            _voiceManager = voiceManager;
        }

        public void OnSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
        {
            if (!ImmersiveScpVoicePlugin.IsEnabled || ev == null || ev.Player == null) return;

            var sender = ev.Player;

            // Step 1: Fetch runtime context preset (Dynamic or Static)
            var preset = ScpVoiceProfiles.GetPreset(sender);
            if (preset == null || !preset.Enable) return;

            // Step 2: Guard boundaries against illegal rooms
            if (ev.Message.Channel == VoiceChatChannel.None ||
                ev.Message.Channel == VoiceChatChannel.Spectator ||
                ev.Message.Channel == VoiceChatChannel.Mimicry ||
                ev.Message.Channel == VoiceChatChannel.PreGameLobby ||
                ev.Message.Channel == VoiceChatChannel.RoundSummary)
                return;

            // Step 3: Decode Opus to raw float PCM stream
            float[] pcm = ScpVoiceDecoder.Decode(ev.Message);
            if (pcm == null || pcm.Length == 0 || ScpVoiceDecoder.IsSilent(pcm, threshold: 0.001f)) return;

            // Step 4: Resolve session container and atomically validate/synchronize DSP graph state
            var session = _voiceManager.StartSession(sender);
            if (session == null) return;

            lock (session.SyncLock)
            {
                if (session.LastAppliedPreset == null || !ScpVoiceProfiles.ArePresetsAcousticallyIdentical(session.LastAppliedPreset, preset))
                {
                    session.SynchronizePipelineGraph(preset);
                    session.LastAppliedPreset = preset.Clone();
                }
            }

            // Process thread-safe float-native DSP pipeline graphs bound exclusively to this session context
            pcm = ScpVoiceDecoder.ApplyEffects(pcm, sender);

            // Step 5: Route message (pozostała część metody bez zmian) ...
            bool isForbiddenProximity = _config.ForbiddenProximity.Contains(sender.Role);

            if (isForbiddenProximity)
            {
                byte[] encoded = ScpVoiceDecoder.EncodeToOpus(pcm);
                Buffer.BlockCopy(encoded, 0, ev.Message.Data, 0, encoded.Length);
                return;
            }

            if (ev.Message.Channel == VoiceChatChannel.ScpChat)
                ev.IsAllowed = false;

            _voiceManager.AppendPcm(sender, pcm);
        }

        public void OnPlayerDied(PlayerDeathEventArgs ev)
        {
            if (ev != null && ev.Player != null)
            {
                ScpVoiceProfiles.ClearCacheFor(ev.Player);
            }
        }

        public void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            if (ev != null && ev.Player != null)
            {
                ScpVoiceProfiles.ClearCacheFor(ev.Player);
            }
        }
    }
}