namespace SCP_Immersive_Voice.Presets.Dynamics.Controllers
{
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    public class Scp106VoiceStateController
    {
        public Scp106VoiceState CurrentState { get; private set; } = Scp106VoiceState.Idle;

        private bool _lowVigorActive = false;

        // Intent: force state with highest priority
        public void SetState(Scp106VoiceState state)
        {
            CurrentState = state;
        }

        // Intent: temporary states like Emerging or PocketDimension (no timer, overwritten by next state)
        public void SetTemporaryState(Scp106VoiceState state, Scp106VoiceState fallback)
        {
            CurrentState = state;
            // Fallback can be applied explicitly from events if needed later.
        }

        // Intent: medium priority states (AtlasDimensional)
        public void TrySetMediumPriorityState(Scp106VoiceState state)
        {
            if (CurrentState == Scp106VoiceState.Idle ||
                CurrentState == Scp106VoiceState.LowVigor)
            {
                CurrentState = state;
            }
        }

        // Intent: low priority states (LowVigor)
        public void TrySetLowPriorityState(Scp106VoiceState state)
        {
            if (CurrentState == Scp106VoiceState.Idle)
            {
                CurrentState = state;
                _lowVigorActive = true;
            }
        }

        public void ClearLowVigor()
        {
            if (_lowVigorActive)
            {
                _lowVigorActive = false;
                CurrentState = Scp106VoiceState.Idle;
            }
        }
    }
}