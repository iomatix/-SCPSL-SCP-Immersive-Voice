namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp096Events;
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;
    using System.Collections.Concurrent;

    public class Scp096AudioHandler
    {
        private readonly ConcurrentDictionary<int, Scp096VoiceStateController> _states = new ConcurrentDictionary<int, Scp096VoiceStateController>();

        private Scp096VoiceStateController GetState(Player player) => _states.GetOrAdd(player.PlayerId, _ => new Scp096VoiceStateController());

        public void OnPlayerDied(PlayerDeathEventArgs ev) => _states.TryRemove(ev.Player.PlayerId, out _);
        public void OnChangingRole(PlayerChangingRoleEventArgs ev) => _states.TryRemove(ev.Player.PlayerId, out _);

        public void On096StartingCrying(Scp096StartCryingEventArgs ev) => GetState(ev.Player).SetState(Scp096VoiceState.Crying);
        public void On096StartedCrying(Scp096StartedCryingEventArgs ev) => GetState(ev.Player).SetState(Scp096VoiceState.Crying);
        public void On096Enraging(Scp096EnragingEventArgs ev) => GetState(ev.Player).SetState(Scp096VoiceState.Enraging, 6f);
        public void On096Enraged(Scp096EnragedEventArgs ev) => GetState(ev.Player).SetState(Scp096VoiceState.Enraged);
        public void On096TryingNotToCry(Scp096TryingNotToCryEventArgs ev) => GetState(ev.Player).SetState(Scp096VoiceState.TryingNotToCry);
        public void On096Charging(Scp096ChargingEventArgs ev) => GetState(ev.Player).SetState(Scp096VoiceState.Charging, 4f);
        public void On096Charged(Scp096ChargedEventArgs ev) => GetState(ev.Player).ResetToIdle();
    }
}