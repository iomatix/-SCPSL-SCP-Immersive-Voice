namespace SCP_Immersive_Voice.Presets.Dynamics.Controllers
{
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;
    using System;

    public class Scp3114VoiceStateController
    {
        private Scp3114VoiceState _currentState = Scp3114VoiceState.Undisguised;
        private DateTime _timestamp = DateTime.UtcNow;
        private float _timeout = 0f;

        public Scp3114VoiceState CurrentState
        {
            get
            {
                if (_timeout > 0f && (DateTime.UtcNow - _timestamp).TotalSeconds > _timeout)
                    ResetToIdle();
                return _currentState;
            }
        }

        public void SetState(Scp3114VoiceState state, float timeoutSeconds = 0f)
        {
            _currentState = state;
            _timestamp = DateTime.UtcNow;
            _timeout = timeoutSeconds;
        }

        public void ResetToIdle()
        {
            _currentState = Scp3114VoiceState.Undisguised;
            _timeout = 0f;
        }
    }
}