using UnityEngine;

namespace SCP_Immersive_Voice.Managers
{
    /// <summary>
    /// Unity lifecycle proxy forwarding frame-tick execution bounds straight into the core voice manager matrix.
    /// </summary>
    public class ScpVoiceUpdater : MonoBehaviour
    {
        private ScpVoiceManager _manager;

        /// <summary>
        /// Binds the active thread-safe voice manager instance to this Unity lifecycle hook.
        /// </summary>
        public void Init(ScpVoiceManager manager) => _manager = manager;

        /// <summary>
        /// Frame-rate dependent tick trigger sweeping positional transforms lock-free.
        /// </summary>
        private void Update() => _manager?.UpdatePositions();
    }
}