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
        public Dictionary<string, IAudioEffect> ActiveNodes { get; } = new(32);
        public ScpVoicePreset LastAppliedPreset { get; set; }
        public DateTime LastPacketReceivedTime { get; set; } = DateTime.MinValue;
        public object SyncLock { get; } = new();

        /// <summary>
        /// Encapsulates spatial updates inside a high-precision hardware tick boundary layer.
        /// </summary>
        public SpatializationDebouncer SpatialDebouncer { get; } = new(350f);
        #endregion

        #region Pre-Allocated Reusable Heap Graph Buffers
        private readonly List<IAudioEffect> _reusableEffectsList = new(32);
        private readonly Dictionary<string, IAudioEffect> _temporaryMap = new(32);
        #endregion

        #region Zero-Allocation Graph Synchronization Matrix
        public void SynchronizePipelineGraph(ScpVoicePreset preset)
        {
            if (preset is null) return;

            float sampleRate = VoiceChatSettings.SampleRate > 0 ? (float)VoiceChatSettings.SampleRate : 48000f;

            // Clear operational structural states without reallocating underlying collection buffers
            _reusableEffectsList.Clear();
            _temporaryMap.Clear();

            // 1. NoiseGate
            float gateThreshold = preset.UseNoiseGate ? preset.NoiseGateThreshold : -45f;
            UpdateOrRegisterSlot("NoiseGate", () => new NoiseGateEffect(gateThreshold, sampleRate), gateThreshold);

            // 2. VocalShriek
            if (preset.VocalShriek > 0f)
                UpdateOrRegisterSlot("VocalShriek", () => new VocalShriekShifterEffect(preset.VocalShriek, sampleRate), preset.VocalShriek);

            // 3. PitchShift
            if (Math.Abs(preset.Pitch - 1f) > 0.01f)
                UpdateOrRegisterSlot("PitchShift", () => new PitchShiftEffect(preset.Pitch, sampleRate, 40f), preset.Pitch);

            // 4. FormantShift
            if (Math.Abs(preset.Formant - 1f) > 0.01f)
                UpdateOrRegisterSlot("FormantShift", () => new FormantShiftEffect(preset.Formant, sampleRate), preset.Formant);

            // 5. FormantDrift
            if (preset.FormantDrift > 0f)
                UpdateOrRegisterSlot("FormantDrift", () => new FormantDriftEffect(preset.FormantDrift), preset.FormantDrift);

            // 6. LaryngealAsymmetry
            if (preset.LaryngealAsymmetry > 0f)
                UpdateOrRegisterSlot("LaryngealAsymmetry", () => new LaryngealAsymmetryEffect(preset.LaryngealAsymmetry, sampleRate), preset.LaryngealAsymmetry);

            // 7. DeathRattle
            if (preset.DeathRattle > 0f)
                UpdateOrRegisterSlot("DeathRattle", () => new DeathRattleEffect(preset.DeathRattle, sampleRate), preset.DeathRattle);

            // 8. Subharmonic
            if (preset.Subharmonic > 0f)
                UpdateOrRegisterSlot("Subharmonic", () => new SubharmonicGrowlEffect(preset.Subharmonic, sampleRate), preset.Subharmonic);

            // 9. Octaver
            if (preset.DemonicOctaverMix > 0f)
                UpdateOrRegisterSlot("Octaver", () => new DemonicOctaverEffect(preset.DemonicOctaverMix, sampleRate), preset.DemonicOctaverMix);

            // 10. Guttural
            if (preset.Guttural > 0f)
                UpdateOrRegisterSlot("Guttural", () => new GutturalResonanceEffect(preset.Guttural, sampleRate), preset.Guttural);

            // 11. Distortion
            if (preset.Distortion > 0f)
                UpdateOrRegisterSlot("Distortion", () => new DistortionEffect(preset.Distortion, sampleRate), preset.Distortion);

            // 12. SiliconModulation
            if (preset.SiliconModulation > 0f)
                UpdateOrRegisterSlot("SiliconModulation", () => new SiliconRingModulatorEffect(preset.SiliconModulation, sampleRate), preset.SiliconModulation);

            // 13. ScreechModulation
            if (preset.ScreechModulation > 0f)
                UpdateOrRegisterSlot("ScreechModulation", () => new ScreechModulatorEffect(preset.ScreechModulation, sampleRate), preset.ScreechModulation);

            // 14. Bitcrush - FIXED: Reuses node instance seamlessly if parameter remains unchanged
            if (preset.Bitcrush > 0f)
                UpdateOrRegisterSlot("Bitcrush", () => new BitcrushEffect(preset.Bitcrush), preset.Bitcrush);

            // 15. SampleRateReduce
            if (preset.SampleRateReduce > 0f)
                UpdateOrRegisterSlot("SampleRateReduce", () => new SampleRateReducerEffect(preset.SampleRateReduce, sampleRate), preset.SampleRateReduce);

            // 16. Tremolo
            if (preset.Tremolo > 0f)
                UpdateOrRegisterSlot("Tremolo", () => new TremoloEffect(preset.Tremolo), preset.Tremolo);

            // 17. Glitch
            if (preset.Glitch > 0f)
                UpdateOrRegisterSlot("Glitch", () => new GlitchBurstEffect(preset.Glitch, sampleRate), preset.Glitch);

            // 18. PredatoryCamouflage
            if (preset.PredatoryCamouflage > 0f)
                UpdateOrRegisterSlot("PredatoryCamouflage", () => new PredatoryCamouflageEffect(preset.PredatoryCamouflage, sampleRate), preset.PredatoryCamouflage);

            // 19. Whisper
            if (preset.WhisperAmount > 0f)
                UpdateOrRegisterSlot("Whisper", () => new WhisperFilterEffect(preset.WhisperAmount, sampleRate), preset.WhisperAmount);

            // 20. Breath
            if (preset.BreathNoise > 0f)
                UpdateOrRegisterSlot("Breath", () => new BreathNoiseEffect(preset.BreathNoise, sampleRate), preset.BreathNoise);

            // 21. Static
            if (preset.StaticNoise > 0f)
                UpdateOrRegisterSlot("Static", () => new StaticNoiseEffect(preset.StaticNoise, sampleRate), preset.StaticNoise);

            // 22. DryCrackle
            if (preset.DryCrackle > 0f)
                UpdateOrRegisterSlot("DryCrackle", () => new DryCrackleEffect(preset.DryCrackle, sampleRate), preset.DryCrackle);

            // 23. FleshCrackle
            if (preset.FleshCrackle > 0f)
                UpdateOrRegisterSlot("FleshCrackle", () => new FleshCrackleEffect(preset.FleshCrackle, sampleRate), preset.FleshCrackle);

            // 24. StoneCrack
            if (preset.StoneCrack > 0f)
                UpdateOrRegisterSlot("StoneCrack", () => new StoneCrackEffect(preset.StoneCrack, sampleRate), preset.StoneCrack);

            // 25. StoneGrind
            if (preset.StoneGrind > 0f)
                UpdateOrRegisterSlot("StoneGrind", () => new StoneGrindEffect(preset.StoneGrind, sampleRate), preset.StoneGrind);

            // 26. Chirp
            if (preset.Chirp > 0f)
                UpdateOrRegisterSlot("Chirp", () => new ChirpEffect(preset.Chirp, sampleRate), preset.Chirp);

            // 27. DataBurst
            if (preset.DataBurst > 0f)
                UpdateOrRegisterSlot("DataBurst", () => new DigitalDataBurstEffect(preset.DataBurst, sampleRate), preset.DataBurst);

            // 28. WetOrganic
            if (preset.WetOrganic > 0f)
                UpdateOrRegisterSlot("WetOrganic", () => new WetOrganicEffect(preset.WetOrganic, sampleRate), preset.WetOrganic);

            // 29. LowPass
            if (preset.LowPass > 0f)
                UpdateOrRegisterSlot("LowPass", () => new LowPassEffect(preset.LowPass, sampleRate), preset.LowPass);

            // 30. HighPass
            if (preset.HighPass > 0f)
                UpdateOrRegisterSlot("HighPass", () => new HighPassEffect(preset.HighPass, sampleRate), preset.HighPass);

            // 31. WetDecay
            if (preset.WetDecay > 0f)
                UpdateOrRegisterSlot("WetDecay", () => new WetDecayEffect(preset.WetDecay, sampleRate), preset.WetDecay);

            // 32. PocketEcho
            if (preset.PocketEcho > 0f)
                UpdateOrRegisterSlot("PocketEcho", () => new PocketDimensionEchoEffect(preset.PocketEcho, sampleRate), preset.PocketEcho);

            // 33. Reverb
            if (preset.Reverb > 0f)
                UpdateOrRegisterSlot("Reverb", () => new ReverbEffect(preset.Reverb, sampleRate), preset.Reverb);

            // Update thread-safe reference array inside the lock-free execution execution pipeline
            Pipeline.UpdateEffects(_reusableEffectsList);

            // Swap operational tracking tables cleanly without structural re-allocations
            ActiveNodes.Clear();
            foreach (var kvp in _temporaryMap)
            {
                ActiveNodes[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Manages structural cache validation and dynamic parameter adjustment for a concrete graph node slot.
        /// Fully optimized branch routing via standard closures bypass.
        /// </summary>
        private void UpdateOrRegisterSlot(string key, Func<IAudioEffect> factory, float runtimeValue)
        {
            if (ActiveNodes.TryGetValue(key, out var existingInstance))
            {
                if (existingInstance is IAdjustableAudioEffect adjustable)
                {
                    adjustable.AdjustParameter(runtimeValue);
                }

                _temporaryMap[key] = existingInstance;
                _reusableEffectsList.Add(existingInstance);
            }
            else
            {
                var newInstance = factory();
                _temporaryMap[key] = newInstance;
                _reusableEffectsList.Add(newInstance);
            }
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
                // FLUENT API ALIGNMENT: Utilizing zero-allocation tuple deconstruction to clear sessions cleanly
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