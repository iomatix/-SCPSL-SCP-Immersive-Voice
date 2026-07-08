using LabApi.Events.Arguments.Scp939Events;
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
    /// Routes biomorphic camouflage abilities and combat triggers for SCP-939 to the generic dynamic voice state manager.
    /// Utilizes automated fallback watchdogs to prevent transitional stuck voice states.
    /// </summary>
    public class Scp939AudioHandler : IScpAudioSubsystem
    {
        #region Public Operational Properties
        /// <summary>
        /// Gets the centralized thread-safe state manager for SCP-939.
        /// </summary>
        public DynamicStateManager<Scp939VoiceState> Manager { get; }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Scp939AudioHandler"/> class.
        /// </summary>
        public Scp939AudioHandler()
        {
            // Target-typed clean constructor instantiation pattern
            Manager = new(
                RoleTypeId.Scp939,
                Scp939VoiceState.IdleWhisper,
                Scp939DynamicPresets.GetPresetForState
            );
        }
        #endregion

        #region Camouflage & Mimicry Listeners
        /// <summary>
        /// Triggers voice spectrum morphing when the predator begins mimicking environmental audio cues.
        /// </summary>
        public void On939MimickingEnvironment(Scp939MimickingEnvironmentEventArgs ev)
        {
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp939VoiceState.Mimicking);
        }

        /// <summary>
        /// Resets the audio profile immediately back to the standard baseline once mimicry execution ceases.
        /// </summary>
        public void On939MimickedEnvironment(Scp939MimickedEnvironmentEventArgs ev)
        {
            if (ev is not null && ev.Player is not null)
                Manager.ResetToDefault(ev.Player);
        }

        /// <summary>
        /// Manages state adjustments when entering or leaving the focused predator stealth overlay.
        /// </summary>
        public void On939Focused(Scp939FocusedEventArgs ev)
        {
            if (ev is null || ev.Player is null) return;

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
        #endregion

        #region Combat & Lunge Mechanics Listeners
        /// <summary>
        /// Injects high-frequency aggressive vocal filters when primary claw combat is active.
        /// </summary>
        public void On939Attacking(Scp939AttackingEventArgs ev)
        {
            // Protects the combat stream with an absolute 3.0-second safety watchdog threshold
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp939VoiceState.Attacking, 3.0f);
        }

        /// <summary>
        /// Applies defensive reactive voice distortions when taking damage during engagement.
        /// </summary>
        public void On939Attacked(Scp939AttackedEventArgs ev)
        {
            // Protects the combat stream with an absolute 1.5-second safety watchdog threshold
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp939VoiceState.Attacking, 1.5f);
        }

        /// <summary>
        /// Forces short-term physical exertion presets when launching the kinetic leap ability.
        /// </summary>
        public void On939Lunging(Scp939LungingEventArgs ev)
        {
            // Sets a strict 3.5-second lunge animation window before auto-expiring back to idle
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp939VoiceState.Attacking, 3.5f);
        }

        /// <summary>
        /// Discharges lunge vocal layers upon completion of the pounce sequence.
        /// </summary>
        public void On939Lunged(Scp939LungedEventArgs ev)
        {
            if (ev is not null && ev.Player is not null)
                Manager.ResetToDefault(ev.Player);
        }
        #endregion

        #region Chemical Dispersion Listeners
        /// <summary>
        /// Routes voice modifications when releasing amnestic clouds into local facility zones.
        /// </summary>
        public void On939CreatingAmnesticCloud(Scp939CreatingAmnesticCloudEventArgs ev)
        {
            // Protects the chemical dispersion stream with a 25.0-second absolute timer watchdog gate
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp939VoiceState.AmnesticCloud, 25.0f);
        }

        /// <summary>
        /// Extends the active environmental state duration while the chemical aerosol field remains saturated.
        /// </summary>
        public void On939CreatedAmnesticCloud(Scp939CreatedAmnesticCloudEventArgs ev)
        {
            if (ev is not null && ev.Player is not null)
                Manager.SetState(ev.Player, Scp939VoiceState.AmnesticCloud, 60.0f);
        }
        #endregion

        #region Binding
        public void BindPipelines()
        {
            Scp939Events.Attacking += On939Attacking;
            Scp939Events.Attacked += On939Attacked;
            Scp939Events.CreatingAmnesticCloud += On939CreatingAmnesticCloud;
            Scp939Events.CreatedAmnesticCloud += On939CreatedAmnesticCloud;
            Scp939Events.Focused += On939Focused;
            Scp939Events.Lunged += On939Lunged;
            Scp939Events.Lunging += On939Lunging;
            Scp939Events.MimickingEnvironment += On939MimickingEnvironment;
            Scp939Events.MimickedEnvironment += On939MimickedEnvironment;

            ScpVoiceProfiles.DynamicProviders.Enqueue(Manager);
        }

        public void UnbindPipelines()
        {
            Scp939Events.Attacking -= On939Attacking;
            Scp939Events.Attacked -= On939Attacked;
            Scp939Events.CreatingAmnesticCloud -= On939CreatingAmnesticCloud;
            Scp939Events.CreatedAmnesticCloud -= On939CreatedAmnesticCloud;
            Scp939Events.Focused -= On939Focused;
            Scp939Events.Lunged -= On939Lunged;
            Scp939Events.Lunging -= On939Lunging;
            Scp939Events.MimickingEnvironment -= On939MimickingEnvironment;
            Scp939Events.MimickedEnvironment -= On939MimickedEnvironment;
        }

        public void PurgePlayer(Player player)
        {
            if (player is null) return;
            Manager?.RemovePlayer(player);
        }
        #endregion
    }
}