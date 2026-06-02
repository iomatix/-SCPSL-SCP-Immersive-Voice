namespace SCP_Immersive_Voice.VoiceProfiles
{
    using PlayerRoles;
    using SCP_Immersive_Voice.AudioProcessing;
    using SCP_Immersive_Voice.AudioProcessing.Effects;
    using ScpImmersiveVoice.Config;

    public static class ScpVoiceProfiles
    {
        public static AudioEffectPipeline GetPipelineFor(RoleTypeId role, ImmersiveScpVoiceConfig config)
        {
            var p = new AudioEffectPipeline();

            if (!config.Presets.TryGetValue(role, out var preset) || !preset.Enable)
                return p;

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


            return p;
        }

    }

}