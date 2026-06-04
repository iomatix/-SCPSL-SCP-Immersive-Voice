namespace SCP_Immersive_Voice.Presets.Dynamics.Providers
{
    using LabApi.Features.Wrappers;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Controllers;
    using SCP_Immersive_Voice.Presets.Dynamics.Interfaces;
    using System.Collections.Concurrent;

    public class Scp939DynamicPresetProvider : IDynamicVoicePresetProvider
    {
        private readonly ConcurrentDictionary<int, Scp939VoiceStateController> _states;

        public Scp939DynamicPresetProvider(ConcurrentDictionary<int, Scp939VoiceStateController> states)
        {
            _states = states;
        }

        public bool TryGetDynamicPreset(Player player, out ScpVoicePreset preset)
        {
            preset = null;

            if (player.Role != RoleTypeId.Scp939)
                return false;

            if (!_states.TryGetValue(player.PlayerId, out var controller))
                return false;

            preset = Scp939DynamicPresets.GetPresetForState(controller.CurrentState);
            return true;
        }
    }
}