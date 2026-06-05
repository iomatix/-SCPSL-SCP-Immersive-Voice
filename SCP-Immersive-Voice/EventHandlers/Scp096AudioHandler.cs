namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp096Events;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets.Dynamics;
    using SCP_Immersive_Voice.Presets.Dynamics.Core;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Handles discrete game state transitions for SCP-096 and routes them to the generic dynamic voice state manager.
    /// </summary>
    public class Scp096AudioHandler
    {
        /// <summary>
        /// Gets the centralized thread-safe state manager for SCP-096.
        /// </summary>
        public DynamicStateManager<Scp096VoiceState> Manager { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scp096AudioHandler"/> class.
        /// </summary>
        public Scp096AudioHandler()
        {
            Manager = new DynamicStateManager<Scp096VoiceState>(
                RoleTypeId.Scp096,
                Scp096VoiceState.Calm,
                Scp096DynamicPresets.GetPresetForState
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

        public void On096StartingCrying(Scp096StartCryingEventArgs ev)
        {
            if (ev != null) Manager.SetState(ev.Player, Scp096VoiceState.Crying);
        }

        public void On096StartedCrying(Scp096StartedCryingEventArgs ev)
        {
            if (ev != null) Manager.SetState(ev.Player, Scp096VoiceState.Crying);
        }

        public void On096Enraging(Scp096EnragingEventArgs ev)
        {
            // Protects the pipeline with a 6-second transient safety watchdog window
            if (ev != null) Manager.SetState(ev.Player, Scp096VoiceState.Enraging, 6.0f);
        }

        public void On096Enraged(Scp096EnragedEventArgs ev)
        {
            if (ev != null) Manager.SetState(ev.Player, Scp096VoiceState.Enraged);
        }

        public void On096TryingNotToCry(Scp096TryingNotToCryEventArgs ev)
        {
            if (ev != null) Manager.SetState(ev.Player, Scp096VoiceState.TryingNotToCry);
        }

        public void On096Charging(Scp096ChargingEventArgs ev)
        {
            // Protects the pipeline with a 4-second structural lunge duration window
            if (ev != null) Manager.SetState(ev.Player, Scp096VoiceState.Charging, 4.0f);
        }

        public void On096Charged(Scp096ChargedEventArgs ev)
        {
            // Seamlessly loops the audio profile back into the calm tracking baseline
            if (ev != null) Manager.ResetToDefault(ev.Player);
        }
    }
}