namespace SCP_Immersive_Voice.Presets.Dynamics.Providers
{
    using LabApi.Features.Wrappers;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Interfaces;
    using System.Collections.Generic;
    public class Scp096DynamicPresetProvider : IDynamicVoicePresetProvider
    {

        /// <summary>
        /// The key of this dictionary is PlayerId.
        /// </summary>
        private readonly Dictionary<int, Scp096VoiceStateController> _states;

        public Scp096DynamicPresetProvider(Dictionary<int, Scp096VoiceStateController> states)
        {
            _states = states;
        }

        public bool TryGetDynamicPreset(Player player, out ScpVoicePreset preset)
        {
            preset = null;

            if (player.Role != RoleTypeId.Scp096)
                return false;

            if (!_states.TryGetValue(player.PlayerId, out var controller))
                return false;

            preset = Scp096DynamicPresets.GetPresetForState(controller.CurrentState);
            return true;
        }
    }

}