using System;

namespace SCP_Immersive_Voice.Presets.Dynamics.Core
{
    /// <summary>
    /// Thread-safe, allocation-free temporal watchdog matrix managing the real-time lifecycle, 
    /// expiration timing, and automated safety reversion of a single player's voice state.
    /// Prevents stuck voice states under heavy network or concurrent server load.
    /// </summary>
    /// <typeparam name="TState">The enum type representing the individual SCP voice state steps.</typeparam>
    public class VoiceStateTracker<TState> where TState : Enum
    {
        #region Private Repositories & Thread Guards
        private readonly object _lock = new();
        private TState _currentState;
        private DateTime _stateTimestamp;
        private float _maxDuration;
        #endregion

        #region Operational Properties
        /// <summary>
        /// Gets the current active voice state applied to the player.
        /// </summary>
        public TState CurrentState
        {
            get { lock (_lock) return _currentState; }
            private set { _currentState = value; }
        }

        /// <summary>
        /// Gets the structural baseline state used as an absolute fallback layout.
        /// </summary>
        public TState FallbackState { get; init; }

        /// <summary>
        /// Gets the precise UTC timestamp indicating when the current state was introduced.
        /// </summary>
        public DateTime StateTimestamp
        {
            get { lock (_lock) return _stateTimestamp; }
            private set { _stateTimestamp = value; }
        }

        /// <summary>
        /// Gets the maximum physical lifespan of the transient state in seconds.
        /// A value of 0.0f implies an infinite duration until explicitly altered by game logic.
        /// </summary>
        public float MaxDuration
        {
            get { lock (_lock) return _maxDuration; }
            private set { _maxDuration = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceStateTracker{TState}"/> class.
        /// </summary>
        /// <param name="defaultState">The entry-level baseline state for the tracking domain.</param>
        public VoiceStateTracker(TState defaultState)
        {
            _currentState = defaultState;
            FallbackState = defaultState;
            _stateTimestamp = DateTime.UtcNow;
            _maxDuration = 0f;
        }
        #endregion

        #region State Mutation Methods
        /// <summary>
        /// Transitions the tracking container into a new state securely with a custom watchdog expiration window.
        /// </summary>
        /// <param name="newState">The incoming voice state to be parsed by the graph coordinator.</param>
        /// <param name="maxDurationSeconds">The strict lifespan limit before the watchdog forces an automatic fallback revert.</param>
        public void UpdateState(TState newState, float maxDurationSeconds = 0f)
        {
            lock (_lock)
            {
                _currentState = newState;
                _stateTimestamp = DateTime.UtcNow;
                _maxDuration = maxDurationSeconds;
            }
        }

        /// <summary>
        /// Forcefully clears volatile state variables and drops the tracker context back into its structural fallback baseline.
        /// </summary>
        public void ResetToFallback()
        {
            lock (_lock)
            {
                _currentState = FallbackState;
                _maxDuration = 0f;
            }
        }

        /// <summary>
        /// Evaluates and resolves the active voice state context. 
        /// Automatically performs an atomic thread-safe watchdog delta compression sweep if a state has breached its allowed lifespan.
        /// </summary>
        /// <returns>The synchronized and validated active state layout at this exact millisecond.</returns>
        public TState GetActiveState()
        {
            lock (_lock)
            {
                if (_maxDuration > 0f)
                {
                    double elapsedSeconds = (DateTime.UtcNow - _stateTimestamp).TotalSeconds;
                    if (elapsedSeconds > _maxDuration)
                    {
                        _currentState = FallbackState;
                        _maxDuration = 0f;
                    }
                }

                return _currentState;
            }
        }
        #endregion
    }
}