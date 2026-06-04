namespace SCP_Immersive_Voice.VoiceProfiles
{
    using LabApi.Features.Wrappers;
    using PlayerRoles;
    using SCP_Immersive_Voice.AudioProcessing;
    using SCP_Immersive_Voice.AudioProcessing.Effects;
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Interfaces;
    using ScpImmersiveVoice;
    using ScpImmersiveVoice.Config;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public static class ScpVoiceProfiles
    {
        public static List<IDynamicVoicePresetProvider> DynamicProviders { get; } = new List<IDynamicVoicePresetProvider>();

        private readonly static ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;
       
        /// <summary>
        /// Cache of player pipelines to avoid allocations and reset of DSP
        /// </summary>
        private readonly static ConcurrentDictionary<int, AudioEffectPipeline> _pipelineCache = new ConcurrentDictionary<int, AudioEffectPipeline>();

        public static AudioEffectPipeline GetPipelineFor(Player player)
        { 
            // Getting stream from cache or building a new one
            return _pipelineCache.GetOrAdd(player.PlayerId, id =>
            {
                var role = player.Role;

                // 1. Dynamic presets
                foreach (var provider in DynamicProviders)
                {
                    if (provider.TryGetDynamicPreset(player, out var dynamicPreset))
                        return BuildPipelineFromPreset(dynamicPreset);
                }

                // 2. Static presets
                if (!_config.Presets.TryGetValue(role, out var preset) || !preset.Enable)
                    return new AudioEffectPipeline();

                return BuildPipelineFromPreset(preset);
            });
        }

        public static void ClearCacheFor(Player player)
        {
            _pipelineCache.TryRemove(player.PlayerId, out _);
        }

        public static ScpVoicePreset GetPreset(Player player)
        {
            var role = player.Role;

            // dynamic first
            foreach (var provider in DynamicProviders)
            {
                if (provider.TryGetDynamicPreset(player, out var dynamicPreset))
                    return dynamicPreset;
            }

            // static
            if (_config.Presets.TryGetValue(role, out var preset))
                return preset;

            // fallback
            return new ScpVoicePreset();
        }


        private static AudioEffectPipeline BuildPipelineFromPreset(ScpVoicePreset preset)
        {
            var p = new AudioEffectPipeline();

            // Getting dynamic sample rate from VoiceChat engine
            float currentSampleRate = (float)VoiceChat.VoiceChatSettings.SampleRate;
            if (currentSampleRate <= 0) currentSampleRate = 48000f; // Fallback helper

            // --- Input modifiers ---
            // AAA Standard Guard: Always maintain a noise gate to sanitize AGC breathing artifacts.
            // If the preset explicitly disables it or has an unconfigured threshold, fallback to standard -45 dB.
            float gateThreshold = preset.UseNoiseGate ? preset.NoiseGateThreshold : -45f;
            p.Add(new NoiseGateEffect(gateThreshold, currentSampleRate));

            // --- Core voice modifiers ---
            if (Math.Abs(preset.Pitch - 1f) > 0.01f)
                p.Add(new PitchShiftEffect(preset.Pitch, currentSampleRate, windowSizeMs: 40f));

            if (Math.Abs(preset.Formant - 1f) > 0.01f)
                p.Add(new FormantShiftEffect(preset.Formant, currentSampleRate));

            if (preset.FormantDrift > 0f)
                p.Add(new FormantDriftEffect(preset.FormantDrift));

            // --- Harmonic generators BEFORE distortion ---
            if (preset.Subharmonic > 0f)
                p.Add(new SubharmonicGrowlEffect(preset.Subharmonic, currentSampleRate));

            if (preset.Guttural > 0f)
                p.Add(new GutturalResonanceEffect(preset.Guttural, currentSampleRate));

            // --- Distortion on full-energy signal ---
            if (preset.Distortion > 0f)
                p.Add(new DistortionEffect(preset.Distortion, currentSampleRate));

            // --- Crackle layers ---
            if (preset.DryCrackle > 0f)
                p.Add(new DryCrackleEffect(preset.DryCrackle, currentSampleRate));

            if (preset.FleshCrackle > 0f)
                p.Add(new FleshCrackleEffect(preset.FleshCrackle, currentSampleRate));

            // --- Noise layers BEFORE filters ---
            if (preset.WhisperAmount > 0f)
                p.Add(new WhisperFilterEffect(preset.WhisperAmount, currentSampleRate));

            if (preset.BreathNoise > 0f)
                p.Add(new BreathNoiseEffect(preset.BreathNoise, currentSampleRate));

            if (preset.StaticNoise > 0f)
                p.Add(new StaticNoiseEffect(preset.StaticNoise, currentSampleRate));

            // --- Wet / spatial ---
            if (preset.WetOrganic > 0f)
                p.Add(new WetOrganicEffect(preset.WetOrganic, currentSampleRate));

            if (preset.WetDecay > 0f)
                p.Add(new WetDecayEffect(preset.WetDecay, currentSampleRate));

            if (preset.PocketEcho > 0f)
                p.Add(new PocketDimensionEchoEffect(preset.PocketEcho, currentSampleRate));

            if (preset.Reverb > 0f)
                p.Add(new ReverbEffect(preset.Reverb, currentSampleRate));

            // --- Filters LAST ---
            if (preset.LowPass > 0f)
                p.Add(new LowPassEffect(preset.LowPass, currentSampleRate));

            if (preset.HighPass > 0f)
                p.Add(new HighPassEffect(preset.HighPass, currentSampleRate));

            // --- Digital degradation ---
            if (preset.Bitcrush > 0f)
                p.Add(new BitcrushEffect(preset.Bitcrush));

            if (preset.SampleRateReduce > 0f)
                p.Add(new SampleRateReducerEffect(preset.SampleRateReduce, currentSampleRate));

            if (preset.Glitch > 0f)
                p.Add(new GlitchBurstEffect(preset.Glitch, currentSampleRate));

            // --- Stone layers ---
            if (preset.StoneCrack > 0f)
                p.Add(new StoneCrackEffect(preset.StoneCrack));

            if (preset.StoneGrind > 0f)
                p.Add(new StoneGrindEffect(preset.StoneGrind));

            // --- Creature chirps ---
            if (preset.Chirp > 0f)
                p.Add(new ChirpEffect(preset.Chirp));

            return p;
        }
    }
}