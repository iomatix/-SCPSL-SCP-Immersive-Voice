using LabApi.Events.Arguments.Scp096Events;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp096;
using SCP_Immersive_Voice.Presets.Dynamics;
using SCP_Immersive_Voice.Presets.Dynamics.Core;
using SCP_Immersive_Voice.Presets.Dynamics.Enums;

namespace ScpImmersiveVoice.EventHandlers
{
    /// <summary>
    /// Handles discrete game state transitions for SCP-096 and routes them to the generic dynamic voice state manager.
    /// Utilizing the unified core state engine to prevent race conditions.
    /// </summary>
    public class Scp096AudioHandler
    {
        #region Public Operational Properties
        /// <summary>
        /// Gets the centralized thread-safe state manager for SCP-096.
        /// </summary>
        public DynamicStateManager<Scp096VoiceState> Manager { get; }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Scp096AudioHandler"/> class.
        /// </summary>
        public Scp096AudioHandler()
        {
            // Target-typed clean constructor instantiation pattern
            Manager = new(
                RoleTypeId.Scp096,
                Scp096VoiceState.Calm,
                Scp096DynamicPresets.GetPresetForState
            );
        }
        #endregion

        #region Master State Engine Receptor
        /// <summary>
        /// Intercepts native SCP-096 rage lifecycle updates and maps them to floating-native DSP voice states.
        /// </summary>
        public void On096ChangedState(Scp096ChangedStateEventArgs ev)
        {
            if (ev is null || ev.Player is null) return;

            switch (ev.State)
            {
                case Scp096RageState.Docile:
                    // Fully calmed down and normalized, return straight to trembling Calm baseline
                    Manager.ResetToDefault(ev.Player);
                    break;

                case Scp096RageState.Distressed:
                    // Triggered! Instantly shifts into the pathetic, heavy sobbing layout
                    Manager.SetState(ev.Player, Scp096VoiceState.Crying);
                    break;

                case Scp096RageState.Enraged:
                    // Active screaming rampage mode, triggers the devastating demonic dual-tone engine
                    Manager.SetState(ev.Player, Scp096VoiceState.Enraged);
                    break;

                case Scp096RageState.Calming:
                    // Rage timeout cooldown. Force the choked throat "TryingNotToCry" layout for 5.5 seconds
                    Manager.SetState(ev.Player, Scp096VoiceState.TryingNotToCry, 5.5f);
                    break;
            }
        }
        #endregion

        #region Tactical Gameplay Overlays
        public void On096Charging(Scp096ChargingEventArgs ev)
        {
            // Inject bull-rush sprint pressure overlay (automatically expires after 5 seconds)
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp096VoiceState.Charging, 5.0f);
        }

        public void On096Charged(Scp096ChargedEventArgs ev)
        {
            // When the sprint lunge finishes, drop back to the active master rage state
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp096VoiceState.Enraged);
        }

        public void On096PryingGate(Scp096PryingGateEventArgs ev)
        {
            // Severe kinetic door-crushing grunt layout injection
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp096VoiceState.PryingGate);
        }

        public void On096PriedGate(Scp096PriedGateEventArgs ev)
        {
            // Recover from door damage execution back into standard rage
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp096VoiceState.Enraged);
        }
        #endregion
    }
}