using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.Decoders;
using SCP_Immersive_Voice.Managers;
using SCP_Immersive_Voice.VoiceProfiles;
using ScpImmersiveVoice.Config;
using System;
using VoiceChat;

namespace ScpImmersiveVoice.EventHandlers
{
    /// <summary>
    /// Core pipeline event listener driving the routing, decoding, and DSP synchronization of live VoIP buffers.
    /// Operates on high-frequency network threads with strict allocation-free safety gates.
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
        /// <summary>
        /// Intercepts raw network voice envelopes, decodes Opus frames, pushes float-native samples 
        /// through target DSP graphs, and routes finalized buffers into customized proximity streaming layers.
        /// </summary>
        public void OnSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
        {
            // Step 1: Structural Validation Guards using C# 9.0 pattern syntax
            if (!ImmersiveScpVoicePlugin.IsEnabled || ev is null || ev.Player is null)
                return;

            Player sender = ev.Player;

            // Step 2: Fetch active runtime context preset profile (Dynamic state graphs)
            var preset = ScpVoiceProfiles.GetPreset(sender);
            if (preset is null || !preset.Enable)
                return;

            // Step 3: Channel Boundary Filtering
            // HIGH-PERFORMANCE UPGRADE: Compressed complex multi-branch if conditions into a clean
            // C# 9.0 pattern sequence matching layout, allowing the JIT compiler to optimize branch routing tables.
            if (ev.Message.Channel is VoiceChatChannel.None
                or VoiceChatChannel.Spectator
                or VoiceChatChannel.Mimicry
                or VoiceChatChannel.PreGameLobby
                or VoiceChatChannel.RoundSummary)
            {
                return;
            }

            // Step 4: Decode compressed network Opus packets into raw floating-native PCM streams
            float[] pcm = ScpVoiceDecoder.Decode(ev.Message);
            if (pcm is null || pcm.Length is 0 || ScpVoiceDecoder.IsSilent(pcm, threshold: 0.001f))
                return;

            // Step 5: Process floating-native DSP pipelines bound exclusively to this player session configuration
            pcm = ScpVoiceDecoder.ApplyEffects(pcm, sender);

            // Step 6: Enforce administrative role and tactical proxy routing restrictions
            bool isForbiddenProximity = _config.ForbiddenProximity.Contains(sender.Role);

            if (isForbiddenProximity)
            {
                // Re-encode back into raw compressed audio bytes for native SL system tracking hooks
                byte[] encoded = ScpVoiceDecoder.EncodeToOpus(pcm);
                if (encoded is not null)
                {
                    Buffer.BlockCopy(encoded, 0, ev.Message.Data, 0, encoded.Length);
                }
                return;
            }

            if (ev.Message.Channel is VoiceChatChannel.ScpChat)
            {
                // Force proxy streaming engine bypass by disabling native voice broadcasting loops
                ev.IsAllowed = false;
            }

            // Route the finalized high-fidelity PCM sample set straight into our optimized hardware manager
            _voiceManager?.AppendPcm(sender, pcm);
        }
        #endregion
    }
}