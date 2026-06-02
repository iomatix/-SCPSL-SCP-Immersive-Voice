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
        public float ProximityDistance { get; set; } = 5.5f;

        [Description("Apply pitch distortion to SCP voices")]
        public bool ApplyDistortion { get; set; } = true;

        [Description("Audio effect presets for each SCP role")]
        public Dictionary<RoleTypeId, ScpVoicePreset> Presets { get; set; }

    }
}