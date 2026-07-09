namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Dynamic identity states for SCP-939, tuned for biomorphic vocal camouflage, predatory stealth,
    /// and visceral, flesh-tearing physical execution roars.
    /// Perfectly calibrated for the stateful, zero-allocation multi-band DSP pipeline matrix.
    /// </summary>
    public static class Scp939DynamicPresets
    {
        // =================================================================
        // IDLE WHISPER — Signature biomorphic unvoiced camouflage baseline
        // =================================================================
        public static readonly ScpVoicePreset IdleWhisperPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 3.20f,
            Pitch = 0.55f,
            Formant = 0.65f,
            LowPass = 2400f,
            HighPass = 260f,
            PredatoryCamouflage = 0.65f,
            BreathNoise = 0.25f,
            WetOrganic = 0.45f,
            FormantDrift = 0.22f,
            Distortion = 0.08f
        };

        // =================================================================
        // MIMICKING — Deceptive human speech with a creepy Uncanny Valley leak
        // =================================================================
        public static readonly ScpVoicePreset MimickingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.45f,
            Pitch = 0.98f,
            Formant = 1.00f,
            LaryngealAsymmetry = 0.35f,
            LowPass = 5200f,
            HighPass = 0f,
            PredatoryCamouflage = 0.15f,
            BreathNoise = 0.06f,
            WetOrganic = 0.12f,
            FormantDrift = 0.08f,
            Reverb = 0.05f
        };

        // =================================================================
        // FOCUSED — Predator stealth: cold, sharpened, highly directional whisper
        // =================================================================
        public static readonly ScpVoicePreset FocusedPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 3.65f,
            Pitch = 0.58f,
            Formant = 0.68f,
            LaryngealAsymmetry = 0.60f,
            HighPass = 800f,
            LowPass = 2800f,
            PredatoryCamouflage = 0.85f,
            BreathNoise = 0.35f,
            WetOrganic = 0.50f,
            Distortion = 0.15f,
            FormantDrift = 0.25f
        };

        // =================================================================
        // ATTACKING — Combat execution: unhinged, fluid-choked biological roar
        // =================================================================
        public static readonly ScpVoicePreset AttackingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.90f,
            Pitch = 0.68f,
            Formant = 0.62f,
            DeathRattle = 0.40f,
            Guttural = 0.55f,
            DemonicOctaverMix = 0.45f,
            Distortion = 0.75f,
            PredatoryCamouflage = 0.35f,
            BreathNoise = 0.75f,
            WetOrganic = 0.75f,
            WetDecay = 0.42f,
            HighPass = 150f,
            FormantDrift = 0.45f
        };

        // =================================================================
        // AMNESTIC CLOUD — Releasing fog: detached, hallucinogenic, smeared presence
        // =================================================================
        public static readonly ScpVoicePreset AmnesticCloudPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            IsGlobalTransmission = true,
            OutputGain = 2.80f,
            Pitch = 0.85f,
            Formant = 0.80f,
            LowPass = 1200f,
            HighPass = 0f,
            Reverb = 0.55f,
            PredatoryCamouflage = 0.45f,
            BreathNoise = 0.18f,
            WetOrganic = 0.35f,
            FormantDrift = 0.30f,
            Distortion = 0.05f
        };

        public static ScpVoicePreset GetPresetForState(Scp939VoiceState state) =>
            state switch
            {
                Scp939VoiceState.IdleWhisper => IdleWhisperPreset,
                Scp939VoiceState.Mimicking => MimickingPreset,
                Scp939VoiceState.Focused => FocusedPreset,
                Scp939VoiceState.Attacking => AttackingPreset,
                Scp939VoiceState.AmnesticCloud => AmnesticCloudPreset,
                _ => IdleWhisperPreset
            };
    }
}