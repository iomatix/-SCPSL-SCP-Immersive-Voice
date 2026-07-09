using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.AudioProcessing;
using SCP_Immersive_Voice.Managers;
using SCP_Immersive_Voice.Presets;
using SCP_Immersive_Voice.Presets.Dynamics.Interfaces;
using ScpImmersiveVoice;
using ScpImmersiveVoice.Config;
using System;
using System.Collections.Concurrent;

namespace SCP_Immersive_Voice.VoiceProfiles
{
    public static class ScpVoiceProfiles
    {
        #region Public Registry Matrix Channels
        public static ConcurrentQueue<IDynamicVoicePresetProvider> DynamicProviders { get; } = new();
        public static ScpVoiceManager VoiceManagerInstance { get; set; }
        #endregion

        #region Private Performance Caches
        private static readonly ImmersiveScpVoiceConfig Config = ImmersiveScpVoicePlugin.StaticConfig;
        private static readonly ScpVoicePreset DisabledPreset = new() { Enable = false };
        private static readonly System.Collections.Generic.Dictionary<PlayerRoles.RoleTypeId, ScpVoicePreset> DefaultPresetsCache = ScpVoiceDefaultPresets.Create();
        #endregion

        #region Pipeline Resolution Routing
        public static (AudioEffectPipeline Pipeline, ScpVoicePreset Preset) ResolvePipelineContext(Player player, VoiceSession session)
        {
            if (player is null || session is null || VoiceManagerInstance is null)
                return (null, null);

            var targetPreset = GetPreset(player);
            if (targetPreset is null || !targetPreset.Enable)
                return (null, null);

            lock (session.SyncLock)
            {
                DateTime now = DateTime.UtcNow;

                bool presetChanged = session.LastAppliedPreset is null || !ArePresetsAcousticallyIdentical(session.LastAppliedPreset, targetPreset);
                bool silenceGapTriggered = (now - session.LastPacketReceivedTime).TotalSeconds > 0.5;

                if (presetChanged || silenceGapTriggered)
                {
                    session.SynchronizePipelineGraph(targetPreset, forceReset: silenceGapTriggered);
                    session.LastAppliedPreset = targetPreset.Clone();
                }

                session.LastPacketReceivedTime = now;
            }

            return (session.Pipeline, targetPreset);
        }

        public static void ClearCacheFor(Player player)
        {
            if (player is null || VoiceManagerInstance is null) return;
            VoiceManagerInstance.StopSession(player);
        }

        public static ScpVoicePreset GetPreset(Player player)
        {
            if (player is null)
                return DisabledPreset;

            var role = player.Role;

            if (Config is not null && Config.EnableDynamicStates)
            {
                foreach (var provider in DynamicProviders)
                {
                    if (provider is not null && provider.TryGetDynamicPreset(player, out var dynamicPreset))
                    {
                        return dynamicPreset;
                    }
                }
            }

            if (Config?.Presets is not null && Config.Presets.TryGetValue(role, out var preset) && preset.Enable)
            {
                return preset;
            }

            return DefaultPresetsCache.TryGetValue(role, out var defaultPreset)
                ? defaultPreset
                : DisabledPreset;
        }
        #endregion

        #region Diagnostic Delta Arithmetic
        public static bool ArePresetsAcousticallyIdentical(ScpVoicePreset a, ScpVoicePreset b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;

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
        #endregion
    }
}