namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Dynamic identity states for SCP‑3114, tuned for skeletal friction, twitching tissue,
    /// and uncanny psychoacoustic transitions between stolen human form and pure bone monstrosity.
    /// Perfectly calibrated for the stateful, zero-allocation multi-band DSP pipeline matrix.
    /// </summary>
    public static class Scp3114DynamicPresets
    {
        // =================================================================
        // UNDISGUISED — True skeletal form: high-tension, cracking calcium mass
        // =================================================================
        public static readonly ScpVoicePreset UndisguisedPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.45f,
            Pitch = 1.28f,
            Formant = 1.15f,
            DryCrackle = 0.42f,
            FleshCrackle = 0.35f,
            FormantDrift = 0.30f,
            Distortion = 0.12f,
            HighPass = 200f,
            Reverb = 0.12f
        };

        // =================================================================
        // DISGUISING — Squeezing into stolen skin: meaty, structural collapse
        // =================================================================
        public static readonly ScpVoicePreset DisguisingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.50f,
            Pitch = 1.15f,
            Formant = 1.02f,
            FleshCrackle = 0.55f,
            DryCrackle = 0.30f,
            WetOrganic = 0.50f,
            FormantDrift = 0.35f,
            Distortion = 0.18f,
            LowPass = 2200f,
            WetDecay = 0.35f
        };

        // =================================================================
        // DISGUISED — Perfect human mimicry with an unsettling Uncanny Valley leak
        // =================================================================
        public static readonly ScpVoicePreset DisguisedPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.00f,
            Pitch = 1.01f,
            Formant = 1.00f,
            Distortion = 0.00f,
            LowPass = 0f,
            HighPass = 0f,
            FormantDrift = 0.08f,
            FleshCrackle = 0.05f,
            DryCrackle = 0.02f,
            WetOrganic = 0.08f
        };

        // =================================================================
        // REVEALING — Ripping the disguise apart: tearing skin, exploding bone strain
        // =================================================================
        public static readonly ScpVoicePreset RevealingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.55f,
            Pitch = 1.35f,
            Formant = 1.10f,
            FleshCrackle = 0.60f,
            DryCrackle = 0.45f,
            WetOrganic = 0.55f,
            FormantDrift = 0.40f,
            Distortion = 0.25f,
            LowPass = 2600f,
            WetDecay = 0.45f
        };

        // =================================================================
        // STRANGLING — Kinetic execution: raw bone leverage crushing throat structures
        // =================================================================
        public static readonly ScpVoicePreset StranglingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.65f,
            Pitch = 1.40f,
            Formant = 1.20f,
            DryCrackle = 0.55f,
            FleshCrackle = 0.50f,
            Guttural = 0.50f,
            Distortion = 0.35f,
            FormantDrift = 0.35f,
            WetOrganic = 0.45f,
            HighPass = 300f,
            WetDecay = 0.50f
        };

        public static ScpVoicePreset GetPresetForState(Scp3114VoiceState state) =>
            state switch
            {
                Scp3114VoiceState.Undisguised => UndisguisedPreset,
                Scp3114VoiceState.Disguising => DisguisingPreset,
                Scp3114VoiceState.Disguised => DisguisedPreset,
                Scp3114VoiceState.Revealing => RevealingPreset,
                Scp3114VoiceState.Strangling => StranglingPreset,
                _ => UndisguisedPreset
            };
    }
}