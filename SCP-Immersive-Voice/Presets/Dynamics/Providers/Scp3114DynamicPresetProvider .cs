namespace SCP_Immersive_Voice.Presets.Dynamics.Providers
{
    using LabApi.Features.Wrappers;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Interfaces;
    using System.Collections.Generic;

    public class Scp3114DynamicPresetProvider : IDynamicVoicePresetProvider
    {
        /// <summary>
        /// The key of this dictionary is PlayerId.
        /// </summary>
        private readonly Dictionary<int, Scp3114VoiceStateController> _states;

        public Scp3114DynamicPresetProvider(Dictionary<int, Scp3114VoiceStateController> states)
        {
            _states = states;
        }

        public bool TryGetDynamicPreset(Player player, out ScpVoicePreset preset)
        {
            preset = null;

            if (player.Role != RoleTypeId.Scp3114)
                return false;

            if (!_states.TryGetValue(player.PlayerId, out var controller))
                return false;

            preset = Scp3114DynamicPresets.GetPresetForState(controller.CurrentState);
            return true;
        }
    }
}
