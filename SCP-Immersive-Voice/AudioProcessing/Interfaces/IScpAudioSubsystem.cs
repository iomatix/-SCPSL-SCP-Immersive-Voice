using LabApi.Features.Wrappers;

namespace SCP_Immersive_Voice.AudioProcessing.Interfaces
{
    /// <summary>
    /// Defines a unified, decoupled architectural contract for an anomalous audio feature subsystem.
    /// </summary>
    public interface IScpAudioSubsystem
    {
        /// <summary>
        /// Binds subsystem network events and registers dynamic topologies into the voice engine profiling queues.
        /// </summary>
        void BindPipelines();

        /// <summary>
        /// Safely detaches all active network listeners and flushes functional hooks from the event loop.
        /// </summary>
        void UnbindPipelines();

        /// <summary>
        /// Evicts a specific target player entity context from internal cache matrices instantly.
        /// </summary>
        /// <param name="player">The target entity player wrapper to evict.</param>
        void PurgePlayer(Player player);
    }
}