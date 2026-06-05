namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp939Events;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics;
    using SCP_Immersive_Voice.Presets.Dynamics.Core;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Routes biomorphic camouflage abilities and combat triggers for SCP-939 to the generic dynamic voice state manager.
    /// Utilizes automated fallback watchdogs to prevent transitional stuck voice states.
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
                // Transition into focused predator stealth whisper
                Manager.SetState(ev.Player, Scp939VoiceState.Focused);
            }
            else
            {
                // Fallback cleanly to standard idle voice camouflage
                Manager.ResetToDefault(ev.Player);
            }
        }

        public void On939Attacking(Scp939AttackingEventArgs ev)
        {
            // Protects the combat stream with an absolute 3-second safety watchdog threshold
            if (ev != null) Manager.SetState(ev.Player, Scp939VoiceState.Attacking, 3.0f);
        }

        public void On939Attacked(Scp939AttackedEventArgs ev)
        {
            // Protects the combat stream with an absolute 1.5-second safety watchdog threshold
            if (ev != null) Manager.SetState(ev.Player, Scp939VoiceState.Attacking, 1.5f);
        }

        public void On939Lunging(Scp939LungingEventArgs ev)
        {
            // Sets a strict 3.5-second lunge animation window before auto-expiring back to idle
            if (ev != null) Manager.SetState(ev.Player, Scp939VoiceState.Attacking, 3.5f);
        }

        public void On939Lunged(Scp939LungedEventArgs ev)
        {
            if (ev != null) Manager.ResetToDefault(ev.Player);
        }

        public void On939CreatingAmnesticCloud(Scp939CreatingAmnesticCloudEventArgs ev)
        {
            // Protects the chemical dispersion stream with a 15-second absolute timer limits
            if (ev != null) Manager.SetState(ev.Player, Scp939VoiceState.AmnesticCloud, 15.0f);
        }

        public void On939CreatedAmnesticCloud(Scp939CreatedAmnesticCloudEventArgs ev)
        {
            if (ev != null) Manager.SetState(ev.Player, Scp939VoiceState.AmnesticCloud, 60.0f);
        }
    }
}