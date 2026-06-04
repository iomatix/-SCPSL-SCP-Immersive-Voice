namespace SCP_Immersive_Voice.Presets.Dynamics.Controllers
{
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;
    using System;

    public class Scp106VoiceStateController
    {
        private Scp106VoiceState _currentState = Scp106VoiceState.Idle;
        private DateTime _timestamp = DateTime.UtcNow;
        private float _timeout = 0f;
        private bool _lowVigorActive = false;

        public Scp106VoiceState CurrentState
        {
            get
            {
                if (_timeout > 0f && (DateTime.UtcNow - _timestamp).TotalSeconds > _timeout)
                    _currentState = _lowVigorActive ? Scp106VoiceState.LowVigor : Scp106VoiceState.Idle;
                return _currentState;
            }
        }

        public void SetState(Scp106VoiceState state, float timeoutSeconds = 0f)
        {
            _currentState = state;
            _timestamp = DateTime.UtcNow;
            _timeout = timeoutSeconds;
        }

        public void TrySetMediumPriorityState(Scp106VoiceState state)
        {
            if (_currentState == Scp106VoiceState.Idle || _currentState == Scp106VoiceState.LowVigor)
            {
                _currentState = state;
                _timeout = 0f;
            }
        }

        public void TrySetLowPriorityState(Scp106VoiceState state)
        {
            _lowVigorActive = true;
            if (_currentState == Scp106VoiceState.Idle) _currentState = state;
        }

        public void ClearLowVigor()
        {
            _lowVigorActive = false;
            if (_currentState == Scp106VoiceState.LowVigor) _currentState = Scp106VoiceState.Idle;
        }

        public void ResetToIdle()
        {
            _currentState = Scp106VoiceState.Idle;
            _timeout = 0f;
        }
    }
}