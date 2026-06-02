namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using LabApi.Features.Wrappers;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Interfaces;
    using System.Collections.Generic;
    public class Scp939DynamicPresetProvider : IDynamicVoicePresetProvider
    {
        private readonly Dictionary<Player, Scp939VoiceStateController> _states;

        public Scp939DynamicPresetProvider(Dictionary<Player, Scp939VoiceStateController> states)
        {
            _states = states;
        }

        public bool TryGetDynamicPreset(Player player, out ScpVoicePreset preset)
        {
            preset = null;

            if (player.Role != RoleTypeId.Scp939)
                return false;

            if (!_states.TryGetValue(player, out var controller))
                return false;

            preset = Scp939DynamicPresets.GetPresetForState(controller.CurrentState);
            return true;
        }
    }

}