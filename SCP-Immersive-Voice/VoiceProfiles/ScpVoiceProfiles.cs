namespace SCP_Immersive_Voice.VoiceProfiles
{
    using LabApi.Features.Wrappers;
    using PlayerRoles;
    using SCP_Immersive_Voice.AudioProcessing;
    using SCP_Immersive_Voice.AudioProcessing.Effects;
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Interfaces;
    using ScpImmersiveVoice.Config;
    using System.Collections.Generic;

    public static class ScpVoiceProfiles
    {
        public static List<IDynamicVoicePresetProvider> DynamicProviders { get; } = new List<IDynamicVoicePresetProvider>();


        public static AudioEffectPipeline GetPipelineFor(Player player, ImmersiveScpVoiceConfig config)
        {
            var role = player.Role;

            // 1. Dynamic presets
            foreach (var provider in DynamicProviders)
            {
                if (provider.TryGetDynamicPreset(player, out var dynamicPreset))
                    return BuildPipelineFromPreset(dynamicPreset);
            }

            // 2. Static presets
            if (!config.Presets.TryGetValue(role, out var preset) || !preset.Enable)
                return new AudioEffectPipeline();

            return BuildPipelineFromPreset(preset);
        }

        private static AudioEffectPipeline BuildPipelineFromPreset(ScpVoicePreset preset)
        {
            var p = new AudioEffectPipeline();

            if (preset.Pitch != 1f)
                p.Add(new PitchShiftEffect(preset.Pitch));

            if (preset.Formant != 1f)
                p.Add(new FormantShiftEffect(preset.Formant));

            if (preset.Distortion > 0f)
                p.Add(new DistortionEffect(preset.Distortion));

            if (preset.LowPass > 0f)
                p.Add(new LowPassEffect(preset.LowPass));

            if (preset.HighPass > 0f)
                p.Add(new HighPassEffect(preset.HighPass));

            if (preset.Reverb > 0f)
                p.Add(new ReverbEffect(preset.Reverb));

            if (preset.BreathNoise > 0f)
                p.Add(new BreathNoiseEffect(preset.BreathNoise));

            if (preset.WhisperAmount > 0f)
                p.Add(new WhisperFilterEffect(preset.WhisperAmount));

            if (preset.StoneCrack > 0f)
                p.Add(new StoneCrackEffect(preset.StoneCrack));

            if (preset.StoneGrind > 0f)
                p.Add(new StoneGrindEffect(preset.StoneGrind));


            return p;
        }

    }
}