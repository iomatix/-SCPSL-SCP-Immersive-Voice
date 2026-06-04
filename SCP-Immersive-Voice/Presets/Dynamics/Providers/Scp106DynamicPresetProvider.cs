namespace SCP_Immersive_Voice.Presets.Dynamics.Providers
{
    using LabApi.Features.Wrappers;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Interfaces;
    using System.Collections.Generic;
    public class Scp106DynamicPresetProvider : IDynamicVoicePresetProvider
    {

        /// <summary>
        /// The key of this dictionary is PlayerId.
        /// </summary>
        private readonly Dictionary<int, Scp106VoiceStateController> _states;

        public Scp106DynamicPresetProvider(Dictionary<int, Scp106VoiceStateController> states)
        {
            _states = states;
        }

        public bool TryGetDynamicPreset(Player player, out ScpVoicePreset preset)
        {
            preset = null;

            if (player.Role != RoleTypeId.Scp106)
                return false;

            if (!_states.TryGetValue(player.PlayerId, out var controller))
                return false;

            preset = Scp106DynamicPresets.GetPresetForState(controller.CurrentState);
            return true;
        }
    }

}