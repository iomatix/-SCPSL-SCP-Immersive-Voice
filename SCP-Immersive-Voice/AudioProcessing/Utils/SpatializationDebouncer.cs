namespace SCP_Immersive_Voice.AudioProcessing.Utils
{
    using System.Diagnostics;

    /// <summary>
    /// Thread-safe, allocation-free temporal debouncer providing a minimum hysteresis boundary 
    /// for spatialization state updates, protecting underlying native audio circular buffers from jitter.
    /// </summary>
    public class SpatializationDebouncer
    {
        private readonly long _hysteresisTicks;
        private long _lastTransitionTimestamp;
        private bool _currentSpatializedState;

        // INTENT: Establishes a localized time-barrier based on low-overhead system hardware ticks 
        // to block rapid network jitter from driving high-frequency circular buffer state changes.
        public SpatializationDebouncer(float minimumHysteresisMs = 350f)
        {
            _hysteresisTicks = (long)((minimumHysteresisMs / 1000f) * Stopwatch.Frequency);
            _lastTransitionTimestamp = 0;
            _currentSpatializedState = false;
        }

        // INTENT: Controls and validates transition requests by testing against the elapsed tick boundary,
        // enforcing structural debounce thresholds with zero heap impact on volatile threads.
        public bool UpdateState(bool targetSpatialized)
        {
            if (targetSpatialized == _currentSpatializedState)
            {
                return _currentSpatializedState;
            }

            long currentTicks = Stopwatch.GetTimestamp();
            if (_lastTransitionTimestamp == 0 || (currentTicks - _lastTransitionTimestamp) >= _hysteresisTicks)
            {
                _currentSpatializedState = targetSpatialized;
                _lastTransitionTimestamp = currentTicks;
            }

            return _currentSpatializedState;
        }

        // INTENT: Flushes tracking fields cleanly to handle hard state adjustments like user respawns or connection losses.
        public void Reset(bool initialState = false)
        {
            _currentSpatializedState = initialState;
            _lastTransitionTimestamp = 0;
        }
    }
}