namespace SCP_Immersive_Voice.Managers
{
    using UnityEngine;

    public class ScpVoiceUpdater : MonoBehaviour
    {
        private void Update()
        {
            ScpVoiceManager.Instance.UpdatePositions();
        }
    }
}
