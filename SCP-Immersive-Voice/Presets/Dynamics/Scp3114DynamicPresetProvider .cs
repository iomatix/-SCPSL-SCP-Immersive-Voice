namespace SCP_Immersive_Voice.Presets.Dynamics
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
        /// The key of this dictionary is UserId.
        /// </summary>
        private readonly Dictionary<string, Scp3114VoiceStateController> _states;

        public Scp3114DynamicPresetProvider(Dictionary<string, Scp3114VoiceStateController> states)
        {
            _states = states;
        }

        public bool TryGetDynamicPreset(Player player, out ScpVoicePreset preset)
        {
            preset = null;

            if (player.Role != RoleTypeId.Scp3114)
                return false;

            if (!_states.TryGetValue(player.UserId, out var controller))
                return false;

            preset = Scp3114DynamicPresets.GetPresetForState(controller.CurrentState);
            return true;
        }
    }
}
