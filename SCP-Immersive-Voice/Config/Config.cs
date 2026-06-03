namespace ScpImmersiveVoice.Config
{
    using PlayerRoles;
    using SCP_Immersive_Voice.Presets;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class ImmersiveScpVoiceConfig
    {
        [Description("Enable proximity voice chat for SCP players")]
        public bool EnableScpProximityVoice { get; set; } = true;

        [Description("SCP proximity chat radious")]
        public float ProximityDistance { get; set; } = 45.75f;

        [Description("Enable audio effects (pitch/formant/distortion) for SCP voices")]
        public bool EnableScpVoiceEffects { get; set; } = true;

        [Description("Audio effect presets for each SCP role")]
        public Dictionary<RoleTypeId, ScpVoicePreset> Presets { get; set; } = ScpVoiceDefaultPresets.Create();

        [Description("Roles excluded from SCP proximity voice")]
        public HashSet<RoleTypeId> ForbiddenProximity { get; set; } = new HashSet<RoleTypeId>()
        {
            RoleTypeId.Scp079,
        };

    }
}