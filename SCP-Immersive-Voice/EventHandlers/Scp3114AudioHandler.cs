namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp3114Events;
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;
    using System.Collections.Concurrent;

    public class Scp3114AudioHandler
    {
        private readonly ConcurrentDictionary<int, Scp3114VoiceStateController> _states = new ConcurrentDictionary<int, Scp3114VoiceStateController>();

        private Scp3114VoiceStateController GetState(Player player) => _states.GetOrAdd(player.PlayerId, _ => new Scp3114VoiceStateController());

        public void OnPlayerDied(PlayerDeathEventArgs ev) => _states.TryRemove(ev.Player.PlayerId, out _);
        public void OnChangingRole(PlayerChangingRoleEventArgs ev) => _states.TryRemove(ev.Player.PlayerId, out _);

        public void On3114Disguising(Scp3114DisguisingEventArgs ev) => GetState(ev.Player).SetState(Scp3114VoiceState.Disguising, 5.0f);
        public void On3114Disguised(Scp3114DisguisedEventArgs ev) => GetState(ev.Player).SetState(Scp3114VoiceState.Disguised);
        public void On3114Revealing(Scp3114RevealingEventArgs ev) => GetState(ev.Player).SetState(Scp3114VoiceState.Revealing, 3.0f);
        public void On3114Revealed(Scp3114RevealedEventArgs ev) => GetState(ev.Player).ResetToIdle();
        public void On3114StrangleStarting(Scp3114StrangleStartingEventArgs ev) => GetState(ev.Player).SetState(Scp3114VoiceState.Strangling, 10.0f);
        public void On3114StrangleStarted(Scp3114StrangleStartedEventArgs ev) => GetState(ev.Player).SetState(Scp3114VoiceState.Strangling, 10.0f);
        public void On3114StrangleAborting(Scp3114StrangleAbortingEventArgs ev) => GetState(ev.Player).ResetToIdle();
        public void On3114StrangleAborted(Scp3114StrangleAbortedEventArgs ev) => GetState(ev.Player).ResetToIdle();
    }
}