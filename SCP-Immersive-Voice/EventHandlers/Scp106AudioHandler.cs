namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp106Events;
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;
    using System.Collections.Concurrent;

    public class Scp106AudioHandler
    {
        private readonly ConcurrentDictionary<int, Scp106VoiceStateController> _states = new ConcurrentDictionary<int, Scp106VoiceStateController>();

        public ConcurrentDictionary<int, Scp106VoiceStateController> States => _states;

        private Scp106VoiceStateController GetState(Player player)
        {
            if (player == null) return null;
            return _states.GetOrAdd(player.PlayerId, _ => new Scp106VoiceStateController());
        }

        public void OnPlayerDied(PlayerDeathEventArgs ev)
        {
            if (ev?.Player == null) return;
            _states.TryRemove(ev.Player.PlayerId, out _);
        }

        public void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            if (ev?.Player == null) return;
            _states.TryRemove(ev.Player.PlayerId, out _);
        }

        public void On106ChangedStalkMode(Scp106ChangedStalkModeEventArgs ev)
        {
            var state = GetState(ev.Player);
            if (state == null) return;

            if (ev.IsStalkActive)
                state.SetState(Scp106VoiceState.Stalking);
            else
                state.SetState(Scp106VoiceState.Emerging, 3.5f);
        }

        public void On106ChangedVigor(Scp106ChangedVigorEventArgs ev)
        {
            var state = GetState(ev.Player);
            if (state == null) return;

            if (ev.Value < 20f) state.TrySetLowPriorityState(Scp106VoiceState.LowVigor);
            else state.ClearLowVigor();
        }

        public void On106TeleportingPlayer(Scp106TeleportingPlayerEvent ev)
        {
            GetState(ev.Player)?.SetState(Scp106VoiceState.PocketDimension, 5.0f);
        }

        public void On106UsingHunterAtlas(Scp106UsingHunterAtlasEventArgs ev)
        {
            var state = GetState(ev.Player);
            if (state == null) return;

            if (ev.IsAllowed) state.TrySetMediumPriorityState(Scp106VoiceState.AtlasDimensional);
            else if (state.CurrentState == Scp106VoiceState.AtlasDimensional) state.ResetToIdle();
        }
    }
}