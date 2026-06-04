namespace SCP_Immersive_Voice.Presets.Dynamics.Resolvers
{

    using global::SCP_Immersive_Voice.Presets.Dynamics.Enums;
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Resolves the final SCP‑106 voice preset based on dynamic state priority.
    /// Designed to merge default identity with temporary emotional states.
    /// </summary>
    public static class Scp106PresetResolver
    {
        /// <summary>
        /// Returns the active preset for SCP‑106 based on the current dynamic state.
        /// </summary>
        public static ScpVoicePreset Resolve(Scp106VoiceState state)
        {
            switch (state)
            {
                // Intent: submerged, muffled, distant presence
                case Scp106VoiceState.Stalking:
                    return Scp106DynamicPresets.StalkingPreset;

                // Intent: rising from the ground with wet burst
                case Scp106VoiceState.Emerging:
                    return Scp106DynamicPresets.EmergingPreset;

                // Intent: deep, unreal, dimensional resonance
                case Scp106VoiceState.PocketDimension:
                    return Scp106DynamicPresets.PocketDimensionPreset;

                // Intent: subtle dimensional smear during Atlas sensing
                case Scp106VoiceState.AtlasDimensional:
                    return Scp106DynamicPresets.AtlasDimensionalPreset;

                // Intent: heavy, collapsing resonance when exhausted
                case Scp106VoiceState.LowVigor:
                    return Scp106DynamicPresets.LowVigorPreset;

                // Intent: tearing, aggressive, dimensional ripping
                case Scp106VoiceState.Attack:
                    return Scp106DynamicPresets.AttackPreset;

                // Intent: baseline decay, old and wet
                case Scp106VoiceState.Idle:
                default:
                    return Scp106DynamicPresets.IdlePreset;
            }
        }

        /// <summary>
        /// Merges the resolved dynamic preset with the default SCP‑106 preset.
        /// Ensures consistent identity while allowing emotional variation.
        /// </summary>
        public static ScpVoicePreset MergeWithDefault(ScpVoicePreset dynamicPreset)
        {
            var baseline = ScpVoiceDefaultPresets.Create()[PlayerRoles.RoleTypeId.Scp106];

            return new ScpVoicePreset
            {
                // Intent: preserve identity while allowing emotional shifts
                Pitch = dynamicPreset.Pitch,
                Formant = dynamicPreset.Formant,

                // Intent: maintain SCP‑106's decayed wetness
                WetDecay = dynamicPreset.WetDecay > 0 ? dynamicPreset.WetDecay : baseline.WetDecay,
                WetOrganic = dynamicPreset.WetOrganic > 0 ? dynamicPreset.WetOrganic : baseline.WetOrganic,

                // Intent: dimensional resonance unique to 106
                PocketEcho = dynamicPreset.PocketEcho > 0 ? dynamicPreset.PocketEcho : baseline.PocketEcho,

                // Intent: preserve spectral shape unless overridden
                LowPass = dynamicPreset.LowPass > 0 ? dynamicPreset.LowPass : baseline.LowPass,
                HighPass = dynamicPreset.HighPass > 0 ? dynamicPreset.HighPass : baseline.HighPass,

                // Intent: distortion only when emotional state requires it
                Distortion = dynamicPreset.Distortion > 0 ? dynamicPreset.Distortion : baseline.Distortion,

                // Intent: maintain subtle instability
                FormantDrift = dynamicPreset.FormantDrift > 0 ? dynamicPreset.FormantDrift : baseline.FormantDrift,

                // Intent: aggressive tearing only in attack states
                Guttural = dynamicPreset.Guttural,
                Subharmonic = dynamicPreset.Subharmonic,

                // Intent: preserve reverb identity unless overridden
                Reverb = dynamicPreset.Reverb > 0 ? dynamicPreset.Reverb : baseline.Reverb,

                // Intent: keep other baseline values intact
                Enable = true
            };
        }
    }
}
