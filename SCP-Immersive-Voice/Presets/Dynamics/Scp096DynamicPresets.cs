namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// AAA Dynamic emotional states for SCP‑096, tuned for staggering psychological instability and escalating rage.
    /// Perfectly balanced for the stateful, zero-allocation multi-band DSP pipeline matrix.
    /// </summary>
    public static class Scp096DynamicPresets
    {
        // =================================================================
        // CALM — Fragile, trembling baseline (Matches our core default preset)
        // =================================================================
        public static readonly ScpVoicePreset CalmPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.4f,
            Pitch = 1.06f,          // Intent: high-tension, panicked vocal cords
            Formant = 0.88f,        // Intent: hollow, unnatural vocal tract scaling
            FormantDrift = 0.70f,   // Intent: Severe laryngeal wailing/trembling emulation
            BreathNoise = 0.75f,    // Intent: High-intensity hyperventilation from constant weeping
            WhisperAmount = 0.35f,  // Intent: De-voice chord segments to break human speech continuity
            Distortion = 0.22f,     // Intent: Organic laryngeal strain
            WetOrganic = 0.38f,     // Intent: High saliva/tear saturation in the throat
            LowPass = 4200f         // Intent: Soften high transients for a miserable, closed character
        };

        // =================================================================
        // CRYING — Total emotional collapse, heavy uncontrollable sobbing
        // =================================================================
        public static readonly ScpVoicePreset CryingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.25f,
            Pitch = 1.12f,          // Intent: Throat constricts further from acute despair
            Formant = 0.85f,        // Intent: More pronounced elongated skull resonance
            FormantDrift = 0.85f,   // Intent: MAXIMUM TREMOLO: Extreme, violent shaking of the voice pitch
            BreathNoise = 0.95f,    // Intent: Violent, gasping intakes of air between words
            WhisperAmount = 0.55f,  // Intent: Vocal chords fully give out, plunging voice into terrifying whispers
            Distortion = 0.28f,     // Intent: Increased mucosal tearing texture
            WetOrganic = 0.55f,     // Intent: Choking on fluids and lacrimal overflow
            WetDecay = 0.25f,       // Intent: Claustrophobic, wet echo trail of a small crying space
            LowPass = 3400f         // Intent: Muffle high components to isolate vocal agony
        };

        // =================================================================
        // TRYING NOT TO CRY — Choked throat tension, forcing words through panic
        // =================================================================
        public static readonly ScpVoicePreset TryingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.35f,
            Pitch = 1.18f,          // Intent: Peak muscular constriction (extreme high tension)
            Formant = 0.90f,
            FormantDrift = 0.45f,   // Intent: Rigid, stiff wailing instead of loose trembling
            BreathNoise = 0.60f,    // Intent: Suffocated, shallow breathing cycles
            WhisperAmount = 0.15f,  // Intent: Forcing real voice out with immense physical strain
            Distortion = 0.45f,     // Intent: Heavy analog saturation representing extreme vocal chord pressure
            WetOrganic = 0.48f,
            LowPass = 2800f         // Intent: Stifled, compressed delivery
        };

        // =================================================================
        // ENRAGING — Transition state, psychological snap, rage awakening
        // =================================================================
        public static readonly ScpVoicePreset EnragingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.50f,
            Pitch = 1.25f,          // Intent: Shifting gears into an ear-piercing scream pitch
            Formant = 1.05f,        // Intent: Jaw unhinging, widening spectral profile
            FormantDrift = 0.40f,
            BreathNoise = 0.85f,    // Intent: Furious airflow rushing outward
            Distortion = 1.20f,     // Intent: Tearing transition saturation (vocal destruction begins)
            Guttural = 0.35f,       // Intent: Introduction of primal false-vocal fold rasp
            Subharmonic = 0.40f,    // Intent: Waking up chest sub-frequencies to give the scream mass
            WetDecay = 0.40f,
            HighPass = 300f         // Intent: Strip low microphone mud to prioritize sharp edges
        };

        // =================================================================
        // ENRAGED — Full primal scream, unhinged monstrous wall of sound
        // =================================================================
        public static readonly ScpVoicePreset RagePreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.65f,      // Intent: Maximum presence to dominate the local VoIP mix
            Pitch = 1.35f,          // Intent: Terrifying, piercing, high-frequency human-breaking scream
            Formant = 1.15f,        // Intent: Fully stretched, agonizing jaw-dropped throat shape
            FormantDrift = 0.60f,   // Intent: Chaotic, dynamic distortion of center formant frequencies
            Distortion = 2.30f,     // Intent: Extreme tearing soft-clipping amplification
            Guttural = 0.75f,       // Intent: Complete ventricular fold disintegration texture (gargantuan roar)
            Subharmonic = 0.65f,    // Intent: MASSIVE CHEST GROWL: Backs up the high scream with pristine sub-harmonics
            BreathNoise = 1.10f,    // Intent: Overwhelming, clipped lung exhaustion pressure
            Reverb = 0.25f,         // Intent: Echoes bouncing off containment facilities
            HighPass = 450f         // Intent: Cut lower rumble completely, forcing the scream to pierce like wood-saw
        };

        // =================================================================
        // CHARGING — Furious bull-rush sprinting, lung-tearing exhaustion
        // =================================================================
        public static readonly ScpVoicePreset ChargingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.45f,
            Pitch = 1.28f,
            Formant = 1.10f,
            FormantDrift = 0.50f,
            Distortion = 1.80f,     // Intent: Severe operational overdrive
            Guttural = 0.60f,       // Intent: Persistent breathing rasp
            Subharmonic = 0.50f,    // Intent: Deep kinetic momentum weight
            BreathNoise = 1.35f,    // Intent: MAXIMUM AIRFLOW: Absolute hyperventilation during the sprint
            HighPass = 400f
        };

        // =================================================================
        // CHARGED — Intercept focus, localized targeted fury
        // =================================================================
        public static readonly ScpVoicePreset ChargedPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.50f,
            Pitch = 1.22f,
            Formant = 1.06f,
            FormantDrift = 0.40f,
            Distortion = 1.55f,
            Guttural = 0.50f,
            Subharmonic = 0.40f,
            BreathNoise = 0.95f,
            HighPass = 350f
        };

        // =================================================================
        // PRYING GATE — Tearing through blast doors, crushing kinetic grunt
        // =================================================================
        public static readonly ScpVoicePreset PryingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.55f,
            Pitch = 0.90f,          // Intent: Dropping pitch due to extreme structural/physical load
            Formant = 0.78f,        // Intent: Deep, compressed gargantuan grunt resonance
            Distortion = 2.10f,     // Intent: Extreme physical exertion overdrive
            Guttural = 0.85f,       // Intent: Peak larynx friction texturing
            Subharmonic = 0.75f,    // Intent: Maximum chest-rattling sub frequency output to simulate force
            LowPass = 2400f         // Intent: Compressed, dense closed mouth exertion
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