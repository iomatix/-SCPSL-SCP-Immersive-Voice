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
    using System.Collections.Generic;

    public static class ScpVoiceProfiles
    {
        public static List<IDynamicVoicePresetProvider> DynamicProviders { get; } = new List<IDynamicVoicePresetProvider>();

        private readonly static ImmersiveScpVoiceConfig _config = ImmersiveScpVoicePlugin.StaticConfig;


        public static AudioEffectPipeline GetPipelineFor(Player player)
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

            // --- Core voice modifiers ---
            if (Math.Abs(preset.Pitch - 1f) > 0.01f)
                p.Add(new PitchShiftEffect(preset.Pitch));

            if (Math.Abs(preset.Formant - 1f) > 0.01f)
                p.Add(new FormantShiftEffect(preset.Formant));

            if (preset.FormantDrift > 0f)
                p.Add(new FormantDriftEffect(preset.FormantDrift));

            // --- Nonlinear effects ---
            if (preset.Distortion > 0f)
                p.Add(new DistortionEffect(preset.Distortion));

            if (preset.Guttural > 0f)
                p.Add(new GutturalResonanceEffect(preset.Guttural));

            if (preset.Subharmonic > 0f)
                p.Add(new SubharmonicGrowlEffect(preset.Subharmonic));

            if (preset.DryCrackle > 0f)
                p.Add(new DryCrackleEffect(preset.DryCrackle));

            if (preset.FleshCrackle > 0f)
                p.Add(new FleshCrackleEffect(preset.FleshCrackle));

            // --- Filtering ---
            if (preset.LowPass > 0f)
                p.Add(new LowPassEffect(preset.LowPass));

            if (preset.HighPass > 0f)
                p.Add(new HighPassEffect(preset.HighPass));

            // --- Spatial / wet ---
            if (preset.Reverb > 0f)
                p.Add(new ReverbEffect(preset.Reverb));

            if (preset.WetDecay > 0f)
                p.Add(new WetDecayEffect(preset.WetDecay));

            if (preset.WetOrganic > 0f)
                p.Add(new WetOrganicEffect(preset.WetOrganic));

            if (preset.PocketEcho > 0f)
                p.Add(new PocketDimensionEchoEffect(preset.PocketEcho));

            // --- Noise layers ---
            if (preset.BreathNoise > 0f)
                p.Add(new BreathNoiseEffect(preset.BreathNoise));

            if (preset.WhisperAmount > 0f)
                p.Add(new WhisperFilterEffect(preset.WhisperAmount));

            if (preset.StaticNoise > 0f)
                p.Add(new StaticNoiseEffect(preset.StaticNoise));

            // --- Digital degradation ---
            if (preset.Bitcrush > 0f)
                p.Add(new BitcrushEffect(preset.Bitcrush));

            if (preset.SampleRateReduce > 0f)
                p.Add(new SampleRateReducerEffect(preset.SampleRateReduce));

            if (preset.Glitch > 0f)
                p.Add(new GlitchBurstEffect(preset.Glitch));

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