using AudioManagerAPI.Defaults;
using AudioManagerAPI.Features.Enums;
using LabApi.Extensions;
using LabApi.Extensions.Misc;
using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.VoiceProfiles;
using ScpImmersiveVoice;
using ScpImmersiveVoice.Config;
using System.Collections.Concurrent;

namespace SCP_Immersive_Voice.Managers
{
    /// <summary>
    /// Thread-safe central manager governing multi-threaded stream allocations and temporal positional tracking matrices.
    /// </summary>
    public class ScpVoiceManager
    {
        #region Private Repositories & Thread Guards
        private readonly ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;
        private readonly ConcurrentDictionary<int, VoiceSession> _sessions = new();
        private readonly object _allocationLock = new();
        #endregion

        #region Session Management Lifecycle
        /// <summary>
        /// Establishes and tracks a hardware streaming channel for a specific anomalous unit using double-checked pattern matching.
        /// </summary>
        public VoiceSession StartSession(Player scp)
        {
            if (scp is null) return null;

            if (_sessions.TryGetValue(scp.PlayerId, out var existing))
                return existing;

            lock (_allocationLock)
            {
                // Double-checked locking thread validation guard
                if (_sessions.TryGetValue(scp.PlayerId, out existing))
                    return existing;

                // Architectural Upgrade: Transitioned to the generic, allocation-free CreateStreamSession pipeline
                // Passing the 'scp' instance directly as TState avoids reference-capturing closures and heap garbage
                int sessionId = DefaultAudioManager.Instance.CreateStreamSession<Player>(
                    position: scp.Position,
                    isSpatial: true,
                    minDistance: 4.25f,
                    maxDistance: _config.ProximityDistance,
                    volume: 1f,
                    state: scp,
                    validPlayersFilter: (p, scpSource) => p is not null && p.IsReady && p != scpSource,
                    priority: AudioPriority.High,
                    persistent: false,
                    lifespan: null,
                    autoCleanup: false
                );

                if (sessionId is 0) return null;

                var session = new VoiceSession { SessionId = sessionId, PlayerInstance = scp };
                _sessions[scp.PlayerId] = session;

                iLogger.Debug(nameof(ScpVoiceManager), $"[VOICE HARDENING] Session REGISTERED. PlayerId: {scp.PlayerId}, SessionId: {sessionId}", _config.Debug);
                return session;
            }
        }

        /// <summary>
        /// Tears down an active hardware streaming session for a target unit and purges tracking references.
        /// </summary>
        public void StopSession(Player scp)
        {
            if (scp is null) return;
            int key = scp.PlayerId;

            if (_sessions.TryRemove(key, out var session))
            {
                DefaultAudioManager.Instance.DestroySession(session.SessionId);
                iLogger.Debug(nameof(ScpVoiceManager), $"[VOICE HARDENING] Audio Streaming Session no. {session.SessionId} successfully destroyed for {scp.Nickname}", _config.Debug);
            }
        }

        /// <summary>
        /// Systematically cuts all active audio channels from the native sound engine heap.
        /// </summary>
        public void StopAllSessions()
        {
            foreach (var (_, session) in _sessions)
            {
                try { DefaultAudioManager.Instance.DestroySession(session.SessionId); } catch { }
            }

            iLogger.Debug(nameof(ScpVoiceManager), "[VOICE HARDENING] All active audio streaming slots cleared from native heap.", _config.Debug);
            _sessions.Clear();
        }
        #endregion

        #region Data Transmission Pipelines
        /// <summary>
        /// Appends pre-allocated rolling PCM blocks directly into the native stream mixer.
        /// Bypasses redundant dictionary tracking lookups for maximum execution speed.
        /// </summary>
        public void AppendPcmDirect(VoiceSession session, float[] samples)
        {
            if (session is null || samples is null || samples.Length == 0) return;
            DefaultAudioManager.Instance.AppendPcmData(session.SessionId, samples);
        }

        /// <summary>
        /// Appends raw float PCM sample blocks directly into the stateful session execution pipeline.
        /// </summary>
        public void AppendPcm(Player scp, float[] samples)
        {
            if (scp is null || samples is null || samples.Length == 0) return;

            if (!_sessions.TryGetValue(scp.PlayerId, out var session))
            {
                session = StartSession(scp);
            }

            if (session is null) return;

            DefaultAudioManager.Instance.AppendPcmData(session.SessionId, samples);
        }
        #endregion

        #region Positional & Spatialization Tick Processor
        /// <summary>
        /// Traverses tracked channels, updates hardware positional transforms, and evaluates spatial filters.
        /// Executed at a high frequency on the primary server tick thread.
        /// </summary>
        public void UpdatePositions()
        {
            if (_sessions.IsEmpty) return;

            foreach (var (playerId, session) in _sessions)
            {
                Player scp = session.PlayerInstance;
                if (scp is null || !scp.IsReady)
                {
                    if (_sessions.TryRemove(playerId, out var deadSession))
                    {
                        try { DefaultAudioManager.Instance.DestroySession(deadSession.SessionId); } catch { }
                        iLogger.Warn(nameof(ScpVoiceManager), $"[VOICE SECURITY] Pruned voice session {deadSession.SessionId} for disconnected PlayerId: {playerId}");
                    }
                    continue;
                }

                // Push vector changes straight into the native engine layer
                DefaultAudioManager.Instance.SetSessionPosition(session.SessionId, scp.Position);

                var state = DefaultAudioManager.Instance.GetSessionState(session.SessionId);
                if (state is null) continue;

                var activePreset = ScpVoiceProfiles.GetPreset(scp);
                if (activePreset is not null)
                {
                    bool targetSpatialization = !activePreset.IsGlobalTransmission;

                    // Route requests into the clean temporal debouncer context to shelter buffers from flushes
                    bool debouncedSpatialization = session.SpatialDebouncer.UpdateState(targetSpatialization);

                    if (state.IsSpatial != debouncedSpatialization)
                    {
                        state.IsSpatial = debouncedSpatialization;

                        if (state.HasPhysicalSpeaker && state.PhysicalSpeaker is not null)
                        {
                            state.PhysicalSpeaker.SetSpatialization(debouncedSpatialization);

                            if (!debouncedSpatialization)
                                state.PhysicalSpeaker.SetVolume(activePreset.OutputGain * 0.85f);
                        }
                    }
                }
            }
        }
        #endregion
    }
}