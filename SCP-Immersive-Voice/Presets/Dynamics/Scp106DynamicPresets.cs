namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// AAA Dynamic identity states for SCP-106, tuned for severe decayed resonance,
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
            IsGlobalTransmission = false, // Proximity directional 3D audio boundary
            OutputGain = 2.35f,
            Pitch = 0.52f,          // Ancient, sub-octave dimensional depth
            Formant = 0.46f,        // Collapsed, rotted vocal cavity architecture
            Distortion = 0.70f,     // AAA SHARPNESS FIX: Harder waveshaping saturation to add harmonic bite
            Guttural = 0.48f,       // AAA SHARPNESS FIX: Severe false-vocal cord rasp (corroded throat grit)
            FleshCrackle = 0.25f,   // AAA SHARPNESS FIX: Liquid tissue degradation transient bubbling
            LowPass = 1150f,        // AAA SHARPNESS FIX: Opened cutoff window to let sharp abrasive grit slice through
            Reverb = 0.40f,
            WetDecay = 0.90f,       // Visceral viscous absorption (slime-coated walls)
            WetOrganic = 0.35f,     // Slimy throat decomposition mechanics
            PocketEcho = 0.80f,     // Non-Euclidean phase-inversion echo matrix
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
            OutputGain = 2.10f,
            Pitch = 0.48f,          // Voice sinks further into the substrate weight
            Formant = 0.42f,        // Extreme structural compression
            LowPass = 550f,         // Drastic cutoff simulating transmission through solid concrete/soil layers
            HighPass = 0f,
            Distortion = 0.45f,
            WetDecay = 1.00f,       // Absolute absorption, no air reflections can escape the solid mass
            WetOrganic = 0.65f,     // Submerged vocal tract choking on corrosion fluid
            PocketEcho = 0.40f,     // Dimensional link is muffled by local real-world density
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
            OutputGain = 2.50f,
            Pitch = 0.58f,
            Formant = 0.52f,
            LowPass = 1400f,        // Sudden acoustic expansion as the entity breaks into open air
            Distortion = 0.90f,     // Tearing saturation modeling the spatial rupture
            Guttural = 0.75f,       // Violent, ragged false-vocal cord rasp (spitting black mucosal bile)
            FleshCrackle = 0.40f,   // Rapid wet popping sound as reality layers tear apart
            WetDecay = 1.00f,       // Exploding, dripping liquid transients
            WetOrganic = 0.70f,
            PocketEcho = 0.90f,
            FormantDrift = 0.50f    // Extreme thermal/spectral instability during phase transit
        };

        // =================================================================
        // POCKET DIMENSION — Complete non-Euclidean server-wide dislocation
        // =================================================================
        public static readonly ScpVoicePreset PocketDimensionPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            IsGlobalTransmission = true, // AAA PLUG-N-PLAY ACTIVATION: Broadcasts server-wide to all live clients!
            OutputGain = 2.55f,     // Boosted headroom for maximum global intelligibility
            Pitch = 0.48f,          // Deeper pitch floor inside his own core reality domain
            Formant = 0.44f,
            LowPass = 1250f,        // AAA SHARPNESS FIX: Opened cutoff so the global voice sounds clean and cutting
            HighPass = 80f,         // Strip rumble mud out of global broadcast
            Distortion = 0.75f,     // Heavy asymmetric overdrive edge
            Guttural = 0.55f,       // AAA SHARPNESS FIX: Direct larynx mucosal abrasion
            Reverb = 0.80f,         // Massive, hollow space diffusion with maximum absorption loss
            WetDecay = 0.85f,
            WetOrganic = 0.25f,
            PocketEcho = 1.00f,     // Absolute phase inversion feedback, stripping local directional cues
            FormantDrift = 0.65f    // Floating LFO center formants, making the sound morph across the listener's head
        };

        // =================================================================
        // ATLAS DIMENSIONAL — Sensing rooms: looking through walls
        // =================================================================
        public static readonly ScpVoicePreset AtlasDimensionalPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            IsGlobalTransmission = false,
            OutputGain = 2.25f,
            Pitch = 0.54f,
            Formant = 0.50f,
            LowPass = 1050f,        // Slightly enhanced boundary visibility passthrough
            Distortion = 0.55f,
            Guttural = 0.35f,       // Subdued mechanical rasp while focused
            Reverb = 0.50f,
            WetDecay = 0.80f,
            WetOrganic = 0.28f,
            PocketEcho = 0.85f,     // Steady dimensional background bleed
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
            OutputGain = 1.95f,
            Pitch = 0.45f,          // Pitch drops to an absolute biological minimum due to loss of nerve energy
            Formant = 0.40f,        // Drooping, elongated, tired throat volume
            LowPass = 800f,
            Distortion = 0.50f,
            Guttural = 0.50f,       // Wet, tired rattling in the larynx
            Subharmonic = 0.35f,    // Static sub-bass weight dragging down the speech delivery
            WetDecay = 1.00f,       // Thick, static accumulation of sludge
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
            OutputGain = 2.45f,
            Pitch = 0.62f,          // Aggressive surge
            Formant = 0.55f,
            HighPass = 220f,        // Remove proximity mud to amplify sharp cutting edges
            LowPass = 1600f,        // Brightest open window for 106 to scream
            Distortion = 1.40f,     // Intense waveshaping clipping (tearing sound)
            Guttural = 0.65f,       // Corroded throat roar rasp
            Subharmonic = 0.60f,    // Massive cinematic low-end punch to sell kinetic threat
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