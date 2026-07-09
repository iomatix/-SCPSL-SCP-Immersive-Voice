namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Dynamic identity states for SCP-106, tuned for severe decayed resonance,
    /// non-Euclidean phase dislocation, and necrotic wet presence with enhanced high-frequency caustic edge.
    /// Perfectly calibrated for the stateful, zero-allocation multi-band DSP pipeline matrix.
    /// </summary>
    public static class Scp106DynamicPresets
    {
        // =================================================================
        // IDLE — Baseline decay: old, wet, heavy presence with acidic edge
        // =================================================================
        public static readonly ScpVoicePreset IdlePreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            IsGlobalTransmission = false,
            OutputGain = 2.65f,     // TUNING: Slight bump to keep him prominent in proximity
            Pitch = 0.52f,
            Formant = 0.46f,
            DemonicOctaverMix = 0.25f,
            Distortion = 0.45f,     // TUNING: Tamed from 0.70f to prevent waveshaper saturation from hashing the low frequencies
            Guttural = 0.48f,
            FleshCrackle = 0.25f,
            LowPass = 1150f,
            Reverb = 0.40f,
            WetDecay = 0.90f,
            WetOrganic = 0.35f,
            PocketEcho = 0.80f,
            FormantDrift = 0.30f
        };

        // =================================================================
        // STALKING — Submerged state: completely buried under solid matter
        // =================================================================
        public static readonly ScpVoicePreset StalkingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            IsGlobalTransmission = false,
            OutputGain = 3.60f,
            Pitch = 0.48f,
            Formant = 0.42f,
            LowPass = 550f,
            HighPass = 0f,
            Distortion = 0.25f,
            WetDecay = 1.00f,
            WetOrganic = 0.65f,
            PocketEcho = 0.40f,
            FormantDrift = 0.20f
        };

        // =================================================================
        // EMERGING — Rising from solid mass: wet burst, physical rupture of reality
        // =================================================================
        public static readonly ScpVoicePreset EmergingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            IsGlobalTransmission = false,
            OutputGain = 2.85f,
            Pitch = 0.58f,
            Formant = 0.52f,
            DemonicOctaverMix = 0.35f,
            LowPass = 1400f,
            Distortion = 0.55f,
            Guttural = 0.75f,
            FleshCrackle = 0.40f,
            WetDecay = 1.00f,
            WetOrganic = 0.70f,
            PocketEcho = 0.90f,
            FormantDrift = 0.50f
        };

        // =================================================================
        // POCKET DIMENSION — Complete non-Euclidean server-wide dislocation
        // =================================================================
        public static readonly ScpVoicePreset PocketDimensionPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            IsGlobalTransmission = true,
            OutputGain = 2.85f,
            Pitch = 0.48f,
            Formant = 0.44f,
            DemonicOctaverMix = 0.45f,
            LowPass = 1250f,
            HighPass = 80f,
            Distortion = 0.45f,
            Guttural = 0.55f,
            Reverb = 0.80f,
            WetDecay = 0.85f,
            WetOrganic = 0.25f,
            PocketEcho = 1.00f,
            FormantDrift = 0.65f
        };

        // =================================================================
        // AMBIENT — Sensing rooms: looking through walls (Old: ATLAS DIMENSIONAL)
        // =================================================================
        public static readonly ScpVoicePreset AtlasDimensionalPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            IsGlobalTransmission = false,
            OutputGain = 2.55f,
            Pitch = 0.54f,
            Formant = 0.50f,
            LowPass = 1050f,
            Distortion = 0.35f,
            Guttural = 0.35f,
            Reverb = 0.50f,
            WetDecay = 0.80f,
            WetOrganic = 0.28f,
            PocketEcho = 0.85f,
            FormantDrift = 0.35f
        };

        // =================================================================
        // LOW VIGOR — Exhausted decay: heavy, collapsing larynx structural fatigue
        // =================================================================
        public static readonly ScpVoicePreset LowVigorPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            IsGlobalTransmission = false,
            OutputGain = 3.20f,
            Pitch = 0.45f,
            Formant = 0.40f,
            LowPass = 800f,
            Distortion = 0.30f,
            Guttural = 0.50f,
            Subharmonic = 0.35f,
            WetDecay = 1.00f,
            WetOrganic = 0.55f,
            PocketEcho = 0.70f,
            FormantDrift = 0.40f
        };

        // =================================================================
        // ATTACK — Combat execution: aggressive reality tearing compression
        // =================================================================
        public static readonly ScpVoicePreset AttackPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            IsGlobalTransmission = false,
            OutputGain = 2.75f,
            Pitch = 0.62f,
            Formant = 0.55f,
            HighPass = 220f,
            LowPass = 1600f,
            Distortion = 0.65f,
            Guttural = 0.65f,
            Subharmonic = 0.60f,
            WetDecay = 0.95f,
            WetOrganic = 0.45f,
            PocketEcho = 0.90f,
            FormantDrift = 0.55f
        };

        public static ScpVoicePreset GetPresetForState(Scp106VoiceState state) =>
            state switch
            {
                Scp106VoiceState.Idle => IdlePreset,
                Scp106VoiceState.Stalking => StalkingPreset,
                Scp106VoiceState.Emerging => EmergingPreset,
                Scp106VoiceState.PocketDimension => PocketDimensionPreset,
                Scp106VoiceState.AtlasDimensional => AtlasDimensionalPreset,
                Scp106VoiceState.LowVigor => LowVigorPreset,
                Scp106VoiceState.Attack => AttackPreset,
                _ => IdlePreset
            };
    }
}