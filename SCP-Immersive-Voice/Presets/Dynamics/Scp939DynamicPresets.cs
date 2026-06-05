namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Dynamic emotional states for SCP-939, tuned for biomorphic vocal camouflage, predatory stealth,
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
            OutputGain = 1.85f,
            Pitch = 0.55f,          // Predatory chest fundamental slowdown
            Formant = 0.65f,        // Elongated reptilian throat volume
            LowPass = 2400f,
            HighPass = 260f,
            PredatoryCamouflage = 0.65f,  // Converts speech into a creepy, unvoiced friction whisper
            BreathNoise = 0.25f,    // Soft, rhythmic lung-air ventilation
            WetOrganic = 0.45f,     // Saliva coating inside the split jaw
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
            OutputGain = 2.00f,
            Pitch = 0.98f,          // Matches standard human vocal chord vibration rates
            Formant = 1.00f,        // Standard human larynx simulation
            LaryngealAsymmetry = 0.35f, // UNCANNY FIX: Microscopic phase drift representing non-human asymmetric throat muscles
            LowPass = 5200f,
            HighPass = 0f,
            PredatoryCamouflage = 0.15f,
            BreathNoise = 0.06f,
            WetOrganic = 0.12f,     // Slight fluid stickiness under the stolen skin
            FormantDrift = 0.08f,   // Unstable throat muscle imitation drift
            Reverb = 0.05f
        };

        // =================================================================
        // FOCUSED — Predator stealth: cold, sharpened, highly directional whisper
        // =================================================================
        public static readonly ScpVoicePreset FocusedPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.10f,
            Pitch = 0.58f,
            Formant = 0.68f,
            LaryngealAsymmetry = 0.60f, // UNCANNY FIX: Strips human cues further to signify pure predatory intent
            HighPass = 800f,        // Cuts chest thuds entirely to isolate cold, airy sibilants
            LowPass = 2800f,
            PredatoryCamouflage = 0.85f,  // Absolute unvoiced whisper replacement
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
            OutputGain = 2.30f,
            Pitch = 0.68f,
            Formant = 0.62f,
            DeathRattle = 0.40f,    // BIOLOGY FIX: Adds visceral, waterlogged trachea fluid gurgling
            Guttural = 0.55f,       // Aggressive ventricular fold abrasion rasping
            DemonicOctaverMix = 0.45f, // Sub-octave chest reinforcement for kinetic threat
            Distortion = 1.20f,     // Screaming operational laryngeal strain
            PredatoryCamouflage = 0.35f,
            BreathNoise = 0.75f,    // Severe, suffocating lung pressure exhaust
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
            IsGlobalTransmission = true, // Global Audio
            OutputGain = 1.90f,
            Pitch = 0.85f,
            Formant = 0.80f,
            LowPass = 1200f,        // Cuts high components to simulate sound travelling through dense gas
            HighPass = 0f,
            Reverb = 0.55f,         // Smeared spatial room reflections representing disorientation
            PredatoryCamouflage = 0.45f,
            BreathNoise = 0.18f,
            WetOrganic = 0.35f,
            FormantDrift = 0.30f,
            Distortion = 0.05f
        };

        public static ScpVoicePreset GetPresetForState(Scp939VoiceState state)
        {
            switch (state)
            {
                case Scp939VoiceState.IdleWhisper:
                    return IdleWhisperPreset;

                case Scp939VoiceState.Mimicking:
                    return MimickingPreset;

                case Scp939VoiceState.Focused:
                    return FocusedPreset;

                case Scp939VoiceState.Attacking:
                    return AttackingPreset;

                case Scp939VoiceState.AmnesticCloud:
                    return AmnesticCloudPreset;

                default:
                    return IdleWhisperPreset;
            }
        }
    }
}