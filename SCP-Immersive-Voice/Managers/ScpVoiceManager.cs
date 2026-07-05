using AudioManagerAPI.Defaults;
using AudioManagerAPI.Features.Enums;
using LabApi.Extensions;
using LabApi.Extensions.Misc;
using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.AudioProcessing;
using SCP_Immersive_Voice.AudioProcessing.Effects;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using SCP_Immersive_Voice.AudioProcessing.Utils;
using SCP_Immersive_Voice.Presets;
using SCP_Immersive_Voice.VoiceProfiles;
using ScpImmersiveVoice;
using ScpImmersiveVoice.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using VoiceChat;

namespace SCP_Immersive_Voice.Managers
{

    /// <summary>
    /// Stateful container wrapping hardware network stream bindings 
    /// alongside a dedicated, isolated float-native DSP pipeline instance.
    /// </summary>
    public class VoiceSession
    {
        #region Operational Properties
        public int SessionId { get; set; }
        public AudioEffectPipeline Pipeline { get; } = new();
        public Dictionary<string, IAudioEffect> ActiveNodes { get; } = new();
        public ScpVoicePreset LastAppliedPreset { get; set; }
        public DateTime LastPacketReceivedTime { get; set; } = DateTime.MinValue;
        public object SyncLock { get; } = new();

        /// <summary>
        /// Encapsulates spatial updates inside a high-precision hardware tick boundary layer.
        /// Teraz bezpiecznie i poprawnie referuje do typu z warstwy /Utils/
        /// </summary>
        public SpatializationDebouncer SpatialDebouncer { get; } = new(350f);
        #endregion

        #region Thread-Safe Reflection Cache
        private static readonly ConcurrentDictionary<(Type, string), FieldInfo> FieldCache = new();
        #endregion

