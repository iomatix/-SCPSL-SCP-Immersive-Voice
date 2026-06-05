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
            Tremolo = 0.65f,        // AAA CRY FIX: Rapid amplitude modulation simulating structural voice breaks
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
            Tremolo = 0.88f,        // AAA CRY FIX: MAXIMUM TREMOLO: Violent, agonizing shaking of the voice (pure weeping)
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
        // ENRAGING — Transition state, psychological snap, roar awakening
        // =================================================================
        public static readonly ScpVoicePreset EnragingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.55f,
            Pitch = 1.00f,          // Shifting down from scream to raw power
            Formant = 1.10f,        // Jaw begins to unhinge, widening throat column
            Distortion = 1.35f,     // Tearing saturation representing internal tissue destruction
            Guttural = 0.50f,
            Subharmonic = 0.45f,    // Awakening low frequencies
            BreathNoise = 0.90f,
            HighPass = 200f
        };

        // =================================================================
        // ENRAGED — Full primal roar, non-human cinema demon entity
        // =================================================================
        public static readonly ScpVoicePreset RagePreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.70f,      // Overwhelming mix presence to command terror
            Pitch = 0.72f,          // AAA DEMON FIX: Dropped to a terrifying beastly baritone level
            Formant = 0.60f,        // AAA DEMON FIX: Elongated gargantuan neck chest chamber profile
            DemonicOctaverMix = 0.65f,     // AAA DEMON FIX: Integrates DemonicOctaverEffect for that dual-tone movie devil roar
            Distortion = 1.85f,     // Violent waveshaping distortion wall
            Guttural = 0.85f,       // Complete ventricular fold rupture texture (massive animalistic rasp)
            Subharmonic = 0.60f,    // Extreme low-end chest reinforcement
            BreathNoise = 1.20f,    // Unstoppable engine-like lung exhaustion pressure
            Reverb = 0.35f,
            HighPass = 100f,        // Open up the response floor to unleash the demonic octaver layer
            LowPass = 6500f
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