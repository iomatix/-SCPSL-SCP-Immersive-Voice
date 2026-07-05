using LabApi.Features.Wrappers;

namespace SCP_Immersive_Voice.Presets.Dynamics.Interfaces
{
    /// <summary>
    /// Defines the contract for modular, biomorphic anomaly handlers capable of injecting runtime-dynamic DSP voice profiles.
    /// </summary>
    public interface IDynamicVoicePresetProvider
    {
        /// <summary>
        /// Attempts to resolve an active dynamic voice preset based on the current live state metrics of the player.
        /// </summary>
        /// <param name="player">The target player entity to evaluate.</param>
        /// <param name="preset">When this method returns, contains the active dynamic DSP configuration; otherwise, null.</param>
        /// <returns>True if a dynamic state preset was successfully matched; otherwise, false.</returns>
        bool TryGetDynamicPreset(Player player, out ScpVoicePreset preset);
    }
}