namespace SCP_Immersive_Voice.Managers
{
    using ScpImmersiveVoice.Config;
    using AudioManagerAPI.Defaults;
    using AudioManagerAPI.Features.Enums;
    using LabApi.Features.Wrappers;
    using System.Collections.Generic;
    using ScpImmersiveVoice;
    using LabApi.Features.Console;

    public class ScpVoiceManager
    {

        private readonly ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;

        public static ScpVoiceManager Instance { get; } = new ScpVoiceManager();
        private readonly Dictionary<Player, int> _sessions = new Dictionary<Player, int>();
        public int StartSession(Player scp)
        {
            if (_sessions.TryGetValue(scp, out int existing))
                return existing;

            int sessionId = DefaultAudioManager.Instance.CreateStreamSession(
                position: scp.Position,
                isSpatial: true,
                minDistance: 0.05f,
                maxDistance: _config.ProximityDistance,
                volume: 1f,
                priority: AudioPriority.High,
                validPlayersFilter: p => p != null && p.IsReady && p != scp
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
        public void AppendPcm(Player scp, float[] samples)
        {
            if (samples == null || samples.Length == 0)
            {
                Logger.Warn($"[SCP-VOICE] AppendPcm: EMPTY PCM from {scp.Nickname}");
                return;
            }

            if (!_sessions.TryGetValue(scp, out int sessionId))
            {
                Logger.Warn($"[SCP-VOICE] AppendPcm: NO SESSION for {scp.Nickname}, creating new one");
                sessionId = StartSession(scp);
            }

            DefaultAudioManager.Instance.AppendPcmData(sessionId, samples);

            var state = DefaultAudioManager.Instance.GetSessionState(sessionId);
            if (state == null)
            {
                Logger.Error($"[SCP-VOICE] AppendPcm: state NULL for session {sessionId}");
                return;
            }

            if (!state.HasPhysicalSpeaker || state.PhysicalSpeaker == null)
            {
                Logger.Warn($"[SCP-VOICE] AppendPcm: NO PHYSICAL SPEAKER for session {sessionId}");
            }
        }

        // Wrapper for Opus decoder
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
                var scp = kvp.Key;
                var sessionId = kvp.Value;

                DefaultAudioManager.Instance.SetSessionPosition(sessionId, scp.Position);
            }
        }
    }
}
