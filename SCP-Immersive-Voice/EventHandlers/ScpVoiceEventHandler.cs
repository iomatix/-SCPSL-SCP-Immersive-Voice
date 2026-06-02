namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp096Events;
    using LabApi.Features.Audio;
    using LabApi.Features.Wrappers;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;
    using SCP_Immersive_Voice.VoiceProfiles;
    using ScpImmersiveVoice.Config;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using VoiceChat;
    using VoiceChat.Codec;
    using VoiceChat.Codec.Enums;

    public class ScpVoiceEventHandler
    {
        private readonly ImmersiveScpVoiceConfig _config;
        private readonly Dictionary<Player, Scp096VoiceStateController> _scp096State = new Dictionary<Player, Scp096VoiceStateController>();

        private readonly OpusDecoder _decoder = new OpusDecoder();
        private readonly OpusEncoder _encoder = new OpusEncoder(OpusApplicationType.Audio);

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
        public ScpVoiceEventHandler(ImmersiveScpVoiceConfig config)
        {
            _config = config;
        }

        public void OnSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
        {
            if (_config.EnableScpVoiceEffects)
            {
                ApplyEffects(ev.Message.Data, ev.Message.DataLength, ev.Player);
            }

            if (!_config.EnableScpProximityVoice) return;
            if (ev.Player.Role.GetFaction() != Faction.SCP) return;
            if (_config.ForbiddenProximity.Contains(ev.Player.Role)) return;     

            ev.Message.Channel = VoiceChatChannel.Proximity;     
        }

        public void OnReceivingVoiceMessage(PlayerReceivingVoiceMessageEventArgs ev)
        {
            if (!_config.EnableScpProximityVoice) return;
            if (ev.Sender.Role.GetFaction() != Faction.SCP) return;
            if (_config.ForbiddenProximity.Contains(ev.Sender.Role)) return;   

            float distance = Vector3.Distance(ev.Player.Position, ev.Sender.Position);
            if (distance > _config.ProximityDistance) ev.IsAllowed = false;

        }

        public void On096StartedCrying(Scp096StartedCryingEventArgs ev)
        {
            Get096State(ev.Player).CurrentState = Scp096VoiceState.Crying;
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
