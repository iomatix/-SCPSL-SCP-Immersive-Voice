namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Dynamic emotional states for SCP-096, tuned for staggering psychological instability,
    /// pathetic fluid-choked sobbing, and monstrous, unhinged demonic rage walls.
    /// Perfectly balanced for the stateful, zero-allocation multi-band DSP pipeline matrix.
    /// </summary>
    public static class Scp096DynamicPresets
    {
        // =================================================================
        // CALM — Fragile, miserable, constantly trembling baseline wail
        // =================================================================
        public static readonly ScpVoicePreset CalmPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.45f,
            Pitch = 1.15f,          // Panicked, hyper-extended vocal cords
            Formant = 0.85f,        // Hollow, shrunken throat architecture
            Tremolo = 0.65f,        //  CRY FIX: Rapid amplitude modulation simulating structural voice breaks
            FormantDrift = 0.60f,
            BreathNoise = 0.80f,    // Heavy, persistent weeping hyperventilation
            WhisperAmount = 0.25f,  // Partially de-voiced harmonic lines to mimic a failing throat
            Distortion = 0.20f,
            WetOrganic = 0.45f,     // High saliva/tear volume saturation in the larynx
            LowPass = 3800f         // Muffle transients to enforce a closed, pathetic profile
        };

        // =================================================================
        // CRYING — Total emotional collapse, heavy uncontrollable sobbing
        // =================================================================
        public static readonly ScpVoicePreset CryingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.35f,
            Pitch = 1.22f,          // Throat constricts completely under overwhelming sorrow
            Formant = 0.80f,        // Elongated, alien-like facial bone resonance
            Tremolo = 0.88f,        //  CRY FIX: MAXIMUM TREMOLO: Violent, agonizing shaking of the voice (pure weeping)
            FormantDrift = 0.85f,
            BreathNoise = 1.10f,    // Violent gasping air intakes choking the speech package
            WhisperAmount = 0.50f,  // Vocal cords completely snap, dropping segments into pure air leaks
            Distortion = 0.30f,
            WetOrganic = 0.65f,     // Choking on visceral mucosal fluids and lacrimal overflow
            WetDecay = 0.35f,
            LowPass = 3200f         // Deeply isolated vocal agony
        };

        // =================================================================
        // TRYING NOT TO CRY — Stifled panic, forcing words through muscle tension
        // =================================================================
        public static readonly ScpVoicePreset TryingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.40f,
            Pitch = 1.28f,          // Extreme vocal compression from muscle clamping
            Formant = 0.88f,
            Tremolo = 0.40f,        // Rigid, suppressed stuttering shaking
            FormantDrift = 0.45f,
            BreathNoise = 0.65f,    // Suffocated, shallow breath choke
            WhisperAmount = 0.15f,
            Distortion = 0.45f,     // High vocal pressure saturation
            WetOrganic = 0.40f,
            LowPass = 2800f
        };

        // =================================================================
        // ENRAGING — Transition state, psychological snap, scream awakening
        // =================================================================
        public static readonly ScpVoicePreset EnragingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.50f,
            Pitch = 1.20f,          // Sliding upwards to an ear-piercing frequency floor
            Formant = 1.10f,        // Jaw unhinging, opening the vocal column
            ScreechModulation = 0.50f, // Introduction of high-register tissue breakdown
            Distortion = 1.10f,     // Tearing compression
            BreathNoise = 0.90f,
            HighPass = 250f         // Strip lower chest mud completely
        };

        // =================================================================
        // ENRAGED — Full primal scream, unhinged agonizing shriek wall
        // =================================================================
        public static readonly ScpVoicePreset RagePreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.65f,      // Commands total dominance over local VoIP mix
            Pitch = 1.32f,          // AAA SCREECH FIX: High-frequency, tense human-breaking pitch limit
            Formant = 1.20f,        // AAA SCREECH FIX: Maximally stretched jaw-dropped throat shape
            ScreechModulation = 0.85f, // AAA SCREECH FIX: Triggers dynamic inharmonic screech modulation for agonizing bite
            Tremolo = 0.35f,        // Adds physical muscular shuddering to the continuous scream wall
            Distortion = 1.65f,     // High polynomial overdrive for raw vocal strain
            Guttural = 0.45f,       // Subtle larynx sandpaper abrasion, keeping the focus on the high screech
            BreathNoise = 1.15f,    // Overwhelming lung pressure
            Reverb = 0.30f,         // Echoes bouncing off facility concrete containment walls
            HighPass = 3800f,       // AAA SCREECH FIX: Muffle any residual sub-bass entirely to focus 100% of power into the piercing shriek
            LowPass = 8500f         // Keep high crystal transients wide open
        };

        // =================================================================
        // CHARGING — Furious kinetic assault, lung-tearing rampage
        // =================================================================
        public static readonly ScpVoicePreset ChargingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.50f,
            Pitch = 0.78f,          // Beastly velocity
            Formant = 0.65f,
            DemonicOctaverMix = 0.45f,     // Subdued demon tracking during the sprint
            Distortion = 1.60f,
            Guttural = 0.70f,
            Subharmonic = 0.50f,
            BreathNoise = 1.40f,    // Maximum airflow exhaustion clipping
            HighPass = 120f
        };

        // =================================================================
        // CHARGED — Intercept focus, localized targeted impact
        // =================================================================
        public static readonly ScpVoicePreset ChargedPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.50f,
            Pitch = 0.82f,
            Formant = 0.70f,
            DemonicOctaverMix = 0.40f,
            Distortion = 1.40f,
            Guttural = 0.60f,
            BreathNoise = 1.00f,
            HighPass = 120f
        };

        // =================================================================
        // PRYING GATE — Crushing blast doors, immense mechanical grunt
        // =================================================================
        public static readonly ScpVoicePreset PryingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.60f,
            Pitch = 0.65f,          // Dropped to absolute biological baseline force limits
            Formant = 0.50f,
            DemonicOctaverMix = 0.55f,     // Double demonic thickness under structural load
            Distortion = 2.20f,     // Extreme physical overdrive clipping
            Guttural = 0.90f,       // Absolute maximum throat tissue friction
            Subharmonic = 0.80f,    // Cinematic concrete-shaking sub frequency output
            LowPass = 2600f
        };

        public static ScpVoicePreset GetPresetForState(Scp096VoiceState state)
        {
            switch (state)
            {
                case Scp096VoiceState.Calm:
                    return CalmPreset;

                case Scp096VoiceState.Crying:
                    return CryingPreset;

                case Scp096VoiceState.TryingNotToCry:
                    return TryingPreset;

                case Scp096VoiceState.Enraging:
                    return EnragingPreset;

                case Scp096VoiceState.Enraged:
                    return RagePreset;

                case Scp096VoiceState.Charging:
                    return ChargingPreset;

                case Scp096VoiceState.Charged:
                    return ChargedPreset;

                case Scp096VoiceState.PryingGate:
                    return PryingPreset;

                default:
                    return CalmPreset;
            }
        }
    }
}