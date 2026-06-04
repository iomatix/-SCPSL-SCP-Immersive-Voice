namespace SCP_Immersive_Voice.Presets.Dynamics.Controllers
{
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;
    using System;

    public class Scp939VoiceStateController
    {
        private Scp939VoiceState _currentState = Scp939VoiceState.IdleWhisper;
        private DateTime _stateTimestamp = DateTime.UtcNow;
        private float _maxStateDurationSeconds = 0f;

        public Scp939VoiceState CurrentState
        {
            get
            {
                // Watchdog: If a transient state exceeds its maximum physical lifespan, auto-fallback to idle whisper
                if (_maxStateDurationSeconds > 0f && (DateTime.UtcNow - _stateTimestamp).TotalSeconds > _maxStateDurationSeconds)
                {
                    ResetToIdle();
                }
                return _currentState;
            }
        }

        public void SetState(Scp939VoiceState newState, float maxDurationSeconds = 0f)
        {
            _currentState = newState;
            _stateTimestamp = DateTime.UtcNow;
            _maxStateDurationSeconds = maxDurationSeconds;
        }

        public void ResetToIdle()
        {
            _currentState = Scp939VoiceState.IdleWhisper;
            _maxStateDurationSeconds = 0f;
        }
    }
}