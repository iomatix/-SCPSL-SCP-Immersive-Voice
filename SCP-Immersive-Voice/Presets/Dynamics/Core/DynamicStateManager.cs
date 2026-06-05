namespace SCP_Immersive_Voice.Presets.Dynamics.Core
{
    using LabApi.Features.Wrappers;
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Interfaces;
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Thread-safe generic state manager that bridges in-game events with the DSP audio pipeline routing.
    /// Eliminates boilerplate by dynamically driving preset translation for any explicit SCP role.
    /// </summary>
    /// <typeparam name="TState">The enum type representing the state graph for the target SCP.</typeparam>
    public class DynamicStateManager<TState> : IDynamicVoicePresetProvider where TState : Enum
    {
        private readonly ConcurrentDictionary<int, VoiceStateTracker<TState>> _activeTrackers = new ConcurrentDictionary<int, VoiceStateTracker<TState>>();
        private readonly RoleTypeId _targetRole;
        private readonly TState _defaultState;
        private readonly Func<TState, ScpVoicePreset> _presetResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicStateManager{TState}"/> class.
        /// </summary>
        /// <param name="targetRole">The dedicated SCP role managed by this instance.</param>
        /// <param name="defaultState">The baseline entry-level state for the managed role.</param>
        /// <param name="presetResolver">The delegate function mapping the active state directly to a handcrafted <see cref="ScpVoicePreset"/>.</param>
        public DynamicStateManager(RoleTypeId targetRole, TState defaultState, Func<TState, ScpVoicePreset> presetResolver)
        {
            if (presetResolver == null)
                throw new ArgumentNullException(nameof(presetResolver));

            _targetRole = targetRole;
            _defaultState = defaultState;
            _presetResolver = presetResolver;
        }

        /// <summary>
        /// Transitions a player to a specific voice state with an optional transient expiration watchdog window.
        /// </summary>
        /// <param name="player">The player instance whose voice profile state is being manipulated.</param>
        /// <param name="state">The target state to be applied.</param>
        /// <param name="maxDurationSeconds">The maximum time limit before the state is automatically rolled back to baseline.</param>
        public void SetState(Player player, TState state, float maxDurationSeconds = 0f)
        {
            if (player == null) return;

            var tracker = _activeTrackers.GetOrAdd(player.PlayerId, id => new VoiceStateTracker<TState>(_defaultState));
            tracker.UpdateState(state, maxDurationSeconds);
        }

        /// <summary>
        /// Forces an immediate, atomic rollback of the player's tracking container back into its baseline fallback profile.
        /// </summary>
        /// <param name="player">The player instance to be reset.</param>
        public void ResetToDefault(Player player)
        {
            if (player == null) return;

            if (_activeTrackers.TryGetValue(player.PlayerId, out var tracker))
            {
                tracker.ResetToFallback();
            }
        }

        /// <summary>
        /// Purges a player's state container from the tracking registry. 
        /// Crucial for session teardowns, role switches, and disconnect hooks to clean system memory boundaries.
        /// </summary>
        /// <param name="player">The player instance whose data is being purged.</param>
        public void RemovePlayer(Player player)
        {
            if (player == null) return;
            _activeTrackers.TryRemove(player.PlayerId, out _);
        }

        /// <summary>
        /// Resolves the real-time dynamic voice preset configuration for a given player.
        /// Core interface implementation of <see cref="IDynamicVoicePresetProvider"/>.
        /// </summary>
        /// <param name="player">The player context requesting a voice profile validation pass.</param>
        /// <param name="preset">The resulting calibrated voice configuration layout container.</param>
        /// <returns><c>true</c> if the provider owns the target configuration mapping; otherwise, <c>false</c>.</returns>
        public bool TryGetDynamicPreset(Player player, out ScpVoicePreset preset)
        {
            preset = null;

            if (player == null || player.Role != _targetRole)
            {
                return false;
            }

            // CRITICAL PRODUCTION SAFETEY FIX (Lazy Initialization):
            // If the player has just spawned and hasn't triggered any game event hooks yet,
            // we safely inject them into the concurrency tracker map using the baseline fallback.
            // This guarantees the pipeline never registers a null or unmapped state boundary, preventing audio dropouts.
            var tracker = _activeTrackers.GetOrAdd(player.PlayerId, id => new VoiceStateTracker<TState>(_defaultState));

            TState activeState = tracker.GetActiveState();
            preset = _presetResolver(activeState);

            return preset != null;
        }
    }
}