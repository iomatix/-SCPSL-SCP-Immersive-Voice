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
    /// Features advanced real-time console debugging traces for state transitions and watchdog lifecycles.
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
        public void SetState(Player player, TState state, float maxDurationSeconds = 0f)
        {
            if (player == null) return;

            var tracker = _activeTrackers.GetOrAdd(player.PlayerId, id => new VoiceStateTracker<TState>(_defaultState));

            //  FIX: Suppress redundant state hammering logs and executions if the player is already established in this exact state.
            // We allow duration-based states to pass through to let cascading watchdogs refresh their timers cleanly.
            if (tracker.CurrentState.Equals(state) && maxDurationSeconds == 0f)
                return;

            LabApi.Features.Console.Logger.Debug($"[VOICE-STATE] Player '{player.Nickname}' ({player.PlayerId}) manually shifted to state: {state} (Watchdog Lifespan: {maxDurationSeconds}s)");

            tracker.UpdateState(state, maxDurationSeconds);
        }

        /// <summary>
        /// Peeks into the live registry to resolve the current active tracking state of a player.
        /// </summary>
        /// <param name="player">The target player context.</param>
        /// <returns>The current tracked state or the baseline default if unassigned.</returns>
        public TState GetCurrentState(Player player)
        {
            if (player == null) return _defaultState;
            return _activeTrackers.TryGetValue(player.PlayerId, out var tracker) ? tracker.CurrentState : _defaultState;
        }

        /// <summary>
        /// Forces an immediate, atomic rollback of the player's tracking container back into its baseline fallback profile.
        /// </summary>
        public void ResetToDefault(Player player)
        {
            if (player == null) return;

            if (_activeTrackers.TryGetValue(player.PlayerId, out var tracker))
            {
                // DIAGNOSTIC TRACE: Log manual baseline resets
                LabApi.Features.Console.Logger.Debug($"[VOICE-STATE] Player '{player.Nickname}' ({player.PlayerId}) reset to baseline state: {_defaultState}");
                tracker.ResetToFallback();
            }
        }

        /// <summary>
        /// Purges a player's state container from the tracking registry.
        /// </summary>
        public void RemovePlayer(Player player)
        {
            if (player == null) return;

            if (_activeTrackers.TryRemove(player.PlayerId, out _))
            {
                LabApi.Features.Console.Logger.Debug($"[VOICE-STATE] Purged tracking profile context for player '{player.Nickname}' ({player.PlayerId}) due to session teardown.");
            }
        }

        /// <summary>
        /// Resolves the real-time dynamic voice preset configuration for a given player.
        /// </summary>
        public bool TryGetDynamicPreset(Player player, out ScpVoicePreset preset)
        {
            preset = null;

            if (player == null || player.Role != _targetRole)
            {
                return false;
            }

            var tracker = _activeTrackers.GetOrAdd(player.PlayerId, id => {
                // DIAGNOSTIC TRACE: Log lazy initialization on first voice packet hit
                LabApi.Features.Console.Logger.Debug($"[VOICE-STATE] Lazy-initialized fresh dynamic tracking registry for '{player.Nickname}' ({player.PlayerId}) as {_defaultState}");
                return new VoiceStateTracker<TState>(_defaultState);
            });

            // Capture states before evaluating the time delta compression pass to detect watchdog triggers
            TState stateBeforeUpdate = tracker.CurrentState;
            TState activeState = tracker.GetActiveState();

            // DIAGNOSTIC TRACE: Check if the Watchdog automatically reverted the state due to an expiration timeout
            if (!stateBeforeUpdate.Equals(activeState))
            {
                LabApi.Features.Console.Logger.Debug($"[VOICE-WATCHDOG] Watchdog fired! Active state for '{player.Nickname}' ({player.PlayerId}) automatically expired from {stateBeforeUpdate} and rolled back to baseline: {activeState}");
            }

            preset = _presetResolver(activeState);
            return preset != null;
        }
    }
}