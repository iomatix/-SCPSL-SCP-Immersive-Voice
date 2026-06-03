namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp096Events;
    using LabApi.Events.Arguments.Scp3114Events;
    using LabApi.Events.Arguments.Scp939Events;
    using LabApi.Features.Audio;
    using LabApi.Features.Wrappers;
    using PlayerRoles;
    using PlayerRoles.Voice;
    using SCP_Immersive_Voice.Decoders;
    using SCP_Immersive_Voice.Managers;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;
    using SCP_Immersive_Voice.VoiceProfiles;
    using ScpImmersiveVoice.Config;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using VoiceChat;
    using VoiceChat.Codec;
    using VoiceChat.Codec.Enums;
    using VoiceChat.Networking;

    public class ScpVoiceEventHandler
    {
        private readonly ImmersiveScpVoiceConfig _config;

        private readonly OpusDecoder _decoder = new OpusDecoder();
        private readonly OpusEncoder _encoder = new OpusEncoder(OpusApplicationType.Audio);

        // Constructor
        public ScpVoiceEventHandler(ImmersiveScpVoiceConfig config)
        {
            _config = config;
        }

        #region SCP State Tracking Controllers
        // SCP-096 Dynamic State Tracking
        private readonly Dictionary<Player, Scp096VoiceStateController> _scp096State = new Dictionary<Player, Scp096VoiceStateController>();
        public Dictionary<Player, Scp096VoiceStateController> Scp096States => _scp096State;
        private Scp096VoiceStateController Get096State(Player player)
        {
            if (!_scp096State.TryGetValue(player, out var controller))
            {
                controller = new Scp096VoiceStateController();
                _scp096State[player] = controller;
            }

            return controller;
        }

        // SCP-939 Dynamic State Tracking
        private readonly Dictionary<Player, Scp939VoiceStateController> _scp939State = new Dictionary<Player, Scp939VoiceStateController>();

        public Dictionary<Player, Scp939VoiceStateController> Scp939States => _scp939State;

        private Scp939VoiceStateController Get939State(Player player)
        {
            if (!_scp939State.TryGetValue(player, out var controller))
            {
                controller = new Scp939VoiceStateController();
                _scp939State[player] = controller;
            }

            return controller;
        }

        // SCP-3114 Dynamic State Tracking
        private readonly Dictionary<Player, Scp3114VoiceStateController> _scp3114State = new Dictionary<Player, Scp3114VoiceStateController>();

        public Dictionary<Player, Scp3114VoiceStateController> Scp3114States => _scp3114State;

        private Scp3114VoiceStateController Get3114State(Player player)
        {
            if (!_scp3114State.TryGetValue(player, out var controller))
            {
                controller = new Scp3114VoiceStateController();
                _scp3114State[player] = controller;
            }

            return controller;
        }

        #endregion

        #region Player Voice Events
        public void OnSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
        {
            var sender = ev.Player;

            // Only for SCP players    
            if (!sender.IsSCP)
                return;

            // Only for SCPs    
            if (sender.Role.GetFaction() != Faction.SCP)
                return;

            // Only ScpChat channel (Q)    
            if (ev.Message.Channel != VoiceChatChannel.ScpChat)
                return;

            // Add DSP effects if enabled    
            if (_config.EnableScpVoiceEffects)
                ApplyEffects(ev.Message.Data, ev.Message.DataLength, sender);

            ev.IsAllowed = false;

            short[] pcm = ScpVoiceDecoder.Decode(ev.Message);
            pcm = ScpVoiceDecoder.ApplyEffects(pcm, sender);

            ScpVoiceManager.Instance.AppendPcm(ev.Player, pcm);

        }

        public void OnReceivingVoiceMessage(PlayerReceivingVoiceMessageEventArgs ev)
        {
            // This event can be used for additional client-side processing if needed, but for now we rely on the sender-side processing and proximity filtering in OnSendingVoiceMessage.

            //var sender = ev.Sender;
            //var receiver = ev.Player;

            //// Only process SCP messages  
            //if (!sender.IsSCP) return;

            //// Only process ScpChat channel messages  
            //if (ev.Message.Channel != VoiceChatChannel.ScpChat) return;

            //// Check if sender's role can use proximity voice  
            //if (_config.ForbiddenProximity.Contains(sender.Role)) return;      

            //// Distance filter - only allow if within proximity distance  
            //float dist = Vector3.Distance(receiver.Position, sender.Position);
            //if (dist > _config.ProximityDistance)
            //{
            //    ev.IsAllowed = false;
            //    return;
            //}
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
        }

        public void On939MimickedEnvironment(Scp939MimickedEnvironmentEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.IdleWhisper;
        }

        public void On939Focused(Scp939FocusedEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.Focused;
        }

        public void On939Attacking(Scp939AttackingEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.Attacking;
        }

        public void On939Attacked(Scp939AttackedEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.IdleWhisper;
        }

        public void On939Lunging(Scp939LungingEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.Attacking;
        }

        public void On939Lunged(Scp939LungedEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.IdleWhisper;
        }

        public void On939CreatingAmnesticCloud(Scp939CreatingAmnesticCloudEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.AmnesticCloud;
        }

        public void On939CreatedAmnesticCloud(Scp939CreatedAmnesticCloudEventArgs ev)
        {
            Get939State(ev.Player).CurrentState = Scp939VoiceState.IdleWhisper;
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
        #endregion


        // ----------------------
        // Core audio processing
        // ----------------------
        private void ApplyEffects(byte[] data, int length, Player player)
        {
            float[] pcm = new float[AudioTransmitter.FrameSize];
            int samples = _decoder.Decode(data, length, pcm);

            var pipeline = ScpVoiceProfiles.GetPipelineFor(player, _config);

            pipeline.Process(pcm, samples);

            byte[] encoded = new byte[AudioTransmitter.MaxEncodedSize];
            int encodedLength = _encoder.Encode(pcm, encoded, samples);

            Buffer.BlockCopy(encoded, 0, data, 0, encodedLength);
        }
    }
}
