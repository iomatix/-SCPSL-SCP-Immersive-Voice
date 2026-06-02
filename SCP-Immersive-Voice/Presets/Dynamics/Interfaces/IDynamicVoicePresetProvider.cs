namespace SCP_Immersive_Voice.Presets.Dynamics.Interfaces
{
    using LabApi.Features.Wrappers;
    public interface IDynamicVoicePresetProvider
    {
        bool TryGetDynamicPreset(Player player, out ScpVoicePreset preset);
    }
}
