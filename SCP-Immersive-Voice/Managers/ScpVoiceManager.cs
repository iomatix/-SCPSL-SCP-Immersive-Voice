namespace SCP_Immersive_Voice.Managers
{
    using AudioManagerAPI.Defaults;
    using AudioManagerAPI.Features.Enums;
    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.VoiceProfiles;
    using ScpImmersiveVoice.Config;
    using ScpImmersiveVoice;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    public class ScpVoiceManager
    {
        private readonly ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;

        private readonly ConcurrentDictionary<int, int> _sessions = new ConcurrentDictionary<int, int>();
        private readonly object _allocationLock = new object();

        public int StartSession(Player scp)
        {
            if (scp == null) return 0;

            if (_sessions.TryGetValue(scp.PlayerId, out int existing))
                return existing;

            lock (_allocationLock)
            {
                if (_sessions.TryGetValue(scp.PlayerId, out existing))
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

        public void StopSession(Player scp)
        {
            if (scp == null) return;
            var key = scp.PlayerId;

            if (_sessions.TryRemove(key, out int sessionId))
            {
                DefaultAudioManager.Instance.DestroySession(sessionId);
                Logger.Debug($"[VOICE HARDENING] Audio Streaming Session no. {sessionId} successfully destroyed for {scp.Nickname}");
            }
        }

        public void StopAllSessions()
        {
            foreach (var kvp in _sessions)
            {
                try { DefaultAudioManager.Instance.DestroySession(kvp.Value); } catch { }
            }
            Logger.Debug("[VOICE HARDENING] All active audio streaming slots cleared from native heap.");
            _sessions.Clear();
        }

        public void AppendPcm(Player scp, float[] samples)
        {
            if (scp == null || samples == null || samples.Length == 0) return;

            if (!_sessions.TryGetValue(scp.PlayerId, out int sessionId))
            {
                sessionId = StartSession(scp);
            }

            if (sessionId == 0) return;

            DefaultAudioManager.Instance.AppendPcmData(sessionId, samples);
        }

        public void UpdatePositions()
        {
            if (_sessions.IsEmpty) return;

            foreach (var kvp in _sessions)
            {
                int playerId = kvp.Key;
                int sessionId = kvp.Value;

                Player scp = Player.Get(playerId);
                if (scp == null || !scp.IsReady)
                {
                    if (_sessions.TryRemove(playerId, out int deadSessionId))
                    {
                        try { DefaultAudioManager.Instance.DestroySession(deadSessionId); } catch { }
                        Logger.Warn($"[VOICE SECURITY] Pruned voice session {deadSessionId} for disconnected PlayerId: {playerId}");
                    }
                    continue;
                }

                DefaultAudioManager.Instance.SetSessionPosition(sessionId, scp.Position);

                var state = DefaultAudioManager.Instance.GetSessionState(sessionId);
                if (state == null) continue;

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
        }
    }
}