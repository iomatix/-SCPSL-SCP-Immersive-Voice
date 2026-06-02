namespace SCP_Immersive_Voice.VoiceProfiles
{
    using PlayerRoles;
    using SCP_Immersive_Voice.AudioProcessing;
    using SCP_Immersive_Voice.AudioProcessing.Effects;
    public static class ScpVoiceProfiles
    {
        public static AudioEffectPipeline GetPipelineFor(RoleTypeId role)
        {
            var p = new AudioEffectPipeline();

            switch (role)
            {
                case RoleTypeId.Scp049:
                    p.Add(new PitchShiftEffect(0.7f));
                    p.Add(new FormantShiftEffect(0.85f));
                    p.Add(new DistortionEffect(1.2f));
                    break;

                case RoleTypeId.Scp096:
                    p.Add(new PitchShiftEffect(1.3f));
                    p.Add(new DistortionEffect(2.0f));
                    break;

                case RoleTypeId.Scp939:
                    p.Add(new PitchShiftEffect(0.5f));
                    p.Add(new FormantShiftEffect(0.6f));
                    break;

                case RoleTypeId.Scp173:
                    p.Add(new DistortionEffect(3.0f));
                    break;

                case RoleTypeId.Scp106:
                    p.Add(new PitchShiftEffect(0.6f));
                    p.Add(new FormantShiftEffect(0.5f));
                    p.Add(new DistortionEffect(1.5f));
                    break;

                case RoleTypeId.Scp3114:
                    p.Add(new PitchShiftEffect(1.8f));
                    break;

                // Flamingo variants
                case RoleTypeId.Flamingo:
                case RoleTypeId.AlphaFlamingo:
                case RoleTypeId.ZombieFlamingo:
                case RoleTypeId.NtfFlamingo:
                case RoleTypeId.ChaosFlamingo:
                    p.Add(new PitchShiftEffect(1.4f));
                    p.Add(new DistortionEffect(1.1f));
                    break;
            }

            return p;
        }
    }

}
