using UnityEngine;

namespace SCP_Immersive_Voice.Managers
{

    public class ScpVoiceUpdater : MonoBehaviour
    {
        private ScpVoiceManager _manager;
        private float _lastUpdateTime;
        private const float UpdateInterval = 0.030f; // (30ms = ~33Hz)

        /// <summary>
        /// Binds the active thread-safe voice manager instance to this Unity lifecycle hook.
        /// </summary>
        public void Init(ScpVoiceManager manager)
        {
            _manager = manager;
            _lastUpdateTime = Time.time;
        }

        private void Update()
        {
            float currentTime = Time.time;
            if (currentTime - _lastUpdateTime >= UpdateInterval)
            {
                _lastUpdateTime = currentTime;
                _manager?.UpdatePositions();
            }
        }
    }
}