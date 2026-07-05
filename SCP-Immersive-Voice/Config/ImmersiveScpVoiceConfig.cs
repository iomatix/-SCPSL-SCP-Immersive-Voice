using LabApi.Loader.Features.Configuration;
using PlayerRoles;
using SCP_Immersive_Voice.Presets;
using System.Collections.Generic;
using System.ComponentModel;

namespace ScpImmersiveVoice.Config
{
    /// <summary>
    /// Configuration profiles mapping hardware thresholds and DSP presets for the immersive voice engine.
    /// </summary>
    public class ImmersiveScpVoiceConfig : LabApiConfig
    {
        [Description("Enable proximity voice chat for SCP players")]
        public bool EnableScpProximityVoice { get; set; } = true;

        [Description("SCP proximity chat radius in meters")]
        public float ProximityDistance { get; set; } = 65.35f;

        [Description("Enable audio effects (pitch/formant/distortion) for SCP voices")]
        public bool EnableScpVoiceEffects { get; set; } = true;

        [Description("Enable advanced dynamic state engines")]
        public bool EnableDynamicStates { get; set; } = true;

        [Description("Audio effect presets for each SCP role")]
        public Dictionary<RoleTypeId, ScpVoicePreset> Presets { get; set; } = ScpVoiceDefaultPresets.Create();

        [Description("Roles excluded from SCP proximity voice")]
        public HashSet<RoleTypeId> ForbiddenProximity { get; set; } = new()
        {
            RoleTypeId.Scp079
        };

        [Description("Enable debug logging")]
        public bool Debug { get; internal set; }
    }
}