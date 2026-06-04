namespace SCP_Immersive_Voice.Presets.Dynamics.Controllers
{
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;
    using System;

    public class Scp096VoiceStateController
    {
        private Scp096VoiceState _currentState = Scp096VoiceState.Calm;
        private DateTime _timestamp = DateTime.UtcNow;
        private float _timeout = 0f;

        public Scp096VoiceState CurrentState
        {
            get
            {
                if (_timeout > 0f && (DateTime.UtcNow - _timestamp).TotalSeconds > _timeout)
                    ResetToIdle();
                return _currentState;
            }
        }

        public void SetState(Scp096VoiceState state, float timeoutSeconds = 0f)
        {
            _currentState = state;
            _timestamp = DateTime.UtcNow;
            _timeout = timeoutSeconds;
        }

        public void ResetToIdle()
        {
            _currentState = Scp096VoiceState.Calm;
            _timeout = 0f;
        }
    }
}