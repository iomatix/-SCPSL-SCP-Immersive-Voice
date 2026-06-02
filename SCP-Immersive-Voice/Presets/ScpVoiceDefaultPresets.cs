namespace SCP_Immersive_Voice.Presets
{
    using PlayerRoles;
    using System.Collections.Generic;

    public static class ScpVoiceDefaultPresets
    {
        public static Dictionary<RoleTypeId, ScpVoicePreset> Create()
        {
            return new Dictionary<RoleTypeId, ScpVoicePreset>()
            {
                [RoleTypeId.Scp049] = new ScpVoicePreset { Enable = true, Pitch = 0.7f, Formant = 0.85f, Distortion = 1.2f },
                [RoleTypeId.Scp096] = new ScpVoicePreset { Enable = true, Pitch = 1.3f, Formant = 1f, Distortion = 2.0f },
                [RoleTypeId.Scp939] = new ScpVoicePreset { Enable = true, Pitch = 0.5f, Formant = 0.6f, Distortion = 0f },
                [RoleTypeId.Scp173] = new ScpVoicePreset { Enable = true, Pitch = 1f, Formant = 1f, Distortion = 3.0f },
                [RoleTypeId.Scp106] = new ScpVoicePreset { Enable = true, Pitch = 0.6f, Formant = 0.5f, Distortion = 1.5f },
                [RoleTypeId.Scp3114] = new ScpVoicePreset { Enable = true, Pitch = 1.8f, Formant = 1f, Distortion = 0f },

                // Flamingo variants
                [RoleTypeId.Flamingo] = new ScpVoicePreset { Enable = true, Pitch = 1.4f, Formant = 1f, Distortion = 1.1f },
                [RoleTypeId.AlphaFlamingo] = new ScpVoicePreset { Enable = true, Pitch = 1.4f, Formant = 1f, Distortion = 1.1f },
                [RoleTypeId.ZombieFlamingo] = new ScpVoicePreset { Enable = true, Pitch = 1.4f, Formant = 1f, Distortion = 1.1f },
                [RoleTypeId.NtfFlamingo] = new ScpVoicePreset { Enable = true, Pitch = 1.4f, Formant = 1f, Distortion = 1.1f },
                [RoleTypeId.ChaosFlamingo] = new ScpVoicePreset { Enable = true, Pitch = 1.4f, Formant = 1f, Distortion = 1.1f },
            };
        }
    }

}
