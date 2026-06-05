namespace SCP_Immersive_Voice.Managers
{
    using AudioManagerAPI.Defaults;
    using AudioManagerAPI.Features.Enums;
    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.VoiceProfiles;
    using ScpImmersiveVoice;
    using ScpImmersiveVoice.Config;
    using System.Collections.Generic;
    using System.Linq;

    public class ScpVoiceManager
    {
        private readonly ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;

        /// <summary>
        /// Audio sessions: key = PlayerId, value = sessionId
        /// </summary>
        private readonly Dictionary<int, int> _sessions = new Dictionary<int, int>();

        /// <summary>
        /// Per-player locks to prevent duplicate StartSession
        /// </summary>
        private readonly Dictionary<int, object> _locks = new Dictionary<int, object>();

        private object GetLock(int playerId)
        {
            if (!_locks.TryGetValue(playerId, out var l))
            {
                l = new object();
                _locks[playerId] = l;
            }
            return l;
        }

        public int StartSession(Player scp)
        {
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

            _sessions[scp.PlayerId] = sessionId;

            Logger.Debug($"[VOICE DEBUG] Session ADDED to dictionary. PlayerId: {scp.PlayerId}, SessionId: {sessionId}");
            Logger.Debug($"[VOICE DEBUG] Total sessions in dictionary: {_sessions.Count}");
            Logger.Debug($"[VOICE DEBUG] Session created for player: {scp.Nickname}, SessionId: {sessionId}");

            return sessionId;
        }

        public void StopSession(Player scp)
        {
            Logger.Debug($"[VOICE DEBUG] StopSession called for player: {scp.Nickname}, PlayerId: {scp.PlayerId}");
            Logger.Debug($"[VOICE DEBUG] Current sessions: {string.Join(", ", _sessions.Keys)}");

            var key = scp.PlayerId;

            lock (GetLock(key))
            {
                if (!_sessions.TryGetValue(key, out int sessionId))
                {
                    Logger.Debug($"[VOICE DEBUG] Session NOT found for player: {scp.Nickname}");
                    return;
                }

                DefaultAudioManager.Instance.DestroySession(sessionId);

                Logger.Debug($"[VOICE DEBUG] Audio Streaming Seassion no. {sessionId} Destroyed");
                _sessions.Remove(key);
            }
        }

        public void StopAllSessions()
        {
            foreach (var kvp in _sessions)
                DefaultAudioManager.Instance.DestroySession(kvp.Value);

            Logger.Debug("[VOICE DEBUG] All Audio Streaming Seassions Destroyed");
            _sessions.Clear();
            _locks.Clear();
        }

        public void AppendPcm(Player scp, float[] samples)
        {
            if (scp == null || samples == null || samples.Length == 0)
            {
                return;
            }

            var key = scp.PlayerId;

            lock (GetLock(key))
            {
                if (!_sessions.TryGetValue(key, out int sessionId))
                {
                    Logger.Debug($"[SCP-VOICE] AppendPcm: NO SESSION for {scp.Nickname}, creating new one");
                    sessionId = StartSession(scp);
                }

                // 1. Append raw samples directly into the real-time hardware buffer
                DefaultAudioManager.Instance.AppendPcmData(sessionId, samples);

                // 2. Resolve the active speaker state context from our framework API
                var state = DefaultAudioManager.Instance.GetSessionState(sessionId);
                if (state == null)
                {
                    Logger.Error($"[SCP-VOICE] AppendPcm: state NULL for session {sessionId}");
                    return;
                }

                // 3. FETCH PRESET & EVALUATE SPATIALIZATION RULES DIRECTLY AT THE SPEAKER BOUNDARY
                var activePreset = ScpVoiceProfiles.GetPreset(scp);
                if (activePreset != null)
                {
                    // If global transmission is requested, IsSpatial must be FALSE (2D Full-Map Broadcast)
                    bool targetSpatialization = !activePreset.IsGlobalTransmission;

                    if (state.IsSpatial != targetSpatialization)
                    {
                        state.IsSpatial = targetSpatialization;

                        // Force live update on the physical hardware worker loop if allocated
                        if (state.HasPhysicalSpeaker && state.PhysicalSpeaker != null)
                        {
                            state.PhysicalSpeaker.SetSpatialization(targetSpatialization);

                            // Optional optimization: balance volume scaling for non-spatial projection
                            if (!targetSpatialization)
                                state.PhysicalSpeaker.SetVolume(activePreset.OutputGain * 0.85f);
                        }
                    }
                }

                if (!state.HasPhysicalSpeaker || state.PhysicalSpeaker == null)
                {
                    Logger.Warn($"[SCP-VOICE] AppendPcm: NO PHYSICAL SPEAKER for session {sessionId}");
                }
            }
        }

        public void AppendPcm(Player scp, short[] pcm)
        {
            if (pcm == null || pcm.Length == 0)
                return;

            float[] samples = new float[pcm.Length];
            const float inv = 1f / 32768f;

            for (int i = 0; i < pcm.Length; i++)
                samples[i] = pcm[i] * inv;

            AppendPcm(scp, samples);
        }

        public void UpdatePositions()
        {
            foreach (var kvp in _sessions)
            {
                int playerId = kvp.Key;

                Player scp = Player.ReadyList.FirstOrDefault(p => p.PlayerId == playerId);
                if (scp == null) continue;

                var sessionId = kvp.Value;
                DefaultAudioManager.Instance.SetSessionPosition(sessionId, scp.Position);
            }
        }
    }
}
