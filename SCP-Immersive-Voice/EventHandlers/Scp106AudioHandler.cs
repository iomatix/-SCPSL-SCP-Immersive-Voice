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

        private Scp106VoiceStateController GetState(Player player) => _states.GetOrAdd(player.PlayerId, _ => new Scp106VoiceStateController());

        public void OnPlayerDied(PlayerDeathEventArgs ev) => _states.TryRemove(ev.Player.PlayerId, out _);
        public void OnChangingRole(PlayerChangingRoleEventArgs ev) => _states.TryRemove(ev.Player.PlayerId, out _);

        public void On106ChangedStalkMode(Scp106ChangedStalkModeEventArgs ev)
        {
            var state = GetState(ev.Player);
            if (ev.IsStalkActive)
                state.SetState(Scp106VoiceState.Stalking);
            else
                state.SetState(Scp106VoiceState.Emerging, timeoutSeconds: 3.5f); // Smooth emerging window
        }

        public void On106ChangedVigor(Scp106ChangedVigorEventArgs ev)
        {
            var state = GetState(ev.Player);
            if (ev.Value < 20f) state.TrySetLowPriorityState(Scp106VoiceState.LowVigor);
            else state.ClearLowVigor();
        }

        public void On106TeleportingPlayer(Scp106TeleportingPlayerEvent ev)
        {
            GetState(ev.Player).SetState(Scp106VoiceState.PocketDimension, timeoutSeconds: 5.0f);
        }

        public void On106UsingHunterAtlas(Scp106UsingHunterAtlasEventArgs ev)
        {
            var state = GetState(ev.Player);
            if (ev.IsAllowed) state.TrySetMediumPriorityState(Scp106VoiceState.AtlasDimensional);
            else if (state.CurrentState == Scp106VoiceState.AtlasDimensional) state.ResetToIdle();
        }
    }
}