        #region Graph Synchronization Matrix
        public void SynchronizePipelineGraph(ScpVoicePreset preset)
        {
            if (preset is null) return;

            float sampleRate = VoiceChatSettings.SampleRate > 0 ? (float)VoiceChatSettings.SampleRate : 48000f;
            var targetNodes = new List<(string Key, Func<IAudioEffect> Factory, float ScalarValue, string ScalarFieldName)>();

            float gateThreshold = preset.UseNoiseGate ? preset.NoiseGateThreshold : -45f;
            float clampedDb = gateThreshold < -96f ? -96f : (gateThreshold > 0f ? 0f : gateThreshold);
            float thresholdLinear = (float)Math.Pow(10, clampedDb / 20.0);
            float thresholdLinearSquared = thresholdLinear * thresholdLinear;

            targetNodes.Add(("NoiseGate", () => new NoiseGateEffect(gateThreshold, sampleRate), thresholdLinearSquared, "_thresholdLinearSquared"));

            if (preset.VocalShriek > 0f) targetNodes.Add(("VocalShriek", () => new VocalShriekShifterEffect(preset.VocalShriek, sampleRate), preset.VocalShriek, "_amount"));
            if (Math.Abs(preset.Pitch - 1f) > 0.01f) targetNodes.Add(("PitchShift", () => new PitchShiftEffect(preset.Pitch, sampleRate, 40f), preset.Pitch, "_pitch"));
            if (Math.Abs(preset.Formant - 1f) > 0.01f) targetNodes.Add(("FormantShift", () => new FormantShiftEffect(preset.Formant, sampleRate), preset.Formant, "_formant"));
            if (preset.FormantDrift > 0f) targetNodes.Add(("FormantDrift", () => new FormantDriftEffect(preset.FormantDrift), preset.FormantDrift, "_amount"));
            if (preset.LaryngealAsymmetry > 0f) targetNodes.Add(("LaryngealAsymmetry", () => new LaryngealAsymmetryEffect(preset.LaryngealAsymmetry, sampleRate), preset.LaryngealAsymmetry, "_amount"));
            if (preset.DeathRattle > 0f) targetNodes.Add(("DeathRattle", () => new DeathRattleEffect(preset.DeathRattle, sampleRate), preset.DeathRattle, "_amount"));
            if (preset.Subharmonic > 0f) targetNodes.Add(("Subharmonic", () => new SubharmonicGrowlEffect(preset.Subharmonic, sampleRate), preset.Subharmonic, "_amount"));
            if (preset.DemonicOctaverMix > 0f) targetNodes.Add(("Octaver", () => new DemonicOctaverEffect(preset.DemonicOctaverMix, sampleRate), preset.DemonicOctaverMix, "_mix"));
            if (preset.Guttural > 0f) targetNodes.Add(("Guttural", () => new GutturalResonanceEffect(preset.Guttural, sampleRate), preset.Guttural, "_amount"));
            if (preset.Distortion > 0f) targetNodes.Add(("Distortion", () => new DistortionEffect(preset.Distortion, sampleRate), preset.Distortion, "_amount"));
            if (preset.SiliconModulation > 0f) targetNodes.Add(("SiliconModulation", () => new SiliconRingModulatorEffect(preset.SiliconModulation, sampleRate), preset.SiliconModulation, "_amount"));
            if (preset.ScreechModulation > 0f) targetNodes.Add(("ScreechModulation", () => new ScreechModulatorEffect(preset.ScreechModulation, sampleRate), preset.ScreechModulation, "_amount"));
            if (preset.Bitcrush > 0f) targetNodes.Add(("Bitcrush", () => new BitcrushEffect(preset.Bitcrush), preset.Bitcrush, null));
            if (preset.SampleRateReduce > 0f) targetNodes.Add(("SampleRateReduce", () => new SampleRateReducerEffect(preset.SampleRateReduce, sampleRate), preset.SampleRateReduce, null));
            if (preset.Tremolo > 0f) targetNodes.Add(("Tremolo", () => new TremoloEffect(preset.Tremolo), preset.Tremolo, "_amount"));
            if (preset.Glitch > 0f) targetNodes.Add(("Glitch", () => new GlitchBurstEffect(preset.Glitch, sampleRate), preset.Glitch, null));
            if (preset.PredatoryCamouflage > 0f) targetNodes.Add(("PredatoryCamouflage", () => new PredatoryCamouflageEffect(preset.PredatoryCamouflage, sampleRate), preset.PredatoryCamouflage, "_amount"));
            if (preset.WhisperAmount > 0f) targetNodes.Add(("Whisper", () => new WhisperFilterEffect(preset.WhisperAmount, sampleRate), preset.WhisperAmount, "_amount"));
            if (preset.BreathNoise > 0f) targetNodes.Add(("Breath", () => new BreathNoiseEffect(preset.BreathNoise, sampleRate), preset.BreathNoise, "_intensity"));
            if (preset.StaticNoise > 0f) targetNodes.Add(("Static", () => new StaticNoiseEffect(preset.StaticNoise, sampleRate), preset.StaticNoise, "_amount"));
            if (preset.DryCrackle > 0f) targetNodes.Add(("DryCrackle", () => new DryCrackleEffect(preset.DryCrackle, sampleRate), preset.DryCrackle, "_amount"));
            if (preset.FleshCrackle > 0f) targetNodes.Add(("FleshCrackle", () => new FleshCrackleEffect(preset.FleshCrackle, sampleRate), preset.FleshCrackle, "_amount"));
            if (preset.StoneCrack > 0f) targetNodes.Add(("StoneCrack", () => new StoneCrackEffect(preset.StoneCrack, sampleRate), preset.StoneCrack, null));
            if (preset.StoneGrind > 0f) targetNodes.Add(("StoneGrind", () => new StoneGrindEffect(preset.StoneGrind, sampleRate), preset.StoneGrind, null));
            if (preset.Chirp > 0f) targetNodes.Add(("Chirp", () => new ChirpEffect(preset.Chirp, sampleRate), preset.Chirp, null));
            if (preset.DataBurst > 0f) targetNodes.Add(("DataBurst", () => new DigitalDataBurstEffect(preset.DataBurst, sampleRate), preset.DataBurst, null));
            if (preset.WetOrganic > 0f) targetNodes.Add(("WetOrganic", () => new WetOrganicEffect(preset.WetOrganic, sampleRate), preset.WetOrganic, "_amount"));
            if (preset.LowPass > 0f) targetNodes.Add(("LowPass", () => new LowPassEffect(preset.LowPass, sampleRate), preset.LowPass, null));
            if (preset.HighPass > 0f) targetNodes.Add(("HighPass", () => new HighPassEffect(preset.HighPass, sampleRate), preset.HighPass, null));
            if (preset.WetDecay > 0f) targetNodes.Add(("WetDecay", () => new WetDecayEffect(preset.WetDecay, sampleRate), preset.WetDecay, "_amount"));
            if (preset.PocketEcho > 0f) targetNodes.Add(("PocketEcho", () => new PocketDimensionEchoEffect(preset.PocketEcho, sampleRate), preset.PocketEcho, "_amount"));
            if (preset.Reverb > 0f) targetNodes.Add(("Reverb", () => new ReverbEffect(preset.Reverb, sampleRate), preset.Reverb, "_amount"));

            var temporaryMap = new Dictionary<string, IAudioEffect>();
            var updatedEffects = new List<IAudioEffect>();

            foreach (var target in targetNodes)
            {
                if (ActiveNodes.TryGetValue(target.Key, out var existingInstance) && target.ScalarFieldName is not null)
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

            foreach (var (key, effect) in temporaryMap)
            {
                ActiveNodes[key] = effect;
            }
        }

        private static void FastInjectScalarField(object instance, string fieldName, float value)
        {
            try
            {
                var type = instance.GetType();
                var field = FieldCache.GetOrAdd((type, fieldName), static key =>
                    key.Item1.GetField(key.Item2, BindingFlags.NonPublic | BindingFlags.Instance));

                field?.SetValue(instance, value);
            }
            catch { }
        }
        #endregion
    }

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

