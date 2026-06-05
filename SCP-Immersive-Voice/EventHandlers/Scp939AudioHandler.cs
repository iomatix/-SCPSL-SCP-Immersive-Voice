namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp939Events;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets.Dynamics;
    using SCP_Immersive_Voice.Presets.Dynamics.Core;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Manages biomorphic camouflage abilities and combat triggers for SCP-939, routing state evaluations to the core manager.
    /// </summary>
    public class Scp939AudioHandler
    {
        /// <summary>
        /// Gets the centralized thread-safe state manager for SCP-939.
        /// </summary>
        public DynamicStateManager<Scp939VoiceState> Manager { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scp939AudioHandler"/> class.
        /// </summary>
        public Scp939AudioHandler()
        {
            Manager = new DynamicStateManager<Scp939VoiceState>(
                RoleTypeId.Scp939,
                Scp939VoiceState.IdleWhisper,
                Scp939DynamicPresets.GetPresetForState
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

        public void On939MimickingEnvironment(Scp939MimickingEnvironmentEventArgs ev)
        {
            if (ev != null) Manager.SetState(ev.Player, Scp939VoiceState.Mimicking);
        }

        public void On939MimickedEnvironment(Scp939MimickedEnvironmentEventArgs ev)
        {
            if (ev != null) Manager.ResetToDefault(ev.Player);
        }

        public void On939Focused(Scp939FocusedEventArgs ev)
        {
            if (ev == null || ev.Player == null) return;

            if (ev.FocusState)
            {
                // Sharpens the directional whisper matrix structure
                Manager.SetState(ev.Player, Scp939VoiceState.Focused);
            }
            else
            {
                // Releases predator vision and rolls back to baseline audio camouflage
                Manager.ResetToDefault(ev.Player);
            }
        }

        public void On939Attacking(Scp939AttackingEventArgs ev)
        {
            // Forces raw throat exposure with a 3-second mechanical safety limit
            if (ev != null) Manager.SetState(ev.Player, Scp939VoiceState.Attacking, 3.0f);
        }

        public void On939Attacked(Scp939AttackedEventArgs ev)
        {
            if (ev != null) Manager.ResetToDefault(ev.Player);
        }

        public void On939Lunging(Scp939LungingEventArgs ev)
        {
            // High-velocity strike override bound by a 2.5-second processing watchdog window
            if (ev != null) Manager.SetState(ev.Player, Scp939VoiceState.Attacking, 2.5f);
        }

        public void On939Lunged(Scp939LungedEventArgs ev)
        {
            if (ev != null) Manager.ResetToDefault(ev.Player);
        }

        public void On939CreatingAmnesticCloud(Scp939CreatingAmnesticCloudEventArgs ev)
        {
            // Introduces heavy environmental chemical dampening for a strict 15-second block
            if (ev != null) Manager.SetState(ev.Player, Scp939VoiceState.AmnesticCloud, 15.0f);
        }

        public void On939CreatedAmnesticCloud(Scp939CreatedAmnesticCloudEventArgs ev)
        {
            if (ev != null) Manager.ResetToDefault(ev.Player);
        }
    }
}