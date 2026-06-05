namespace ScpImmersiveVoice.EventHandlers
{
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.Scp106Events;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets.Dynamics;
    using SCP_Immersive_Voice.Presets.Dynamics.Core;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Processes conditional parameters and active ability flags for SCP-106, translating them to localized voice states.
    /// </summary>
    public class Scp106AudioHandler
    {
        /// <summary>
        /// Gets the centralized thread-safe state manager for SCP-106.
        /// </summary>
        public DynamicStateManager<Scp106VoiceState> Manager { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scp106AudioHandler"/> class.
        /// </summary>
        public Scp106AudioHandler()
        {
            Manager = new DynamicStateManager<Scp106VoiceState>(
                RoleTypeId.Scp106,
                Scp106VoiceState.Idle,
                Scp106DynamicPresets.GetPresetForState
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

        public void On106ChangedStalkMode(Scp106ChangedStalkModeEventArgs ev)
        {
            if (ev == null || ev.Player == null) return;

            if (ev.IsStalkActive)
            {
                // Submerged state: heavy low-pass mud dampening
                Manager.SetState(ev.Player, Scp106VoiceState.Stalking);
            }
            else
            {
                // Emerging state: wet flesh rupture modeling with a 3.5-second fallback window
                Manager.SetState(ev.Player, Scp106VoiceState.Emerging, 3.5f);
            }
        }

        public void On106ChangedVigor(Scp106ChangedVigorEventArgs ev)
        {
            if (ev == null || ev.Player == null) return;

            // Evaluates low-vigor exhaustion fatigue limits dynamically
            if (ev.Value < 20f)
            {
                Manager.SetState(ev.Player, Scp106VoiceState.LowVigor);
            }
            else
            {
                // Reverts back to baseline if the exhaustion threshold is recovered
                Manager.ResetToDefault(ev.Player);
            }
        }

        public void On106TeleportingPlayer(Scp106TeleportingPlayerEvent ev)
        {
            // Forces extradimensional phase dislocation for 5 seconds during pocket transfers
            if (ev != null && ev.Player != null)
            {
                Manager.SetState(ev.Player, Scp106VoiceState.PocketDimension, 5.0f);
            }
        }

        public void On106UsingHunterAtlas(Scp106UsingHunterAtlasEventArgs ev)
        {
            if (ev == null || ev.Player == null) return;

            if (ev.IsAllowed)
            {
                // Seamlessly injects the structural room-sensing dimensional bleed
                Manager.SetState(ev.Player, Scp106VoiceState.AtlasDimensional);
            }
            else
            {
                // Gracefully closes the atlas map overlap profile
                Manager.ResetToDefault(ev.Player);
            }
        }
    }
}