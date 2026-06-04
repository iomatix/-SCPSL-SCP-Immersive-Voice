namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp096Events;
    using LabApi.Events.Arguments.Scp106Events;
    using LabApi.Events.Arguments.Scp3114Events;
    using LabApi.Events.Arguments.Scp939Events;
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.Decoders;
    using SCP_Immersive_Voice.Managers;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;
    using SCP_Immersive_Voice.VoiceProfiles;
    using ScpImmersiveVoice.Config;
    using System;
    using System.Collections.Generic;
    using VoiceChat;

    public class ScpVoiceEventHandler
    {
        private readonly ImmersiveScpVoiceConfig _config;
        private readonly ScpVoiceManager _voiceManager;

        // Constructor
        public ScpVoiceEventHandler(ImmersiveScpVoiceConfig config, ScpVoiceManager voiceManager)
        {
            _config = config;
            _voiceManager = voiceManager;
        }

        #region SCP State Tracking Controllers
        // SCP-096 Dynamic State Tracking
        /// <summary>
        /// The key of this dictionary is PlayerId.
        /// </summary>
        private readonly Dictionary<int, Scp096VoiceStateController> _scp096State = new Dictionary<int, Scp096VoiceStateController>();
        public Dictionary<int, Scp096VoiceStateController> Scp096States => _scp096State;
        private Scp096VoiceStateController Get096State(Player player)
        {
            if (!_scp096State.TryGetValue(player.PlayerId, out var controller))
            {
                controller = new Scp096VoiceStateController();
                _scp096State[player.PlayerId] = controller;
            }

            return controller;
        }

        // SCP-939 Dynamic State Tracking
        /// <summary>
        /// The key of this dictionary is PlayerId.
        /// </summary>
        private readonly Dictionary<int, Scp939VoiceStateController> _scp939State = new Dictionary<int, Scp939VoiceStateController>();


        public Dictionary<int, Scp939VoiceStateController> Scp939States => _scp939State;

        private Scp939VoiceStateController Get939State(Player player)
        {
            if (!_scp939State.TryGetValue(player.PlayerId, out var controller))
            {
                controller = new Scp939VoiceStateController();
                _scp939State[player.PlayerId] = controller;
            }

            return controller;
        }

        /// <summary>
        /// Ensures SCP-939 does not get stuck in a transient state.
        /// </summary>
        private void Reset939IfStuck(Player player)
        {
            var state = Get939State(player);

            // States that are allowed to persist
            bool isActive =
                state.CurrentState == Scp939VoiceState.Mimicking ||
                state.CurrentState == Scp939VoiceState.Focused ||
                state.CurrentState == Scp939VoiceState.Attacking ||
                state.CurrentState == Scp939VoiceState.AmnesticCloud;

            // If not in an active state → fallback to IdleWhisper
            if (!isActive)
                state.CurrentState = Scp939VoiceState.IdleWhisper;
        }


        // SCP-3114 Dynamic State Tracking
        private readonly Dictionary<int, Scp3114VoiceStateController> _scp3114State = new Dictionary<int, Scp3114VoiceStateController>();

        /// <summary>
        /// The key of this dictionary is PlayerId.
        /// </summary>
        public Dictionary<int, Scp3114VoiceStateController> Scp3114States => _scp3114State;

        private Scp3114VoiceStateController Get3114State(Player player)
        {
            if (!_scp3114State.TryGetValue(player.PlayerId, out var controller))
            {
                controller = new Scp3114VoiceStateController();
                _scp3114State[player.PlayerId] = controller;
            }

            return controller;
        }


        // SCP-106 Dynamic State Tracking
        private readonly Dictionary<int, Scp106VoiceStateController> _scp106State = new Dictionary<int, Scp106VoiceStateController>();

        /// <summary>
        /// The key of this dictionary is PlayerId.
        /// </summary>
        public Dictionary<int, Scp106VoiceStateController> Scp106States => _scp106State;

        private Scp106VoiceStateController Get106State(Player player)
        {
            if (!_scp106State.TryGetValue(player.PlayerId, out var controller))
            {
                controller = new Scp106VoiceStateController();
                _scp106State[player.PlayerId] = controller;
            }

            return controller;
        }
        #endregion

        #region Player Voice Events
        public void OnSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
        {
            if (!ImmersiveScpVoicePlugin.IsEnabled) return;

            var sender = ev.Player;

            // Only process if this class has a preset
            var preset = ScpVoiceProfiles.GetPreset(sender);
            if (!preset.Enable) return;

            // Only process voice messages
            if (ev.Message.Channel == VoiceChatChannel.None || ev.Message.Channel == VoiceChatChannel.Spectator || ev.Message.Channel == VoiceChatChannel.Mimicry || ev.Message.Channel == VoiceChatChannel.PreGameLobby || ev.Message.Channel == VoiceChatChannel.RoundSummary)
                return;

            bool isForbidden = _config.ForbiddenProximity.Contains(sender.Role);

            // Decode Opus → float PCM
            float[] pcm = ScpVoiceDecoder.Decode(ev.Message);
            // Gate
            if (pcm.Length == 0 || ScpVoiceDecoder.IsSilent(pcm, threshold: 0.001f)) return;

            // Apply Effects Pipeline
            pcm = ScpVoiceDecoder.ApplyEffects(pcm, sender);
            
            // Logic
            ev.IsAllowed = false; // disable all events, allow only speaker sound for debugg TODO: REMOVE THIS LINE
            // --- CASE 1: Forbidden proximity (e.g. 079 - defined in config.) ---
            // Apply effects only TODO
            if (isForbidden)
            {
                // Re-encode float PCM → OPUS so original channel sends modified voice
                byte[] encoded = ScpVoiceDecoder.EncodeToOpus(pcm);

                Buffer.BlockCopy(encoded, 0, ev.Message.Data, 0, encoded.Length);
                //ev.IsAllowed = true; // TODO: Bring it back to true after debug
                return;
            }

            // --- CASE 2: Allowed proximity (SCPs) ---
            // Disable original channel and make it proxy via Appending TODO
            if (ev.Message.Channel == VoiceChatChannel.ScpChat)
                ev.IsAllowed = false; // block original SCPChat

            // Send float PCM to proximity audio system
            _voiceManager.AppendPcm(sender, pcm);
        }
        #endregion

        #region SCP Dynamic State Events
        // ----------------------
        // SCP-096 dynamic states
        // ----------------------
        public void On096StartingCrying(Scp096StartCryingEventArgs ev)
        {
            Get096State(ev.Player).CurrentState = Scp096VoiceState.Crying;
        }

        public void On096StartedCrying(Scp096StartedCryingEventArgs ev)
        {
            Get096State(ev.Player).CurrentState = Scp096VoiceState.Crying;
        }

        public void On096Enraging(Scp096EnragingEventArgs ev)
        {
            Get096State(ev.Player).CurrentState = Scp096VoiceState.Enraging;
        }

        public void On096Enraged(Scp096EnragedEventArgs ev)
        {
            Get096State(ev.Player).CurrentState = Scp096VoiceState.Enraged;
        }

        public void On096TryingNotToCry(Scp096TryingNotToCryEventArgs ev)
        {
            Get096State(ev.Player).CurrentState = Scp096VoiceState.TryingNotToCry;
        }

        public void On096Charging(Scp096ChargingEventArgs ev)
        {
            Get096State(ev.Player).CurrentState = Scp096VoiceState.Charging;
        }

        public void On096Charged(Scp096ChargedEventArgs ev)
        {
            Get096State(ev.Player).CurrentState = Scp096VoiceState.Charged;
        }

        // ----------------------
        // SCP-939 dynamic states
        // ----------------------
        public void On939MimickingEnvironment(Scp939MimickingEnvironmentEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.Mimicking;
            Reset939IfStuck(ev.Player);
        }

        public void On939MimickedEnvironment(Scp939MimickedEnvironmentEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.IdleWhisper;
            Reset939IfStuck(ev.Player);
        }

        public void On939Focused(Scp939FocusedEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.Focused;
            Reset939IfStuck(ev.Player);
        }

        public void On939Attacking(Scp939AttackingEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.Attacking;
            Reset939IfStuck(ev.Player);
        }

        public void On939Attacked(Scp939AttackedEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.IdleWhisper;
            Reset939IfStuck(ev.Player);
        }

        public void On939Lunging(Scp939LungingEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.Attacking;
            Reset939IfStuck(ev.Player);
        }

        public void On939Lunged(Scp939LungedEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.IdleWhisper;
            Reset939IfStuck(ev.Player);
        }

        public void On939CreatingAmnesticCloud(Scp939CreatingAmnesticCloudEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.AmnesticCloud;
            Reset939IfStuck(ev.Player);
        }

        public void On939CreatedAmnesticCloud(Scp939CreatedAmnesticCloudEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.IdleWhisper;
            Reset939IfStuck(ev.Player);
        }

        // ----------------------
        // SCP-3114 dynamic states
        // ----------------------

        public void On3114Disguising(Scp3114DisguisingEventArgs ev)
        {
            Get3114State(ev.Player).CurrentState = Scp3114VoiceState.Disguising;
        }

        public void On3114Disguised(Scp3114DisguisedEventArgs ev)
        {
            Get3114State(ev.Player).CurrentState = Scp3114VoiceState.Disguised;
        }

        public void On3114Revealing(Scp3114RevealingEventArgs ev)
        {
            Get3114State(ev.Player).CurrentState = Scp3114VoiceState.Revealing;
        }

        public void On3114Revealed(Scp3114RevealedEventArgs ev)
        {
            Get3114State(ev.Player).CurrentState = Scp3114VoiceState.Undisguised;
        }

        public void On3114StrangleStarting(Scp3114StrangleStartingEventArgs ev)
        {
            Get3114State(ev.Player).CurrentState = Scp3114VoiceState.Strangling;
        }

        public void On3114StrangleStarted(Scp3114StrangleStartedEventArgs ev)
        {
            Get3114State(ev.Player).CurrentState = Scp3114VoiceState.Strangling;
        }

        public void On3114StrangleAborting(Scp3114StrangleAbortingEventArgs ev)
        {
            Get3114State(ev.Player).CurrentState = Scp3114VoiceState.Undisguised;
        }

        public void On3114StrangleAborted(Scp3114StrangleAbortedEventArgs ev)
        {
            Get3114State(ev.Player).CurrentState = Scp3114VoiceState.Undisguised;
        }


        // ----------------------
        // SCP-106 dynamic states
        // ----------------------
        public void On106ChangedStalkMode(Scp106ChangedStalkModeEventArgs ev)
        {
            var state = Get106State(ev.Player);

            if (ev.IsStalkActive)
            {
                // Intent: submerged, muffled, distant presence
                state.SetState(Scp106VoiceState.Stalking);
            }
            else
            {
                // Intent: rising from the ground with wet burst
                state.SetTemporaryState(
                    Scp106VoiceState.Emerging,
                    fallback: Scp106VoiceState.Idle
                );
            }
        }

        public void On106ChangedVigor(Scp106ChangedVigorEventArgs ev)
        {
            var state = Get106State(ev.Player);

            // Intent: heavy, collapsing resonance when exhausted
            if (ev.Value < 20f)
                state.TrySetLowPriorityState(Scp106VoiceState.LowVigor);
            else
                state.ClearLowVigor();
        }

        public void On106TeleportingPlayer(Scp106TeleportingPlayerEvent ev)
        {
            var state = Get106State(ev.Player);

            // Intent: dimensional echo during pocket transition
            state.SetTemporaryState(
                Scp106VoiceState.PocketDimension,
                fallback: Scp106VoiceState.Idle
            );
        }

        public void On106UsingHunterAtlas(Scp106UsingHunterAtlasEventArgs ev)
        {
            var state = Get106State(ev.Player);

            if (ev.IsAllowed)
            {
                // Intent: subtle dimensional smear while sensing rooms
                state.TrySetMediumPriorityState(Scp106VoiceState.AtlasDimensional);
            }
            else
            {
                // Intent: return to baseline when Atlas closes
                if (state.CurrentState == Scp106VoiceState.AtlasDimensional)
                    state.SetState(Scp106VoiceState.Idle);
            }
        }

        #endregion
    }
}
