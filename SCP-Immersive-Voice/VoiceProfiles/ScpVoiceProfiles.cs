namespace SCP_Immersive_Voice.VoiceProfiles
{
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.AudioProcessing;
    using SCP_Immersive_Voice.Managers;
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Interfaces;
    using ScpImmersiveVoice;
    using ScpImmersiveVoice.Config;
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Configuration routing facade acting as the primary boundary layer 
    /// between abstract identity states and stateful live session graphs.
    /// </summary>
    public static class ScpVoiceProfiles
    {
        public static ConcurrentQueue<IDynamicVoicePresetProvider> DynamicProviders { get; } = new ConcurrentQueue<IDynamicVoicePresetProvider>();

        private readonly static ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;

        /// <summary>
        /// Global bridge property linking the static routing facade directly to the active instance-bound session heap.
        /// </summary>
        public static ScpVoiceManager VoiceManagerInstance { get; set; }

        /// <summary>
        /// Backward-compatible bridge accessing the session-bound pipeline. 
        /// Safeguards the graph synchronization loop under a thread-safe context lock.
        /// </summary>
        public static AudioEffectPipeline GetPipelineFor(Player player, ScpVoicePreset targetPreset)
        {
            if (player == null || targetPreset == null || VoiceManagerInstance == null) return null;

            var session = VoiceManagerInstance.StartSession(player);
            if (session == null) return null;

            lock (session.SyncLock)
            {
                DateTime now = DateTime.Now;

                bool presetChanged = session.LastAppliedPreset == null || !ArePresetsAcousticallyIdentical(session.LastAppliedPreset, targetPreset);
                bool silenceGapTriggered = (now - session.LastPacketReceivedTime).TotalSeconds > 0.5f;

                // INTENT: Rebuild the DSP graph if the structural configuration profile mutates, 
                // OR flush internal delay lines/buffers if a transmission gap suggests a new phrase has started.
                if (presetChanged || silenceGapTriggered)
                {
                    session.SynchronizePipelineGraph(targetPreset);
                    session.LastAppliedPreset = targetPreset.Clone();
                }

                // Maintain the temporal tracking reference to monitor incoming packet frequency.
                session.LastPacketReceivedTime = now;
            }

            return session.Pipeline;
        }

        /// <summary>
        /// Forces an immediate structural teardown of the subject's stream, flushing both hardware and DSP allocations.
        /// </summary>
        public static void ClearCacheFor(Player player)
        {
            if (player == null || VoiceManagerInstance == null) return;

            // Terminating the session natively disposes of both the circular buffer arrays and filters simultaneously
            VoiceManagerInstance.StopSession(player);
        }

        /// <summary>
        /// Evaluates float-precision metrics to avoid redundant, expensive DSP graph rebuild constraints.
        /// </summary>
        public static bool ArePresetsAcousticallyIdentical(ScpVoicePreset a, ScpVoicePreset b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;

            return Math.Abs(a.Pitch - b.Pitch) < 0.001f &&
                   Math.Abs(a.Formant - b.Formant) < 0.001f &&
                   Math.Abs(a.LowPass - b.LowPass) < 0.001f &&
                   Math.Abs(a.HighPass - b.HighPass) < 0.001f &&
                   Math.Abs(a.Distortion - b.Distortion) < 0.001f &&
                   Math.Abs(a.WhisperAmount - b.WhisperAmount) < 0.001f &&
                   Math.Abs(a.FormantDrift - b.FormantDrift) < 0.001f &&
                   Math.Abs(a.Subharmonic - b.Subharmonic) < 0.001f &&
                   Math.Abs(a.Guttural - b.Guttural) < 0.001f &&
                   Math.Abs(a.DryCrackle - b.DryCrackle) < 0.001f &&
                   Math.Abs(a.FleshCrackle - b.FleshCrackle) < 0.001f &&
                   Math.Abs(a.VocalShriek - b.VocalShriek) < 0.001f &&
                   Math.Abs(a.LaryngealAsymmetry - b.LaryngealAsymmetry) < 0.001f &&
                   Math.Abs(a.DeathRattle - b.DeathRattle) < 0.001f &&
                   Math.Abs(a.DemonicOctaverMix - b.DemonicOctaverMix) < 0.001f &&
                   Math.Abs(a.SiliconModulation - b.SiliconModulation) < 0.001f &&
                   Math.Abs(a.ScreechModulation - b.ScreechModulation) < 0.001f &&
                   Math.Abs(a.Tremolo - b.Tremolo) < 0.001f &&
                   Math.Abs(a.Glitch - b.Glitch) < 0.001f &&
                   Math.Abs(a.PredatoryCamouflage - b.PredatoryCamouflage) < 0.001f &&
                   Math.Abs(a.BreathNoise - b.BreathNoise) < 0.001f &&
                   Math.Abs(a.StaticNoise - b.StaticNoise) < 0.001f &&
                   Math.Abs(a.WetOrganic - b.WetOrganic) < 0.001f &&
                   Math.Abs(a.WetDecay - b.WetDecay) < 0.001f &&
                   Math.Abs(a.PocketEcho - b.PocketEcho) < 0.001f &&
                   Math.Abs(a.Reverb - b.Reverb) < 0.001f &&
                   a.UseNoiseGate == b.UseNoiseGate &&
                   Math.Abs(a.NoiseGateThreshold - b.NoiseGateThreshold) < 0.001f &&
                   a.Enable == b.Enable &&
                   a.Mode == b.Mode;
        }

        public static ScpVoicePreset GetPreset(Player player)
        {
            if (player == null) return new ScpVoicePreset { Enable = false };
            var role = player.Role;

            if (_config.EnableDynamicStates)
            {
                foreach (var provider in DynamicProviders)
                {
                    if (provider.TryGetDynamicPreset(player, out var dynamicPreset))
                    {
                        return dynamicPreset;
                    }
                }
            }

            if (_config.Presets.TryGetValue(role, out var preset) && preset.Enable)
            {
                return preset;
            }

            return ScpVoiceDefaultPresets.Create().TryGetValue(role, out var defaultPreset)
                ? defaultPreset
                : new ScpVoicePreset { Enable = false };
        }
    }
}