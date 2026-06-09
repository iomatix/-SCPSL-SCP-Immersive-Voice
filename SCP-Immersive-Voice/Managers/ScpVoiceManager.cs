namespace SCP_Immersive_Voice.Managers
{
    using AudioManagerAPI.Defaults;
    using AudioManagerAPI.Features.Enums;
    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.VoiceProfiles;
    using ScpImmersiveVoice;
    using ScpImmersiveVoice.Config;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ScpVoiceManager
    {
        private readonly ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;

        /// <summary>
        /// Audio sessions registry tracking active streams. Key = PlayerId, Value = SessionId.
        /// </summary>
        private readonly Dictionary<int, int> _sessions = new Dictionary<int, int>();

        /// <summary>
        /// Thread-safe synchronization root protecting all internal collection mutations across network threads.
        /// </summary>
        private readonly object _stateLock = new object();

        /// <summary>
        /// Allocates a synchronized real-time audio stream channel for a designated network actor.
        /// </summary>
        public int StartSession(Player scp)
        {
            if (scp == null) return 0;

            lock (_stateLock)
            {
                // Double-check lock pattern to prevent duplicate thread racing allocation
                if (_sessions.TryGetValue(scp.PlayerId, out int existing))
                    return existing;

                int sessionId = DefaultAudioManager.Instance.CreateStreamSession(
                    position: scp.Position,
                    isSpatial: true,
                    minDistance: 4.25f,
                    maxDistance: _config.ProximityDistance,
                    volume: 1f,
                    priority: AudioPriority.High,
                    validPlayersFilter: p => p != null && p.IsReady && p != scp
                );

                if (sessionId == 0) return 0;

                _sessions[scp.PlayerId] = sessionId;

                Logger.Debug($"[VOICE HARDENING] Session REGISTERED. PlayerId: {scp.PlayerId}, SessionId: {sessionId}");
                return sessionId;
            }
        }

        /// <summary>
        /// Deterministically tears down an active voice transmission pipeline and releases hardware descriptors.
        /// </summary>
        public void StopSession(Player scp)
        {
            if (scp == null) return;

            var key = scp.PlayerId;

            lock (_stateLock)
            {
                if (!_sessions.TryGetValue(key, out int sessionId))
                {
                    // Shuts down quiet log noise for non-tracked base human players changing roles
                    return;
                }

                DefaultAudioManager.Instance.DestroySession(sessionId);
                _sessions.Remove(key);

                Logger.Debug($"[VOICE HARDENING] Audio Streaming Session no. {sessionId} successfully destroyed for {scp.Nickname}");
            }
        }

        /// <summary>
        /// Forces a cascade flush of all active voice allocations during hard plugin/round context drops.
        /// </summary>
        public void StopAllSessions()
        {
            lock (_stateLock)
            {
                foreach (var kvp in _sessions)
                {
                    try { DefaultAudioManager.Instance.DestroySession(kvp.Value); } catch { }
                }

                Logger.Debug("[VOICE HARDENING] All active audio streaming slots cleared from native heap.");
                _sessions.Clear();
            }
        }

        /// <summary>
        /// Feeds incoming asynchronous raw PCM frames straight into the synchronized active hardware pipeline.
        /// </summary>
        public void AppendPcm(Player scp, float[] samples)
        {
            if (scp == null || samples == null || samples.Length == 0) return;

            var key = scp.PlayerId;
            int sessionId;

            // Thread-safe isolation gate ensuring dictionary read/write safety
            lock (_stateLock)
            {
                if (!_sessions.TryGetValue(key, out sessionId))
                {
                    sessionId = StartSession(scp);
                }
            }

            if (sessionId == 0) return;

            // Audio push operations execute outside the strict state lock to guarantee sub-millisecond voice thread performance
            DefaultAudioManager.Instance.AppendPcmData(sessionId, samples);

            var state = DefaultAudioManager.Instance.GetSessionState(sessionId);
            if (state == null) return;

            var activePreset = ScpVoiceProfiles.GetPreset(scp);
            if (activePreset != null)
            {
                bool targetSpatialization = !activePreset.IsGlobalTransmission;

                if (state.IsSpatial != targetSpatialization)
                {
                    state.IsSpatial = targetSpatialization;

                    if (state.HasPhysicalSpeaker && state.PhysicalSpeaker != null)
                    {
                        state.PhysicalSpeaker.SetSpatialization(targetSpatialization);

                        if (!targetSpatialization)
                            state.PhysicalSpeaker.SetVolume(activePreset.OutputGain * 0.85f);
                    }
                }
            }
        }

        /// <summary>
        /// Synchronizes real-time world coordinate vectors for all registered active transmission targets.
        /// </summary>
        public void UpdatePositions()
        {
            lock (_stateLock)
            {
                if (_sessions.Count == 0) return;

                foreach (var kvp in _sessions)
                {
                    int playerId = kvp.Key;

                    Player scp = Player.ReadyList.FirstOrDefault(p => p.PlayerId == playerId);
                    if (scp == null || !scp.IsReady) continue;

                    DefaultAudioManager.Instance.SetSessionPosition(kvp.Value, scp.Position);
                }
            }
        }
    }
}