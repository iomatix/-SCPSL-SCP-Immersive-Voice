namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Dynamic identity states for SCP‑106, tuned for decayed resonance,
    /// dimensional instability and wet organic presence.
    /// Designed to shift tone based on movement, submersion and dimensional travel.
    /// </summary>
    public static class Scp106DynamicPresets
    {
        // Baseline decay: old, wet, heavy presence
        public static readonly ScpVoicePreset IdlePreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Pitch = 0.58f,
            Formant = 0.55f,
            Distortion = 0.55f,
            LowPass = 900f,
            Reverb = 0.48f,
            WetDecay = 0.85f,
            WetOrganic = 0.22f,
            PocketEcho = 0.70f,
            FormantDrift = 0.30f
        };

        // Submerged state: muffled, distant, buried under matter
        public static readonly ScpVoicePreset StalkingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Pitch = 0.50f,
            Formant = 0.48f,
            LowPass = 650f,
            HighPass = 0f,
            Distortion = 0.35f,
            WetDecay = 0.95f,
            WetOrganic = 0.40f,
            PocketEcho = 0.55f,
            FormantDrift = 0.25f
        };

        // Emerging from ground: wet burst, dimensional tearing
        public static readonly ScpVoicePreset EmergingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Pitch = 0.62f,
            Formant = 0.60f,
            LowPass = 1100f,
            Distortion = 0.75f,
            WetDecay = 1.00f,
            WetOrganic = 0.55f,
            PocketEcho = 0.85f,
            FormantDrift = 0.40f
        };

        // Dimensional echo: deep, unreal, smeared resonance
        public static readonly ScpVoicePreset PocketDimensionPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Pitch = 0.52f,
            Formant = 0.50f,
            LowPass = 700f,
            HighPass = 0f,
            Distortion = 0.65f,
            Reverb = 0.65f,
            WetDecay = 0.90f,
            WetOrganic = 0.35f,
            PocketEcho = 1.00f,
            FormantDrift = 0.45f
        };

        // Atlas usage: subtle dimensional smear without full distortion
        public static readonly ScpVoicePreset AtlasDimensionalPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Pitch = 0.56f,
            Formant = 0.58f,
            LowPass = 1000f,
            Distortion = 0.45f,
            Reverb = 0.40f,
            WetDecay = 0.75f,
            WetOrganic = 0.25f,
            PocketEcho = 0.80f,
            FormantDrift = 0.28f
        };

        // Exhausted decay: heavy, wet, collapsing resonance
        public static readonly ScpVoicePreset LowVigorPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Pitch = 0.48f,
            Formant = 0.50f,
            LowPass = 800f,
            Distortion = 0.40f,
            WetDecay = 1.00f,
            WetOrganic = 0.45f,
            PocketEcho = 0.65f,
            FormantDrift = 0.35f
        };

        // Attack mode: tearing, aggressive, dimensional ripping
        public static readonly ScpVoicePreset AttackPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Pitch = 0.70f,
            Formant = 0.62f,
            HighPass = 300f,
            LowPass = 1300f,
            Distortion = 1.20f,
            WetDecay = 0.95f,
            WetOrganic = 0.55f,
            PocketEcho = 0.90f,
            Guttural = 0.35f,
            Subharmonic = 0.25f,
            FormantDrift = 0.50f
        };

        public static ScpVoicePreset GetPresetForState(Scp106VoiceState state)
        {
            switch (state)
            {
                case Scp106VoiceState.Idle:
                    return IdlePreset;

                case Scp106VoiceState.Stalking:
                    return StalkingPreset;

                case Scp106VoiceState.Emerging:
                    return EmergingPreset;

                case Scp106VoiceState.PocketDimension:
                    return PocketDimensionPreset;

                case Scp106VoiceState.AtlasDimensional:
                    return AtlasDimensionalPreset;

                case Scp106VoiceState.LowVigor:
                    return LowVigorPreset;

                case Scp106VoiceState.Attack:
                    return AttackPreset;

                default:
                    return IdlePreset;
            }
        }
    }
}