                int sessionId = DefaultAudioManager.Instance.CreateStreamSession(
                    position: scp.Position,
                    isSpatial: true,
                    minDistance: 4.25f,
                    maxDistance: _config.ProximityDistance,
                    volume: 1f,
                    priority: AudioPriority.High,
                    validPlayersFilter: p => p is not null && p.IsReady && p != scp
                );

                if (sessionId is 0) return null;

                var session = new VoiceSession { SessionId = sessionId };
                _sessions[scp.PlayerId] = session;

                iLogger.Debug(nameof(ScpVoiceManager), $"[VOICE HARDENING] Session REGISTERED. PlayerId: {scp.PlayerId}, SessionId: {sessionId}");
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
                iLogger.Debug(nameof(ScpVoiceManager), $"[VOICE HARDENING] Audio Streaming Session no. {session.SessionId} successfully destroyed for {scp.Nickname}");
            }
        }

        /// <summary>
        /// Systematically cuts all active audio channels from the native sound engine heap.
        /// </summary>
        public void StopAllSessions()
        {
            // FLUENT API ALIGNMENT: Utilizing zero-allocation tuple deconstruction to clear sessions cleanly
            foreach (var (_, session) in _sessions)
            {
                try { DefaultAudioManager.Instance.DestroySession(session.SessionId); } catch { }
            }

            iLogger.Debug(nameof(ScpVoiceManager), "[VOICE HARDENING] All active audio streaming slots cleared from native heap.");
            _sessions.Clear();
        }
        #endregion

        #region Data Transmission Pipelines
        /// <summary>
        /// Appends raw float PCM sample blocks directly into the stateful session execution pipeline.
        /// </summary>
        public void AppendPcm(Player scp, float[] samples)
        {
            if (scp is null || samples is null || samples.Length is 0) return;

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

            // Eradicated heavy KeyValuePair wrappers, streaming deconstructed primitives natively via framework extensions.
            // This drops memory allocation to exact ZERO during the server tick loop sweeps.
            foreach (var (playerId, session) in _sessions)
            {
                Player scp = Player.Get(playerId);
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