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

            // 1. Priority Guard: If the player is engaged in an action, don't interrupt their breathing.
            Scp106VoiceState currentState = Manager.GetCurrentState(ev.Player);
            if (currentState == Scp106VoiceState.Stalking ||
                currentState == Scp106VoiceState.Emerging ||
                currentState == Scp106VoiceState.PocketDimension ||
                currentState == Scp106VoiceState.AtlasDimensional)
            {
                return;
            }

            // 2. Simple Dynamics: If the player is below 15% vigor, they are in a low-vigor state.
            if (ev.Value <= 15f)
            {
                Manager.SetState(ev.Player, Scp106VoiceState.LowVigor);
            }
            else if (ev.Value >= 30f)
            {
                if (currentState == Scp106VoiceState.LowVigor)
                {
                    Manager.ResetToDefault(ev.Player);
                }
            }
        }

        public void On106TeleportingPlayer(Scp106TeleportingPlayerEvent ev)
        {
            // Forces extradimensional phase dislocation for 6.35 seconds during pocket transfers
            if (ev != null && ev.Player != null)
            {
                Manager.SetState(ev.Player, Scp106VoiceState.PocketDimension, 6.35f);
            }
        }

        public void On106UsingHunterAtlas(Scp106UsingHunterAtlasEventArgs ev)
        {
            if (ev == null || ev.Player == null) return;

            if (ev.IsAllowed)
            {
                // WATCHDOG FIX: Instead of lifetime 0 (infinite), we hard-lock the dimensional bleed 
                // to 4.2 seconds. After this window cascades, the watchdog auto-reverts the player back to Idle cleanly.
                Manager.SetState(ev.Player, Scp106VoiceState.AtlasDimensional, 4.2f);
            }
            else
            {
                Manager.ResetToDefault(ev.Player);
            }
        }
    }
}