using LabApi.Events.Arguments.Scp106Events;
using PlayerRoles;
using SCP_Immersive_Voice.Presets.Dynamics;
using SCP_Immersive_Voice.Presets.Dynamics.Core;
using SCP_Immersive_Voice.Presets.Dynamics.Enums;

namespace ScpImmersiveVoice.EventHandlers
{
    /// <summary>
    /// Processes conditional parameters and active ability flags for SCP-106, translating them to localized voice states.
    /// </summary>
    public class Scp106AudioHandler
    {
        #region Public Operational Properties
        /// <summary>
        /// Gets the centralized thread-safe state manager for SCP-106.
        /// </summary>
        public DynamicStateManager<Scp106VoiceState> Manager { get; }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Scp106AudioHandler"/> class.
        /// </summary>
        public Scp106AudioHandler()
        {
            // Target-typed clean constructor instantiation pattern
            Manager = new(
                RoleTypeId.Scp106,
                Scp106VoiceState.Idle,
                Scp106DynamicPresets.GetPresetForState
            );
        }
        #endregion

        // USUNIĘTO: Metody OnPlayerDied oraz OnChangedRole zostały wycięte.
        // Czyszczenie sesji i pamięci podręcznej realizuje teraz potok ImmersiveScpVoicePlugin.PurgePlayerContext.

        #region Extradimensional State Mechanics listeners
        /// <summary>
        /// Intercepts SCP-106 stalk capability triggers to toggle low-pass mud dampening layers.
        /// </summary>
        public void On106ChangedStalkMode(Scp106ChangedStalkModeEventArgs ev)
        {
            if (ev is null || ev.Player is null) return;

            if (ev.IsStalkActive)
            {
                // Submerged state: heavy low-pass mud dampening
                Manager.SetState(ev.Player, Scp106VoiceState.Stalking);
            }
            else
            {
                // Emerging state: wet flesh rupture modeling with a 3.5-second watchdog fallback window
                Manager.SetState(ev.Player, Scp106VoiceState.Emerging, 3.5f);
            }
        }

        /// <summary>
        /// Evaluates stamina/vigor metrics to inject fatigued respiratory feedback matrices.
        /// </summary>
        public void On106ChangedVigor(Scp106ChangedVigorEventArgs ev)
        {
            if (ev is null || ev.Player is null) return;

            // 1. Priority Guard: If the player is actively engaged in an ability, bypass breathing adjustments.
            Scp106VoiceState currentState = Manager.GetCurrentState(ev.Player);

            // PERFORMANCE UPGRADE: Swapped nested conditional logic chains for a pristine C# 9.0 pattern match evaluation loop
            if (currentState is Scp106VoiceState.Stalking
                             or Scp106VoiceState.Emerging
                             or Scp106VoiceState.PocketDimension
                             or Scp106VoiceState.AtlasDimensional)
            {
                return;
            }

            // 2. Simple Dynamics: Enforce exhausted layout if vigor resources breach the 15% threshold floor
            if (ev.Value <= 15f)
            {
                Manager.SetState(ev.Player, Scp106VoiceState.LowVigor);
            }
            else if (ev.Value >= 30f)
            {
                if (currentState is Scp106VoiceState.LowVigor)
                {
                    Manager.ResetToDefault(ev.Player);
                }
            }
        }

        /// <summary>
        /// Intercepts pocket dimension transition capture loops to force phase dislocation filters.
        /// </summary>
        public void On106TeleportingPlayer(Scp106TeleportingPlayerEvent ev)
        {
            // Forces extradimensional phase dislocation for 6.35 seconds during pocket transfers
            if (ev is not null && ev.Player is not null)
            {
                Manager.SetState(ev.Player, Scp106VoiceState.PocketDimension, 6.35f);
            }
        }

        /// <summary>
        /// Manages dimensional bleed filters when traversing the Hunter's Atlas overlay mesh.
        /// </summary>
        public void On106UsingHunterAtlas(Scp106UsingHunterAtlasEventArgs ev)
        {
            if (ev is null || ev.Player is null) return;

            if (ev.IsAllowed)
            {
                // WATCHDOG CONFIGURATION: Instead of an infinite tracking lock, the dimensional bleed 
                // is restricted to a 4.2-second window. After it cascades, the watchdog resets the state cleanly.
                Manager.SetState(ev.Player, Scp106VoiceState.AtlasDimensional, 4.2f);
            }
            else
            {
                Manager.ResetToDefault(ev.Player);
            }
        }
        #endregion
    }
}