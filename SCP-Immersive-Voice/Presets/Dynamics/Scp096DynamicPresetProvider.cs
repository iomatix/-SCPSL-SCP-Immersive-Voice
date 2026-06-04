namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using LabApi.Features.Wrappers;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Interfaces;
    using System.Collections.Generic;
    public class Scp096DynamicPresetProvider : IDynamicVoicePresetProvider
    {

        /// <summary>
        /// The key of this dictionary is UserId.
        /// </summary>
        private readonly Dictionary<string, Scp096VoiceStateController> _states;

        public Scp096DynamicPresetProvider(Dictionary<string, Scp096VoiceStateController> states)
        {
            _states = states;
        }

        public bool TryGetDynamicPreset(Player player, out ScpVoicePreset preset)
        {
            preset = null;

            if (player.Role != RoleTypeId.Scp096)
                return false;

            if (!_states.TryGetValue(player.UserId, out var controller))
                return false;

            preset = Scp096DynamicPresets.GetPresetForState(controller.CurrentState);
            return true;
        }
    }

}