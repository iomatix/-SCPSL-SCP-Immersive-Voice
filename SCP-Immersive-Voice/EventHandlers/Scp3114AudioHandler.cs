using LabApi.Events.Arguments.Scp3114Events;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using SCP_Immersive_Voice.Presets.Dynamics;
using SCP_Immersive_Voice.Presets.Dynamics.Core;
using SCP_Immersive_Voice.Presets.Dynamics.Enums;
using SCP_Immersive_Voice.VoiceProfiles;

namespace ScpImmersiveVoice.EventHandlers
{
    /// <summary>
    /// Routes gameplay hooks and structural state updates for SCP-3114 to the generic dynamic voice state manager.
    /// </summary>
    public class Scp3114AudioHandler : IScpAudioSubsystem
    {
        #region Public Operational Properties
        /// <summary>
        /// Gets the centralized thread-safe state manager for SCP-3114.
        /// </summary>
        public DynamicStateManager<Scp3114VoiceState> Manager { get; }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Scp3114AudioHandler"/> class.
        /// </summary>
        public Scp3114AudioHandler()
        {
            // Target-typed clean constructor instantiation pattern
            Manager = new(
                RoleTypeId.Scp3114,
                Scp3114VoiceState.Undisguised,
                Scp3114DynamicPresets.GetPresetForState
            );
        }
        #endregion

        #region Disguise Lifecycle Listeners
        /// <summary>
        /// Intercepts disguise initiation hooks to inject progressive flesh-weaving audio presets.
        /// </summary>
        public void On3114Disguising(Scp3114DisguisingEventArgs ev)
        {
            // Protects the pipeline with a 6-second dynamic flesh-weaving watchdog window
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp3114VoiceState.Disguising, 6.0f);
        }

        /// <summary>
        /// Commits full role replication spectrum parameters once the disguise is successfully assumed.
        /// </summary>
        public void On3114Disguised(Scp3114DisguisedEventArgs ev)
        {
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp3114VoiceState.Disguised);
        }

        /// <summary>
        /// Handles the structural skin-tearing transition phase when dropping a disguise identity.
        /// </summary>
        public void On3114Revealing(Scp3114RevealingEventArgs ev)
        {
            // Protects the pipeline with a 3.5-second structural tearing watchdog window
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp3114VoiceState.Revealing, 3.5f);
        }

        /// <summary>
        /// Resets the voice matrix profile back to the standard exposed bone baseline.
        /// </summary>
        public void On3114Revealed(Scp3114RevealedEventArgs ev)
        {
            if (ev is not null && ev.Player is not null)
                Manager.ResetToDefault(ev.Player);
        }
        #endregion

        #region Combat & Strangle Mechanics Listeners
        public void On3114StrangleStarting(Scp3114StrangleStartingEventArgs ev)
        {
            // Sets execution state with a 10-second absolute chokehold lifespan limit
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp3114VoiceState.Strangling, 10.0f);
        }

        public void On3114StrangleStarted(Scp3114StrangleStartedEventArgs ev)
        {
            // Sets execution state with a 10-second absolute chokehold lifespan limit
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp3114VoiceState.Strangling, 10.0f);
        }

        public void On3114StrangleAborting(Scp3114StrangleAbortingEventArgs ev)
        {
            if (ev is not null && ev.Player is not null)
                Manager.ResetToDefault(ev.Player);
        }

        public void On3114StrangleAborted(Scp3114StrangleAbortedEventArgs ev)
        {
            if (ev is not null && ev.Player is not null)
                Manager.ResetToDefault(ev.Player);
        }
        #endregion

        #region Dance & Movement Listeners
        public void On3114DanceStarting(Scp3114StartingDanceEventArgs ev)
        {
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp3114VoiceState.Dancing);
        }

        public void On3114Dance(Scp3114StartedDanceEventArgs ev)
        {
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp3114VoiceState.Dancing);
        }
        #endregion

        #region Binding
        public void BindPipelines()
        {
            Scp3114Events.Disguising += On3114Disguising;
            Scp3114Events.Disguised += On3114Disguised;
            Scp3114Events.Revealing += On3114Revealing;
            Scp3114Events.Revealed += On3114Revealed;
            Scp3114Events.StrangleAborted += On3114StrangleAborted;
            Scp3114Events.StrangleAborting += On3114StrangleAborting;
            Scp3114Events.StrangleStarted += On3114StrangleStarted;
            Scp3114Events.StrangleStarting += On3114StrangleStarting;
            Scp3114Events.StartDancing += On3114DanceStarting;
            Scp3114Events.Dance += On3114Dance;


            ScpVoiceProfiles.DynamicProviders.Enqueue(Manager);
        }

        public void UnbindPipelines()
        {
            Scp3114Events.Disguising -= On3114Disguising;
            Scp3114Events.Disguised -= On3114Disguised;
            Scp3114Events.Revealing -= On3114Revealing;
            Scp3114Events.Revealed -= On3114Revealed;
            Scp3114Events.StrangleAborted -= On3114StrangleAborted;
            Scp3114Events.StrangleAborting -= On3114StrangleAborting;
            Scp3114Events.StrangleStarted -= On3114StrangleStarted;
            Scp3114Events.StrangleStarting -= On3114StrangleStarting;
            Scp3114Events.StartDancing -= On3114DanceStarting;
            Scp3114Events.Dance -= On3114Dance;
        }

        public void PurgePlayer(Player player)
        {
            if (player is null) return;
            Manager?.RemovePlayer(player);
        }
        #endregion
    }
}