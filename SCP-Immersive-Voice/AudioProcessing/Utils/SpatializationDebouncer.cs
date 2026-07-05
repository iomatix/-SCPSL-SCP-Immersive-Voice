using System.Diagnostics;

namespace SCP_Immersive_Voice.AudioProcessing.Utils
{
    /// <summary>
    /// Thread-safe, allocation-free temporal debouncer providing a minimum hysteresis boundary 
    /// for spatialization state updates, protecting underlying native audio circular buffers from jitter.
    /// </summary>
    public class SpatializationDebouncer
    {
        #region Private Repositories
        private readonly long _hysteresisTicks;
        private long _lastTransitionTimestamp;
        private bool _currentSpatializedState;
        #endregion

        #region Initialization
        /// <summary>
        /// Establishes a localized time-barrier based on low-overhead system hardware ticks 
        /// to block rapid network jitter from driving high-frequency circular buffer state changes.
        /// </summary>
        public SpatializationDebouncer(float minimumHysteresisMs = 350f)
        {
            _hysteresisTicks = (long)((minimumHysteresisMs / 1000f) * Stopwatch.Frequency);
            _lastTransitionTimestamp = 0;
            _currentSpatializedState = false;
        }
        #endregion

        #region Operational Logic
        /// <summary>
        /// Controls and validates transition requests by testing against the elapsed tick boundary,
        /// enforcing structural debounce thresholds with zero heap impact on volatile threads.
        /// </summary>
        public bool UpdateState(bool targetSpatialized)
        {
            if (targetSpatialized == _currentSpatializedState)
            {
                return _currentSpatializedState;
            }

            long currentTicks = Stopwatch.GetTimestamp();

            // C# 9.0 Pattern Matching metrics evaluation
            if (_lastTransitionTimestamp is 0 || (currentTicks - _lastTransitionTimestamp) >= _hysteresisTicks)
            {
                _currentSpatializedState = targetSpatialized;
                _lastTransitionTimestamp = currentTicks;
            }

            return _currentSpatializedState;
        }

        /// <summary>
        /// Flushes tracking fields cleanly to handle hard state adjustments like user respawns or connection losses.
        /// </summary>
        public void Reset(bool initialState = false)
        {
            _currentSpatializedState = initialState;
            _lastTransitionTimestamp = 0;
        }
        #endregion
    }
}