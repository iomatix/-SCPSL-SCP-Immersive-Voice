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
    using System.Collections.Generic;
    using System.Reflection;
    using System.Diagnostics;
    using SCP_Immersive_Voice.AudioProcessing;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using SCP_Immersive_Voice.AudioProcessing.Effects;
    using SCP_Immersive_Voice.Presets;

    /// <summary>
    /// Thread-safe, allocation-free temporal debouncer protecting underlying native 
    /// audio circular buffers from high-frequency network packet jitter.
    /// </summary>
    public class SpatializationDebouncer
    {
        private readonly long _hysteresisTicks;
        private long _lastTransitionTimestamp;
        private bool _currentSpatializedState;

        public SpatializationDebouncer(float minimumHysteresisMs = 350f)
        {
            _hysteresisTicks = (long)((minimumHysteresisMs / 1000f) * Stopwatch.Frequency);
            _lastTransitionTimestamp = 0;
            _currentSpatializedState = false;
        }

        // INTENT: Guard state changes against low-overhead hardware tick boundaries to eliminate rapid packet bouncing.
        public bool UpdateState(bool targetSpatialized)
        {
            if (targetSpatialized == _currentSpatializedState)
            {
                return _currentSpatializedState;
            }

            long currentTicks = Stopwatch.GetTimestamp();
            if (_lastTransitionTimestamp == 0 || (currentTicks - _lastTransitionTimestamp) >= _hysteresisTicks)
            {
                _currentSpatializedState = targetSpatialized;
                _lastTransitionTimestamp = currentTicks;
            }

            return _currentSpatializedState;
        }

        public void Reset(bool initialState = false)
        {
            _currentSpatializedState = initialState;
            _lastTransitionTimestamp = 0;
        }
    }

    /// <summary>
    /// Stateful container wrapping hardware network stream bindings 
    /// alongside a dedicated, isolated float-native DSP pipeline instance.
    /// </summary>
    public class VoiceSession
    {
        public int SessionId { get; set; }
        public AudioEffectPipeline Pipeline { get; } = new AudioEffectPipeline();
        public Dictionary<string, IAudioEffect> ActiveNodes { get; } = new Dictionary<string, IAudioEffect>();
        public ScpVoicePreset LastAppliedPreset { get; set; } = null;
        public DateTime LastPacketReceivedTime { get; set; } = DateTime.MinValue;
        public readonly object SyncLock = new object();

        // INTENT: Encapsulate spatial updates inside a high-precision hardware tick boundary layer.
        public SpatializationDebouncer SpatialDebouncer { get; } = new SpatializationDebouncer(350f);

        private readonly static ConcurrentDictionary<(Type, string), FieldInfo> _fieldCache =
            new ConcurrentDictionary<(Type, string), FieldInfo>();

        /// <summary>
        /// Rebuilds the DSP graph offline and executes an atomic transfer into the active pipeline runtime.
        /// </summary>
        public void SynchronizePipelineGraph(ScpVoicePreset preset)
        {
            float sr = (float)VoiceChat.VoiceChatSettings.SampleRate;
            if (sr <= 0) sr = 48000f;

            var targetNodes = new List<(string Key, Func<IAudioEffect> Factory, float ScalarValue, string ScalarFieldName)>();

            float gateThreshold = preset.UseNoiseGate ? preset.NoiseGateThreshold : -45f;
            float clampedDb = gateThreshold < -96f ? -96f : (gateThreshold > 0f ? 0f : gateThreshold);
            float thresholdLinear = (float)Math.Pow(10, clampedDb / 20.0);

            // INTENT: Convert the raw dB threshold into a pre-squared linear value offline to ensure the 
            // reflection field matches the expectations of the float-native RMS loop seamlessly.
            float thresholdLinearSquared = thresholdLinear * thresholdLinear;
            targetNodes.Add(("NoiseGate", () => new NoiseGateEffect(gateThreshold, sr), thresholdLinearSquared, "_thresholdLinearSquared"));

            if (preset.VocalShriek > 0f)
                targetNodes.Add(("VocalShriek", () => new VocalShriekShifterEffect(preset.VocalShriek, sr), preset.VocalShriek, "_amount"));

            if (Math.Abs(preset.Pitch - 1f) > 0.01f)
                targetNodes.Add(("PitchShift", () => new PitchShiftEffect(preset.Pitch, sr, 40f), preset.Pitch, "_pitch"));

            if (Math.Abs(preset.Formant - 1f) > 0.01f)
                targetNodes.Add(("FormantShift", () => new FormantShiftEffect(preset.Formant, sr), preset.Formant, "_formant"));

            if (preset.FormantDrift > 0f)
                targetNodes.Add(("FormantDrift", () => new FormantDriftEffect(preset.FormantDrift), preset.FormantDrift, "_amount"));

            if (preset.LaryngealAsymmetry > 0f)
                targetNodes.Add(("LaryngealAsymmetry", () => new LaryngealAsymmetryEffect(preset.LaryngealAsymmetry, sr), preset.LaryngealAsymmetry, "_amount"));

            if (preset.DeathRattle > 0f)
                targetNodes.Add(("DeathRattle", () => new DeathRattleEffect(preset.DeathRattle, sr), preset.DeathRattle, "_amount"));

            if (preset.Subharmonic > 0f)
                targetNodes.Add(("Subharmonic", () => new SubharmonicGrowlEffect(preset.Subharmonic, sr), preset.Subharmonic, "_amount"));

            if (preset.DemonicOctaverMix > 0f)
                targetNodes.Add(("Octaver", () => new DemonicOctaverEffect(preset.DemonicOctaverMix, sr), preset.DemonicOctaverMix, "_mix"));

            if (preset.Guttural > 0f)
                targetNodes.Add(("Guttural", () => new GutturalResonanceEffect(preset.Guttural, sr), preset.Guttural, "_amount"));

            if (preset.Distortion > 0f)
                targetNodes.Add(("Distortion", () => new DistortionEffect(preset.Distortion, sr), preset.Distortion, "_amount"));

            if (preset.SiliconModulation > 0f)
                targetNodes.Add(("SiliconModulation", () => new SiliconRingModulatorEffect(preset.SiliconModulation, sr), preset.SiliconModulation, "_amount"));

            if (preset.ScreechModulation > 0f)
                targetNodes.Add(("ScreechModulation", () => new ScreechModulatorEffect(preset.ScreechModulation, sr), preset.ScreechModulation, "_amount"));

            if (preset.Bitcrush > 0f)
                targetNodes.Add(("Bitcrush", () => new BitcrushEffect(preset.Bitcrush), preset.Bitcrush, null));

            if (preset.SampleRateReduce > 0f)
                targetNodes.Add(("SampleRateReduce", () => new SampleRateReducerEffect(preset.SampleRateReduce, sr), preset.SampleRateReduce, null));

            if (preset.Tremolo > 0f)
                targetNodes.Add(("Tremolo", () => new TremoloEffect(preset.Tremolo), preset.Tremolo, "_amount"));

            if (preset.Glitch > 0f)
                targetNodes.Add(("Glitch", () => new GlitchBurstEffect(preset.Glitch, sr), preset.Glitch, null));

            if (preset.PredatoryCamouflage > 0f)
                targetNodes.Add(("PredatoryCamouflage", () => new PredatoryCamouflageEffect(preset.PredatoryCamouflage, sr), preset.PredatoryCamouflage, "_amount"));

            if (preset.WhisperAmount > 0f)
                targetNodes.Add(("Whisper", () => new WhisperFilterEffect(preset.WhisperAmount, sr), preset.WhisperAmount, "_amount"));

            if (preset.BreathNoise > 0f)
                targetNodes.Add(("Breath", () => new BreathNoiseEffect(preset.BreathNoise, sr), preset.BreathNoise, "_intensity"));

            if (preset.StaticNoise > 0f)
                targetNodes.Add(("Static", () => new StaticNoiseEffect(preset.StaticNoise, sr), preset.StaticNoise, "_amount"));

            if (preset.DryCrackle > 0f)
                targetNodes.Add(("DryCrackle", () => new DryCrackleEffect(preset.DryCrackle, sr), preset.DryCrackle, "_amount"));

            if (preset.FleshCrackle > 0f)
                targetNodes.Add(("FleshCrackle", () => new FleshCrackleEffect(preset.FleshCrackle, sr), preset.FleshCrackle, "_amount"));

            if (preset.StoneCrack > 0f)
                targetNodes.Add(("StoneCrack", () => new StoneCrackEffect(preset.StoneCrack, sr), preset.StoneCrack, null));

            if (preset.StoneGrind > 0f)
                targetNodes.Add(("StoneGrind", () => new StoneGrindEffect(preset.StoneGrind, sr), preset.StoneGrind, null));

            if (preset.Chirp > 0f)
                targetNodes.Add(("Chirp", () => new ChirpEffect(preset.Chirp, sr), preset.Chirp, null));

            if (preset.DataBurst > 0f)
                targetNodes.Add(("DataBurst", () => new DigitalDataBurstEffect(preset.DataBurst, sr), preset.DataBurst, null));

            if (preset.WetOrganic > 0f)
                targetNodes.Add(("WetOrganic", () => new WetOrganicEffect(preset.WetOrganic, sr), preset.WetOrganic, "_amount"));

            if (preset.LowPass > 0f)
                targetNodes.Add(("LowPass", () => new LowPassEffect(preset.LowPass, sr), preset.LowPass, null));

            if (preset.HighPass > 0f)
                targetNodes.Add(("HighPass", () => new HighPassEffect(preset.HighPass, sr), preset.HighPass, null));

            if (preset.WetDecay > 0f)
                targetNodes.Add(("WetDecay", () => new WetDecayEffect(preset.WetDecay, sr), preset.WetDecay, "_amount"));

            if (preset.PocketEcho > 0f)
                targetNodes.Add(("PocketEcho", () => new PocketDimensionEchoEffect(preset.PocketEcho, sr), preset.PocketEcho, "_amount"));

            if (preset.Reverb > 0f)
                targetNodes.Add(("Reverb", () => new ReverbEffect(preset.Reverb, sr), preset.Reverb, "_amount"));

            var temporaryMap = new Dictionary<string, IAudioEffect>();
            var updatedEffects = new List<IAudioEffect>();

            foreach (var target in targetNodes)
            {
                if (ActiveNodes.TryGetValue(target.Key, out var existingInstance) && target.ScalarFieldName != null)
                {
                    FastInjectScalarField(existingInstance, target.ScalarFieldName, target.ScalarValue);
                    temporaryMap[target.Key] = existingInstance;
                    updatedEffects.Add(existingInstance);
                }
                else
                {
                    var newInstance = target.Factory();
                    temporaryMap[target.Key] = newInstance;
                    updatedEffects.Add(newInstance);
                }
            }

            Pipeline.UpdateEffects(updatedEffects);

            ActiveNodes.Clear();
            foreach (var kvp in temporaryMap)
            {
                ActiveNodes[kvp.Key] = kvp.Value;
            }
        }

        private static void FastInjectScalarField(object instance, string fieldName, float value)
        {
            try
            {
                var type = instance.GetType();
                var field = _fieldCache.GetOrAdd((type, fieldName), key =>
                    key.Item1.GetField(key.Item2, BindingFlags.NonPublic | BindingFlags.Instance));

                if (field != null)
                {
                    field.SetValue(instance, value);
                }
            }
            catch { }
        }
    }

    public class ScpVoiceManager
    {
        private readonly ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;
        private readonly ConcurrentDictionary<int, VoiceSession> _sessions = new ConcurrentDictionary<int, VoiceSession>();
        private readonly object _allocationLock = new object();

        public VoiceSession StartSession(Player scp)
        {
            if (scp == null) return null;

            if (_sessions.TryGetValue(scp.PlayerId, out var existing))
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

                if (sessionId == 0) return null;

                var session = new VoiceSession { SessionId = sessionId };
                _sessions[scp.PlayerId] = session;

                Logger.Debug($"[VOICE HARDENING] Session REGISTERED. PlayerId: {scp.PlayerId}, SessionId: {sessionId}");
                return session;
            }
        }

        public void StopSession(Player scp)
        {
            if (scp == null) return;
            var key = scp.PlayerId;

            if (_sessions.TryRemove(key, out var session))
            {
                DefaultAudioManager.Instance.DestroySession(session.SessionId);
                Logger.Debug($"[VOICE HARDENING] Audio Streaming Session no. {session.SessionId} successfully destroyed for {scp.Nickname}");
            }
        }

        public void StopAllSessions()
        {
            foreach (var kvp in _sessions)
            {
                try { DefaultAudioManager.Instance.DestroySession(kvp.Value.SessionId); } catch { }
            }
            Logger.Debug("[VOICE HARDENING] All active audio streaming slots cleared from native heap.");
            _sessions.Clear();
        }

        public void AppendPcm(Player scp, float[] samples)
        {
            if (scp == null || samples == null || samples.Length == 0) return;

            if (!_sessions.TryGetValue(scp.PlayerId, out var session))
            {
                session = StartSession(scp);
            }

            if (session == null) return;

            DefaultAudioManager.Instance.AppendPcmData(session.SessionId, samples);
        }

        public void UpdatePositions()
        {
            if (_sessions.IsEmpty) return;

            foreach (var kvp in _sessions)
            {
                int playerId = kvp.Key;
                var session = kvp.Value;

                Player scp = Player.Get(playerId);
                if (scp == null || !scp.IsReady)
                {
                    if (_sessions.TryRemove(playerId, out var deadSession))
                    {
                        try { DefaultAudioManager.Instance.DestroySession(deadSession.SessionId); } catch { }
                        Logger.Warn($"[VOICE SECURITY] Pruned voice session {deadSession.SessionId} for disconnected PlayerId: {playerId}");
                    }
                    continue;
                }

                DefaultAudioManager.Instance.SetSessionPosition(session.SessionId, scp.Position);

                var state = DefaultAudioManager.Instance.GetSessionState(session.SessionId);
                if (state == null) continue;

                var activePreset = ScpVoiceProfiles.GetPreset(scp);
                if (activePreset != null)
                {
                    bool targetSpatialization = !activePreset.IsGlobalTransmission;

                    // INTENT: Route state requests into the OOP debouncer primitive to shelter downstream native buffers from network flushes.
                    bool debouncedSpatialization = session.SpatialDebouncer.UpdateState(targetSpatialization);

                    if (state.IsSpatial != debouncedSpatialization)
                    {
                        state.IsSpatial = debouncedSpatialization;

                        if (state.HasPhysicalSpeaker && state.PhysicalSpeaker != null)
                        {
                            state.PhysicalSpeaker.SetSpatialization(debouncedSpatialization);

                            if (!debouncedSpatialization)
                                state.PhysicalSpeaker.SetVolume(activePreset.OutputGain * 0.85f);
                        }
                    }
                }
            }
        }
    }
}