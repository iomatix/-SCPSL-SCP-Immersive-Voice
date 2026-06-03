namespace SCP_Immersive_Voice.Managers
{
    using ScpImmersiveVoice.Config;
    using AudioManagerAPI.Defaults;
    using AudioManagerAPI.Features.Enums;
    using LabApi.Features.Wrappers;
    using System.Collections.Generic;
    using ScpImmersiveVoice;

    public class ScpVoiceManager
    {

        private readonly ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;

        public static ScpVoiceManager Instance { get; } = new ScpVoiceManager();
        private readonly Dictionary<Player, int> _sessions = new Dictionary<Player, int>();
        public int StartSession(Player scp)
        {
            if (_sessions.TryGetValue(scp, out int existing))
                return existing;

            int sessionId = DefaultAudioManager.Instance.PlayAudio(
                key: "scp_voice_" + scp.PlayerId,
                position: scp.Position,
                loop: false,
                volume: 1f,
                minDistance: 0.05f,
                maxDistance: _config.ProximityDistance,
                isSpatial: true,
                priority: AudioPriority.High,
                validPlayersFilter: p => p.PlayerId != scp.PlayerId
            );

            _sessions[scp] = sessionId;
            return sessionId;
        }

        public void StopSession(Player scp)
        {
            if (!_sessions.TryGetValue(scp, out int sessionId))
                return;

            DefaultAudioManager.Instance.DestroySession(sessionId);
            _sessions.Remove(scp);
        }

        public void StopAllSessions()
        {
            foreach (var kvp in _sessions) DefaultAudioManager.Instance.DestroySession(kvp.Value);

            _sessions.Clear();
        }

        public void AppendPcm(Player scp, short[] pcm)
        {
            if (!_sessions.TryGetValue(scp, out int sessionId))
                sessionId = StartSession(scp);

            DefaultAudioManager.Instance.AppendPcmData(sessionId, pcm);
        }

        public void UpdatePositions()
        {
            foreach (var kvp in _sessions)
            {
                var scp = kvp.Key;
                var sessionId = kvp.Value;

                DefaultAudioManager.Instance.SetSessionPosition(sessionId, scp.Position);
            }
        }
    }
}
