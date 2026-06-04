namespace SCP_Immersive_Voice.Managers
{
    using UnityEngine;

    public class ScpVoiceUpdater : MonoBehaviour
    {
        private ScpVoiceManager _manager;

        public void Init(ScpVoiceManager manager)
        {
            _manager = manager;
        }

        private void Update()
        {
            _manager?.UpdatePositions();
        }
    }
}
