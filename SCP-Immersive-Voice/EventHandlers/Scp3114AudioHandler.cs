namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp3114Events;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics;
    using SCP_Immersive_Voice.Presets.Dynamics.Core;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Routes gameplay hooks and structural state updates for SCP-3114 to the generic dynamic voice state manager.
    /// </summary>
    public class Scp3114AudioHandler
    {
        /// <summary>
        /// Gets the centralized thread-safe state manager for SCP-3114.
        /// </summary>
        public DynamicStateManager<Scp3114VoiceState> Manager { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scp3114AudioHandler"/> class.
        /// </summary>
        public Scp3114AudioHandler()
        {
            Manager = new DynamicStateManager<Scp3114VoiceState>(
                RoleTypeId.Scp3114,
                Scp3114VoiceState.Undisguised,
                Scp3114DynamicPresets.GetPresetForState
            );
        }

        /// <summary>
        /// Session cleanup hook triggered upon player death.
        /// </summary>
        public void OnPlayerDied(PlayerDeathEventArgs ev)
        {
            if (ev != null && ev.Player != null)
            {
                Manager.RemovePlayer(ev.Player);
            }
        }

        /// <summary>
        /// Session cleanup hook triggered when a player changes their role class.
        /// </summary>
        public void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            if (ev != null && ev.Player != null)
            {
                Manager.RemovePlayer(ev.Player);
            }
        }

        public void On3114Disguising(Scp3114DisguisingEventArgs ev)
        {
            // Protects the pipeline with a 5-second dynamic flesh-weaving watchdog window
            if (ev != null) Manager.SetState(ev.Player, Scp3114VoiceState.Disguising, 5.0f);
        }

        public void On3114Disguised(Scp3114DisguisedEventArgs ev)
        {
            if (ev != null) Manager.SetState(ev.Player, Scp3114VoiceState.Disguised);
        }

        public void On3114Revealing(Scp3114RevealingEventArgs ev)
        {
            // Protects the pipeline with a 3-second structural tearing watchdog window
            if (ev != null) Manager.SetState(ev.Player, Scp3114VoiceState.Revealing, 3.0f);
        }

        public void On3114Revealed(Scp3114RevealedEventArgs ev)
        {
            if (ev != null) Manager.ResetToDefault(ev.Player);
        }

        public void On3114StrangleStarting(Scp3114StrangleStartingEventArgs ev)
        {
            // Sets execution state with a 10-second absolute chokehold lifespan limit
            if (ev != null) Manager.SetState(ev.Player, Scp3114VoiceState.Strangling, 10.0f);
        }

        public void On3114StrangleStarted(Scp3114StrangleStartedEventArgs ev)
        {
            if (ev != null) Manager.SetState(ev.Player, Scp3114VoiceState.Strangling, 10.0f);
        }

        public void On3114StrangleAborting(Scp3114StrangleAbortingEventArgs ev)
        {
            if (ev != null) Manager.ResetToDefault(ev.Player);
        }

        public void On3114StrangleAborted(Scp3114StrangleAbortedEventArgs ev)
        {
            if (ev != null) Manager.ResetToDefault(ev.Player);
        }
    }
}