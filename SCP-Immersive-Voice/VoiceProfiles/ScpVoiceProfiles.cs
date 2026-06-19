namespace SCP_Immersive_Voice.VoiceProfiles
{
    using LabApi.Features.Wrappers;
    using SCP_Immersive_Voice.AudioProcessing;
    using SCP_Immersive_Voice.AudioProcessing.Effects;
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Interfaces;
    using ScpImmersiveVoice;
    using ScpImmersiveVoice.Config;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;

    public static class ScpVoiceProfiles
    {
        public static ConcurrentQueue<IDynamicVoicePresetProvider> DynamicProviders { get; } = new ConcurrentQueue<IDynamicVoicePresetProvider>();

        private readonly static ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;

        private readonly static ConcurrentDictionary<int, PipelineContainer> _stableCache = new ConcurrentDictionary<int, PipelineContainer>();

        private readonly static ConcurrentDictionary<(Type, string), FieldInfo> _fieldCache = new ConcurrentDictionary<(Type, string), FieldInfo>();

        private class PipelineContainer
        {
            public readonly AudioEffectPipeline Pipeline = new AudioEffectPipeline();
            public readonly Dictionary<string, IAudioEffect> ActiveNodes = new Dictionary<string, IAudioEffect>();
            public ScpVoicePreset LastAppliedPreset = null;
            public readonly object SyncLock = new object();
        }

        public static AudioEffectPipeline GetPipelineFor(Player player, ScpVoicePreset targetPreset)
        {
            if (player == null || targetPreset == null) return null;

            var container = _stableCache.GetOrAdd(player.PlayerId, id => new PipelineContainer());

            if (container.LastAppliedPreset == null || !ArePresetsAcousticallyIdentical(container.LastAppliedPreset, targetPreset))
            {
                lock (container.SyncLock)
                {
                    if (container.LastAppliedPreset == null || !ArePresetsAcousticallyIdentical(container.LastAppliedPreset, targetPreset))
                    {
                        SynchronizePipelineGraph(container, targetPreset);
                        container.LastAppliedPreset = targetPreset;
                    }
                }
            }

            return container.Pipeline;
        }

        private static bool ArePresetsAcousticallyIdentical(ScpVoicePreset a, ScpVoicePreset b)
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
                   a.Enable == b.Enable &&
                   a.Mode == b.Mode;
        }

        public static void ClearCacheFor(Player player)
        {
            if (player == null) return;
            _stableCache.TryRemove(player.PlayerId, out _);
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

        private static void SynchronizePipelineGraph(PipelineContainer container, ScpVoicePreset preset)
        {
            float sr = (float)VoiceChat.VoiceChatSettings.SampleRate;
            if (sr <= 0) sr = 48000f;

            var targetNodes = new List<(string Key, Func<IAudioEffect> Factory, float ScalarValue, string ScalarFieldName)>();

            float gateThreshold = preset.UseNoiseGate ? preset.NoiseGateThreshold : -45f;
            targetNodes.Add(("NoiseGate", () => new NoiseGateEffect(gateThreshold, sr), gateThreshold, "_threshold"));

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
            container.Pipeline.Clear();

            foreach (var target in targetNodes)
            {
                if (container.ActiveNodes.TryGetValue(target.Key, out var existingInstance) && target.ScalarFieldName != null)
                {
                    FastInjectScalarField(existingInstance, target.ScalarFieldName, target.ScalarValue);
                    temporaryMap[target.Key] = existingInstance;
                    container.Pipeline.Add(existingInstance);
                }
                else
                {
                    var newInstance = target.Factory();
                    temporaryMap[target.Key] = newInstance;
                    container.Pipeline.Add(newInstance);
                }
            }

            container.ActiveNodes.Clear();
            foreach (var kvp in temporaryMap)
            {
                container.ActiveNodes[kvp.Key] = kvp.Value;
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
            catch
            {
            }
        }
    }
}