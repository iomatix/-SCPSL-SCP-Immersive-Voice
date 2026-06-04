namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// AAA Dynamic identity states for SCP‑106, tuned for severe decayed resonance,
    /// non-Euclidean phase dislocation, and necrotic wet presence.
    /// Perfectly calibrated for the stateful, zero-allocation multi-band DSP pipeline matrix.
    /// </summary>
    public static class Scp106DynamicPresets
    {
        // =================================================================
        // IDLE — Baseline decay: old, wet, heavy presence (Matches core baseline)
        // =================================================================
        public static readonly ScpVoicePreset IdlePreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.25f,
            Pitch = 0.52f,          // Intent: ancient, sub-octave dimensional depth
            Formant = 0.48f,        // Intent: collapsed, rotted vocal cavity architecture
            Distortion = 0.65f,     // Intent: severe acidic corrosion texture destroying wave boundaries
            LowPass = 850f,         // Intent: extreme, suffocating damp mud and dirt muffling
            Reverb = 0.40f,
            WetDecay = 0.95f,       // Intent: visceral viscous absorption (slime-coated walls)
            WetOrganic = 0.35f,     // Intent: slimy throat decomposition mechanics
            PocketEcho = 0.85f,     // Intent: non-Euclidean phase-inversion echo matrix
            FormantDrift = 0.30f
        };

        // =================================================================
        // STALKING — Submerged state: completely buried under solid matter
        // =================================================================
        public static readonly ScpVoicePreset StalkingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.10f,
            Pitch = 0.48f,          // Intent: Voice sinks further into the substrate weight
            Formant = 0.42f,        // Intent: Extreme structural compression
            LowPass = 500f,         // AAA FIX: Drastic cutoff simulating transmission through solid concrete/soil layers
            HighPass = 0f,
            Distortion = 0.40f,
            WetDecay = 1.00f,       // Intent: Absolute absorption, no air reflections can escape the solid mass
            WetOrganic = 0.60f,     // Intent: Submerged vocal tract choking on corrosion fluid
            PocketEcho = 0.40f,     // Intent: Dimensional link is muffled by local real-world density
            FormantDrift = 0.20f
        };

        // =================================================================
        // EMERGING — Rising from solid mass: wet burst, physical rupture of reality
        // =================================================================
        public static readonly ScpVoicePreset EmergingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.40f,
            Pitch = 0.58f,
            Formant = 0.52f,
            LowPass = 1200f,        // Intent: Sudden acoustic expansion as the entity breaks into open air
            Distortion = 0.85f,     // Intent: Tearing saturation modeling the spatial rupture
            Guttural = 0.60f,       // AAA FIX: Violent false-vocal cord rasp (spitting black mucosal bile)
            WetDecay = 1.00f,       // Intent: Exploding, dripping liquid transients
            WetOrganic = 0.70f,
            PocketEcho = 0.90f,
            FormantDrift = 0.50f    // Intent: Extreme thermal/spectral instability during phase transit
        };

        // =================================================================
        // POCKET DIMENSION — Complete non-Euclidean phase dislocation
        // =================================================================
        public static readonly ScpVoicePreset PocketDimensionPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.30f,
            Pitch = 0.50f,
            Formant = 0.45f,
            LowPass = 750f,         // Intent: Suffocating, thick pocket decay ambience
            HighPass = 0f,
            Distortion = 0.70f,
            Reverb = 0.75f,         // Intent: Massive, hollow space diffusion with maximum absorption loss
            WetDecay = 0.90f,
            WetOrganic = 0.30f,
            PocketEcho = 1.00f,     // AAA FIX: Absolute phase inversion feedback, stripping local directional cues
            FormantDrift = 0.65f    // AAA FIX: Floating LFO center formants, making the sound morph across the room
        };

        // =================================================================
        // ATLAS DIMENSIONAL — Sensing rooms: looking through walls
        // =================================================================
        public static readonly ScpVoicePreset AtlasDimensionalPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.20f,
            Pitch = 0.54f,
            Formant = 0.50f,
            LowPass = 950f,
            Distortion = 0.50f,
            Reverb = 0.50f,
            WetDecay = 0.80f,
            WetOrganic = 0.28f,
            PocketEcho = 0.85f,     // Intent: Steady dimensional background bleed
            FormantDrift = 0.35f
        };

        // =================================================================
        // LOW VIGOR — Exhausted decay: heavy, collapsing larynx structural fatigue
        // =================================================================
        public static readonly ScpVoicePreset LowVigorPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 1.90f,
            Pitch = 0.45f,          // AAA FIX: Pitch drops to an absolute biological minimum due to loss of nerve energy
            Formant = 0.40f,        // Intent: Drooping, elongated, tired throat volume
            LowPass = 700f,
            Distortion = 0.45f,
            Guttural = 0.40f,       // Intent: Heavy, tired rattling in the larynx
            Subharmonic = 0.35f,    // Intent: Static sub-bass weight dragging down the speech delivery
            WetDecay = 1.00f,       // Intent: Thick, static accumulation of sludge
            WetOrganic = 0.50f,
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
            OutputGain = 2.35f,
            Pitch = 0.62f,          // Intent: Aggressive surge
            Formant = 0.55f,
            HighPass = 250f,        // Intent: Remove proximity mud to amplify sharp cutting edges
            LowPass = 1500f,        // Intent: Brightest open window for 106 to scream
            Distortion = 1.35f,     // Intent: High waveshaping clipping (tearing sound)
            Guttural = 0.55f,       // Intent: Corroded throat roar rasp
            Subharmonic = 0.60f,    // AAA FIX: Pristine, massive cinematic low-end punch to sell kinetic threat
            WetDecay = 0.95f,
            WetOrganic = 0.45f,
            PocketEcho = 0.90f,
            FormantDrift = 0.55f
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