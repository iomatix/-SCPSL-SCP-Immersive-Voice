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
            Pitch = 1.15f,          // Simulates hyper-extended, panicked vocal cords
            Formant = 0.85f,        // Compresses the throat cavity to remove authoritative lower presence
            Tremolo = 0.65f,        // Injects rapid amplitude instability to mimic rapid sobbing breaks
            FormantDrift = 0.60f,   // Emulates organic pitch weeping drift
            BreathNoise = 0.80f,    // Simulates severe hyperventilation under panic conditions
            WhisperAmount = 0.25f,  // Destroys harmonic lines to signify a failing, raw throat structure
            Distortion = 0.20f,     // Adds minimal organic throat friction
            WetOrganic = 0.45f,     // Simulates fluid buildup within the active vocal tract
            LowPass = 3800f         // Attenuates high air transients to close down the acoustic space
        };

        // =================================================================
        // CRYING — Total emotional collapse, heavy uncontrollable sobbing
        // =================================================================
        public static readonly ScpVoicePreset CryingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.35f,
            Pitch = 1.22f,          // Constricts the root fundamental to evoke total muscular despair
            Formant = 0.80f,        // Accents an unnatural, elongated skull resonance profile
            Tremolo = 0.88f,        // Induces maximum amplitude modulation to simulate violent weeping
            FormantDrift = 0.85f,   // Continuous phase tracking shifts to maximize the perception of agony
            BreathNoise = 1.10f,    // Forces intense gasping breath cycles into the transmission block
            WhisperAmount = 0.50f,  // Causes vocal folds to completely give out into pure air leaks
            Distortion = 0.30f,     // Models mucosal tearing textures
            WetOrganic = 0.65f,     // Severe fluid tracking representing heavy tear/mucus overflow
            WetDecay = 0.35f,       // Simulates a claustrophobic, reflective wet boundary surface
            LowPass = 3200f         // Targets and isolates vocal agony frequencies exclusively
        };

        // =================================================================
        // TRYING NOT TO CRY — Stifled panic, forcing words through muscle tension
        // =================================================================
        public static readonly ScpVoicePreset TryingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.40f,
            Pitch = 1.28f,          // Represents extreme vocal clamping via muscular constriction
            Formant = 0.88f,        // Maintained throat profile matching high psychological stress
            Tremolo = 0.40f,        // Suppressed, rigid stuttering instead of loose trembling
            FormantDrift = 0.45f,   // Stiff, strained tracking shifts
            BreathNoise = 0.65f,    // Simulates shallow, suffocated choke-breathing cycles
            WhisperAmount = 0.15f,  // Retains harmonic structure to model forcing speech out under duress
            Distortion = 0.45f,     // High saturation representing massive subglottic air pressure
            WetOrganic = 0.40f,     // Retains basic fluid wetness elements
            LowPass = 2800f         // Restricts upper harmonics to simulate a stifled, held-back delivery
        };

        // =================================================================
        // ENRAGING — Transition state, psychological snap, scream awakening
        // =================================================================
        public static readonly ScpVoicePreset EnragingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.50f,
            VocalShriek = 0.45f,     // Begins shifting input frequencies violently toward falsetto bands
            Pitch = 1.15f,          // Accelerates fundamental tone tension upwards
            Formant = 1.05f,        // Widens the throat track as the entity unhinges its jaw
            ScreechModulation = 0.40f, // Injects high-frequency structural decay components
            Distortion = 1.10f,     // Tearing saturation marking the onset of emotional breaking
            BreathNoise = 0.90f,    // Intense outward airflow volume acceleration
            HighPass = 200f         // Strips low proximity microphone mud to prioritize early edge clarity
        };

        // =================================================================
        // ENRAGED — Full primal scream, unhinged agonizing shriek wall
        // =================================================================
        public static readonly ScpVoicePreset RagePreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.60f,
            VocalShriek = 0.80f,     // Transposes the input signal and layers +12/+24 semitone shifts
            Pitch = 1.25f,          // Secondary pitch expansion stretching toward structural break limits
            Formant = 1.25f,        // Massive throat architecture simulation representing an unhinged jaw
            DemonicOctaverMix = 0.12f, // Placed deep into the noise floor to provide a subtle subconscious pomruk
            ScreechModulation = 0.85f, // Shakes upper frequency bands through non-harmonic sideband modulations
            Tremolo = 0.30f,        // Injects intense muscular shivering to the sustained screech wall
            Distortion = 1.95f,     // Hard polynomial clipping transforming the voice into a High Fry Scream
            Guttural = 0.35f,       // Subtle air friction texture to replicate throat abrasion
            BreathNoise = 1.20f,    // Overwhelming lung exhaustion pressure
            Reverb = 0.30f,         // Models hard acoustic boundary reflections matching concrete facility rooms
            HighPass = 150f,        // Restricts residual microphone mud while allowing the low pomruk through
            LowPass = 9500f         // Leaves the upper air band completely open for razor-sharp glass transients
        };

        // =================================================================
        // CHARGING — Furious kinetic assault, lung-tearing rampage
        // =================================================================
        public static readonly ScpVoicePreset ChargingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.50f,
            Pitch = 0.78f,          // Deepens the baseline to convey heavy forward kinetic momentum
            Formant = 0.65f,        // Modifies vocal tract resonance to reflect brute physical mass
            DemonicOctaverMix = 0.45f, // Stacks a pronounced monstrous presence layer during active locomotion
            Distortion = 1.60f,     // Heavy operational saturation overdrive
            Guttural = 0.70f,       // Adds distinct rasping friction matching persistent heavy exertion
            Subharmonic = 0.50f,    // Reinforces low-end frequencies to track heavy spatial movement
            BreathNoise = 1.40f,    // Maximum airflow clipping representing complete lung exhaustion
            HighPass = 120f         // Keeps the low-frequency physical energy path unimpeded
        };

        // =================================================================
        // CHARGED — Intercept focus, localized targeted impact
        // =================================================================
        public static readonly ScpVoicePreset ChargedPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.50f,
            Pitch = 0.82f,          // Slightly elevated pitch relative to charging, focusing energy
            Formant = 0.70f,        // Adjusts structural throat spacing for targeted projection
            DemonicOctaverMix = 0.40f, // Balanced sub-octave layer anchoring threat delivery
            Distortion = 1.40f,     // Sustained compression showing constant physical strain
            Guttural = 0.60f,       // Throat rasp tracking deceleration forces
            BreathNoise = 1.00f,    // Heavy breath component capturing structural decompression
            HighPass = 120f         // Constant low-end visibility pass
        };

        // =================================================================
        // PRYING GATE — Crushing blast doors, immense mechanical grunt
        // =================================================================
        public static readonly ScpVoicePreset PryingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.60f,
            Pitch = 0.65f,          // Forced to absolute biological floor limits to signal crushing physical strain
            Formant = 0.50f,        // Extreme expansion of the lower chest chamber resonance
            DemonicOctaverMix = 0.55f, // Multiplies demonic sub-layers to project immense physical force
            Distortion = 2.20f,     // Peak polynomial distortion representing maximum mechanical load leverage
            Guttural = 0.90f,       // Absolute maximum tissue abrasion texture simulating severe physical exertion
            Subharmonic = 0.80f,    // Generates high-energy concrete-shaking sub frequencies
            LowPass = 2600f         // Clamps upper frequency ranges to model a dense, heavy closed grunt
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