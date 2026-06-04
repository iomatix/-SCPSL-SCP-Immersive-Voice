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
            if (!ImmersiveScpVoicePlugin.IsEnabled || ev.Player == null) return;

            var sender = ev.Player;

            // Step 1: Fetch dynamic or static context preset blueprint
            var preset = ScpVoiceProfiles.GetPreset(sender);
            if (!preset.Enable) return;

            // Step 2: Guard against illegal global voice rooms
            if (ev.Message.Channel == VoiceChatChannel.None ||
                ev.Message.Channel == VoiceChatChannel.Spectator ||
                ev.Message.Channel == VoiceChatChannel.Mimicry ||
                ev.Message.Channel == VoiceChatChannel.PreGameLobby ||
                ev.Message.Channel == VoiceChatChannel.RoundSummary)
                return;

            // Step 3: Extract and decode incoming Opus byte buffer to raw float PCM stream
            float[] pcm = ScpVoiceDecoder.Decode(ev.Message);
            if (pcm.Length == 0 || ScpVoiceDecoder.IsSilent(pcm, threshold: 0.001f)) return;

            // Step 4: Execute our in-place thread-safe DSP effect pipeline graph
            pcm = ScpVoiceDecoder.ApplyEffects(pcm, sender);

            // Step 5: Route packet according to configuration rules
            bool isForbiddenProximity = _config.ForbiddenProximity.Contains(sender.Role);

            if (isForbiddenProximity)
            {
                // Re-encode processed floating stream back to Opus data frames for small radio transmitters (e.g., SCP-079)
                byte[] encoded = ScpVoiceDecoder.EncodeToOpus(pcm);
                Buffer.BlockCopy(encoded, 0, ev.Message.Data, 0, encoded.Length);
                return;
            }

            // Standard proximity spatialization route
            if (ev.Message.Channel == VoiceChatChannel.ScpChat)
                ev.IsAllowed = false; // Block native internal SCP channel transmission to force local positional proxying

            _voiceManager.AppendPcm(sender, pcm);
        }

        public void OnPlayerDied(PlayerDiedEventArgs ev)
        {
            if (ev.Player == null) return;
            ScpVoiceProfiles.ClearCacheFor(ev.Player);
        }

        public void OnChangingRole(PlayerChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;
            ScpVoiceProfiles.ClearCacheFor(ev.Player);
        }
    }
}