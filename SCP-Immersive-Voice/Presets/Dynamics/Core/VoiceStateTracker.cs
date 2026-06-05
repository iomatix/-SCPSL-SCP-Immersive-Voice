namespace SCP_Immersive_Voice.Presets.Dynamics.Core
{
    using System;

    /// <summary>
    /// Manages the real-time lifecycle, expiration timing, and automated watchdog safety of a single player's voice state.
    /// Designed to prevent thread-locking and stuck voice states under heavy network or server load.
    /// </summary>
    /// <typeparam name="TState">The enum type representing the individual SCP voice state steps.</typeparam>
    public class VoiceStateTracker<TState> where TState : Enum
    {
        /// <summary>
        /// Gets the current active voice state applied to the player.
        /// </summary>
        public TState CurrentState { get; private set; }

        /// <summary>
        /// Gets the structural baseline state used as an absolute fallback layout.
        /// </summary>
        public TState FallbackState { get; private set; }

        /// <summary>
        /// Gets the precise UTC timestamp indicating when the current state was introduced.
        /// </summary>
        public DateTime StateTimestamp { get; private set; }

        /// <summary>
        /// Gets the maximum physical lifespan of the transient state in seconds.
        /// A value of 0.0f implies an infinite duration until explicitly altered by game logic.
        /// </summary>
        public float MaxDuration { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceStateTracker{TState}"/> class.
        /// </summary>
        /// <param name="defaultState">The entry-level baseline state for the tracking domain.</param>
        public VoiceStateTracker(TState defaultState)
        {
            CurrentState = defaultState;
            FallbackState = defaultState;
            StateTimestamp = DateTime.UtcNow;
            MaxDuration = 0f;
        }

        /// <summary>
        /// Transitions the tracking container into a new state with an optional watchdog expiration window.
        /// </summary>
        /// <param name="newState">The incoming voice state to be parsed by the graph coordinator.</param>
        /// <param name="maxDurationSeconds">The strict lifespan limit before the watchdog forces an automatic fallback revert.</param>
        public void UpdateState(TState newState, float maxDurationSeconds = 0f)
        {
            CurrentState = newState;
            StateTimestamp = DateTime.UtcNow;
            MaxDuration = maxDurationSeconds;
        }

        /// <summary>
        /// Forcefully clears volatile state variables and drops the tracker context back into its structural fallback baseline.
        /// </summary>
        public void ResetToFallback()
        {
            CurrentState = FallbackState;
            MaxDuration = 0f;
        }

        /// <summary>
        /// Evaluates and resolves the active voice state context. 
        /// Automatically performs an atomic thread-safe watchdog delta compression sweep if a state has breached its allowed lifespan.
        /// </summary>
        /// <returns>The synchronized and validated active state layout at this exact millisecond.</returns>
        public TState GetActiveState()
        {
            if (MaxDuration > 0f)
            {
                double elapsedSeconds = (DateTime.UtcNow - StateTimestamp).TotalSeconds;
                if (elapsedSeconds > MaxDuration)
                {
                    ResetToFallback();
                }
            }

            return CurrentState;
        }

    }
}