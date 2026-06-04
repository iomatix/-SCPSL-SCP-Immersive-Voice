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
        public static List<IDynamicVoicePresetProvider> DynamicProviders { get; } = new List<IDynamicVoicePresetProvider>();

        private readonly static ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;

        // Permanent thread-safe cache containing stateful container blocks instead of raw naked pipelines
        private readonly static ConcurrentDictionary<int, PipelineContainer> _stableCache = new ConcurrentDictionary<int, PipelineContainer>();

        /// <summary>
        /// Context container holding the processing pipeline and live instances map to protect memory boundaries.
        /// </summary>
        private class PipelineContainer
        {
            public readonly AudioEffectPipeline Pipeline = new AudioEffectPipeline();
            public readonly Dictionary<string, IAudioEffect> ActiveNodes = new Dictionary<string, IAudioEffect>();
            public ScpVoicePreset LastAppliedPreset = null;
            public readonly object SyncLock = new object();
        }

        public static AudioEffectPipeline GetPipelineFor(Player player)
        {
            if (player == null) return null;

            // 1. Fetch or initialize the permanent context container container for the player
            var container = _stableCache.GetOrAdd(player.PlayerId, id => new PipelineContainer());

            // 2. Resolve what preset should be processed at this exact microsecond
            var targetPreset = GetPreset(player);

            // 3. Thread-safe runtime check to see if the state actually mutated
            if (container.LastAppliedPreset == null || !ReferenceEquals(container.LastAppliedPreset, targetPreset))
            {
                lock (container.SyncLock)
                {
                    // Re-verify after locking to avoid multi-thread state collisions
                    if (!ReferenceEquals(container.LastAppliedPreset, targetPreset))
                    {
                        SynchronizePipelineGraph(container, targetPreset);
                        container.LastAppliedPreset = targetPreset;
                    }
                }
            }

            return container.Pipeline;
        }

        public static void ClearCacheFor(Player player)
        {
            if (player == null) return;
            _stableCache.TryRemove(player.PlayerId, out _);
        }

        public static ScpVoicePreset GetPreset(Player player)
        {
            var role = player.Role;

            // Dynamic states have highest execution priority
            foreach (var provider in DynamicProviders)
            {
                if (provider.TryGetDynamicPreset(player, out var dynamicPreset))
                    return dynamicPreset;
            }

            // Fallback to configured static role presets
            if (_config.Presets.TryGetValue(role, out var preset) && preset.Enable)
                return preset;

            return ScpVoiceDefaultPresets.Create().TryGetValue(role, out var defaultPreset)
                ? defaultPreset
                : new ScpVoicePreset { Enable = false };
        }

        /// <summary>
        /// Synchronizes the running processing graph in-place. 
        /// Reuses active node instances to maintain perfect mathematical filter phase continuity.
        /// </summary>
        private static void SynchronizePipelineGraph(PipelineContainer container, ScpVoicePreset preset)
        {
            float sr = (float)VoiceChat.VoiceChatSettings.SampleRate;
            if (sr <= 0) sr = 48000f;

            // Step 1: Create the target list representing the execution requirement
            var targetNodes = new List<(string Key, Func<IAudioEffect> Factory, float ScalarValue, string ScalarFieldName)>();

            // Noise Gate (Always persistent)
            float gateThreshold = preset.UseNoiseGate ? preset.NoiseGateThreshold : -45f;
            targetNodes.Add(("NoiseGate", () => new NoiseGateEffect(gateThreshold, sr), gateThreshold, "_threshold"));

            // Core Voice Modifiers
            if (Math.Abs(preset.Pitch - 1f) > 0.01f)
                targetNodes.Add(("PitchShift", () => new PitchShiftEffect(preset.Pitch, sr, 40f), preset.Pitch, "_pitch"));

            if (Math.Abs(preset.Formant - 1f) > 0.01f)
                targetNodes.Add(("FormantShift", () => new FormantShiftEffect(preset.Formant, sr), preset.Formant, "_formant"));

            if (preset.FormantDrift > 0f)
                targetNodes.Add(("FormantDrift", () => new FormantDriftEffect(preset.FormantDrift), preset.FormantDrift, "_amount"));

            // Harmonics
            if (preset.Subharmonic > 0f)
                targetNodes.Add(("Subharmonic", () => new SubharmonicGrowlEffect(preset.Subharmonic, sr), preset.Subharmonic, "_amount"));

            if (preset.Guttural > 0f)
                targetNodes.Add(("Guttural", () => new GutturalResonanceEffect(preset.Guttural, sr), preset.Guttural, "_amount"));

            // Distortion
            if (preset.Distortion > 0f)
                targetNodes.Add(("Distortion", () => new DistortionEffect(preset.Distortion, sr), preset.Distortion, "_amount"));

            // Crackles
            if (preset.DryCrackle > 0f)
                targetNodes.Add(("DryCrackle", () => new DryCrackleEffect(preset.DryCrackle, sr), preset.DryCrackle, "_amount"));

            if (preset.FleshCrackle > 0f)
                targetNodes.Add(("FleshCrackle", () => new FleshCrackleEffect(preset.FleshCrackle, sr), preset.FleshCrackle, "_amount"));

            // Noises
            if (preset.WhisperAmount > 0f)
                targetNodes.Add(("Whisper", () => new WhisperFilterEffect(preset.WhisperAmount, sr), preset.WhisperAmount, "_amount"));

            if (preset.BreathNoise > 0f)
                targetNodes.Add(("Breath", () => new BreathNoiseEffect(preset.BreathNoise, sr), preset.BreathNoise, "_intensity"));

            if (preset.StaticNoise > 0f)
                targetNodes.Add(("Static", () => new StaticNoiseEffect(preset.StaticNoise, sr), preset.StaticNoise, "_amount"));

            // Spatial / Wet
            if (preset.WetOrganic > 0f)
                targetNodes.Add(("WetOrganic", () => new WetOrganicEffect(preset.WetOrganic, sr), preset.WetOrganic, "_amount"));

            if (preset.WetDecay > 0f)
                targetNodes.Add(("WetDecay", () => new WetDecayEffect(preset.WetDecay, sr), preset.WetDecay, "_amount"));

            if (preset.PocketEcho > 0f)
                targetNodes.Add(("PocketEcho", () => new PocketDimensionEchoEffect(preset.PocketEcho, sr), preset.PocketEcho, "_amount"));

            if (preset.Reverb > 0f)
                targetNodes.Add(("Reverb", () => new ReverbEffect(preset.Reverb, sr), preset.Reverb, "_amount"));

            // Filters
            if (preset.LowPass > 0f)
                targetNodes.Add(("LowPass", () => new LowPassEffect(preset.LowPass, sr), preset.LowPass, null)); // Filters rebuild on raw frequency shift

            if (preset.HighPass > 0f)
                targetNodes.Add(("HighPass", () => new HighPassEffect(preset.HighPass, sr), preset.HighPass, null));

            // Degradation
            if (preset.Bitcrush > 0f)
                targetNodes.Add(("Bitcrush", () => new BitcrushEffect(preset.Bitcrush), preset.Bitcrush, null));

            if (preset.SampleRateReduce > 0f)
                targetNodes.Add(("SampleRateReduce", () => new SampleRateReducerEffect(preset.SampleRateReduce, sr), preset.SampleRateReduce, null));

            if (preset.Glitch > 0f)
                targetNodes.Add(("Glitch", () => new GlitchBurstEffect(preset.Glitch, sr), preset.Glitch, null));

            // Stone Layers
            if (preset.StoneCrack > 0f)
                targetNodes.Add(("StoneCrack", () => new StoneCrackEffect(preset.StoneCrack, sr), preset.StoneCrack, null));

            if (preset.StoneGrind > 0f)
                targetNodes.Add(("StoneGrind", () => new StoneGrindEffect(preset.StoneGrind, sr), preset.StoneGrind, null));

            // Chirps
            if (preset.Chirp > 0f)
                targetNodes.Add(("Chirp", () => new ChirpEffect(preset.Chirp, sr), preset.Chirp, null));

            // Step 2: In-place reconciliation loop
            var temporaryMap = new Dictionary<string, IAudioEffect>();
            container.Pipeline.Clear(); // Empty structural list safely without dropping operational memory allocation maps

            foreach (var target in targetNodes)
            {
                if (container.ActiveNodes.TryGetValue(target.Key, out var existingInstance) && target.ScalarFieldName != null)
                {
                    // CRITICAL AAA OPTIMIZATION: The instance exists! Maintain it to preserve history registers.
                    // Rapidly inject the new configuration scalar via high-speed cached reflection to bypass readonly boundaries.
                    FastInjectScalarField(existingInstance, target.ScalarFieldName, target.ScalarValue);
                    temporaryMap[target.Key] = existingInstance;
                    container.Pipeline.Add(existingInstance);
                }
                else
                {
                    // Instantiate fresh node only if it wasn't present before or requires structural parameter rebuilds (like filters)
                    var newInstance = target.Factory();
                    temporaryMap[target.Key] = newInstance;
                    container.Pipeline.Add(newInstance);
                }
            }

            // Step 3: Atomic state assignment update
            container.ActiveNodes.Clear();
            foreach (var kvp in temporaryMap)
            {
                container.ActiveNodes[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// High-speed reflection helper to overwrite configuration values while protecting active filter state memory registers.
        /// </summary>
        private static void FastInjectScalarField(object instance, string fieldName, float value)
        {
            try
            {
                var field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(instance, value);
                }
            }
            catch
            { // Safeguard fallback block 
            }
        }
    }
}