namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp939Events;
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;
    using System.Collections.Concurrent;

    public class Scp939AudioHandler
    {
        // Thread-safe concurrent dictionary to expose states directly to the provider node
        private readonly ConcurrentDictionary<int, Scp939VoiceStateController> _states = new ConcurrentDictionary<int, Scp939VoiceStateController>();

        public ConcurrentDictionary<int, Scp939VoiceStateController> States => _states;

        private Scp939VoiceStateController GetState(Player player)
        {
            return _states.GetOrAdd(player.PlayerId, _ => new Scp939VoiceStateController());
        }

        // --- Session Purge Hooks ---
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

        // --- Game Core State Triggers ---
        public void On939MimickingEnvironment(Scp939MimickingEnvironmentEventArgs ev) => GetState(ev.Player).SetState(Scp939VoiceState.Mimicking);
        public void On939MimickedEnvironment(Scp939MimickedEnvironmentEventArgs ev) => GetState(ev.Player).ResetToIdle();

        public void On939Focused(Scp939FocusedEventArgs ev)
        {
            var state = GetState(ev.Player);
            if (state == null) return;

            if (ev.FocusState)
            {
                state.SetState(Scp939VoiceState.Focused);
            }
            else
            {
                state.ResetToIdle();
            }
        }

        public void On939Attacking(Scp939AttackingEventArgs ev) => GetState(ev.Player).SetState(Scp939VoiceState.Attacking, maxDurationSeconds: 3.0f);
        public void On939Attacked(Scp939AttackedEventArgs ev) => GetState(ev.Player).ResetToIdle();

        public void On939Lunging(Scp939LungingEventArgs ev) => GetState(ev.Player).SetState(Scp939VoiceState.Attacking, maxDurationSeconds: 2.5f);
        public void On939Lunged(Scp939LungedEventArgs ev) => GetState(ev.Player).ResetToIdle();

        public void On939CreatingAmnesticCloud(Scp939CreatingAmnesticCloudEventArgs ev) => GetState(ev.Player).SetState(Scp939VoiceState.AmnesticCloud, maxDurationSeconds: 15.0f);
        public void On939CreatedAmnesticCloud(Scp939CreatedAmnesticCloudEventArgs ev) => GetState(ev.Player).ResetToIdle();
    }
}