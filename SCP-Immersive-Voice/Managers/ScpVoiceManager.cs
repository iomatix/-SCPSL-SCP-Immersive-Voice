namespace SCP_Immersive_Voice.Managers
{
    using ScpImmersiveVoice.Config;
    using AudioManagerAPI.Defaults;
    using AudioManagerAPI.Features.Enums;
    using LabApi.Features.Wrappers;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using ScpImmersiveVoice;

    public class ScpVoiceManager
    {

        private readonly ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;

        public static ScpVoiceManager Instance { get; } = new ScpVoiceManager();
        private readonly Dictionary<Player, int> _sessions = new Dictionary<Player, int>();
        public void StartSession(Player scp)
        {
            if (_sessions.ContainsKey(scp))
                return;

            int sessionId = DefaultAudioManager.Instance.PlayAudio(
                key: "scp_voice_" + scp.PlayerId,
                position: scp.Position,
                loop: false,
                volume: 1f,
                minDistance: 0.5f,
                maxDistance: _config.ProximityDistance,
                isSpatial: true,
                priority: AudioPriority.High,
                validPlayersFilter: p => p != scp
            );

            _sessions[scp] = sessionId;
        }

        public void StopSession(Player scp)
        {
            if (!_sessions.TryGetValue(scp, out int sessionId))
                return;

            DefaultAudioManager.Instance.Stop(sessionId);
            _sessions.Remove(scp);
        }

        public void AppendPcm(Player scp, short[] pcm)
        {
            if (!_sessions.TryGetValue(scp, out int sessionId))
                StartSession(scp);

            DefaultAudioManager.Instance.AppendPcmData(sessionId, pcm);
        }

        public void UpdatePositions()
        {
            foreach (var kvp in _sessions)
            {
                var scp = kvp.Key;
                var sessionId = kvp.Value;

                DefaultAudioManager.Instance.UpdatePosition(sessionId, scp.Position);
            }
        }
    }
}
