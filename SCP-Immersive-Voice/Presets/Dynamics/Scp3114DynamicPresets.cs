namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// AAA Dynamic identity states for SCP‑3114, tuned for skeletal friction, twitching tissue,
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
            Pitch = 1.45f,          // Intent: frantic, high-velocity nerve tension frequency
            Formant = 1.20f,        // Intent: shrunken, hollow calcium skull space resonance
            DryCrackle = 0.90f,     // Intent: core bone-on-bone friction and snapping ligaments
            FleshCrackle = 0.70f,   // Intent: rapid wet transients of leftover twitching tissue
            FormantDrift = 0.45f,   // Intent: unstable bone alignment shifting timbre dynamically
            Distortion = 0.20f,     // Intent: sharp, jagged tearing edges
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
            Pitch = 1.22f,          // Intent: pitch drops as bone mass is forced into damp skin
            Formant = 1.05f,        // Intent: throat cavity constricts under foreign tissue load
            FleshCrackle = 0.95f,   // AAA FIX: Maximum wet tissue crackle — shifting meat and stretching hide
            DryCrackle = 0.50f,     // Intent: stifled joint snapping beneath layers of skin
            WetOrganic = 0.85f,     // Intent: heavy blood and fluid friction noise floor
            FormantDrift = 0.55f,   // Intent: volatile, morphing vocal identity during transit
            Distortion = 0.35f,
            LowPass = 2200f,        // Intent: acoustic dampening through unaligned fat layers
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
            Pitch = 1.01f,          // Intent: nearly indistinguishable from actual human vocal chord vibration
            Formant = 1.00f,        // Intent: natural human vocal tract spacing
            Distortion = 0.00f,
            LowPass = 0f,
            HighPass = 0f,
            FormantDrift = 0.08f,   // AAA FIX: Micro LFO drifting — subtly wrong speech resonance that triggers suspicion
            FleshCrackle = 0.05f,   // AAA FIX: Micro-granular layer — tiny, wet twitching sound audible only during silence
            DryCrackle = 0.02f,     // Intent: rare, deep skeletal micro-snap when speaking rapidly
            WetOrganic = 0.08f      // Intent: a slightly too wet, slimy throat articulation
        };

        // =================================================================
        // REVEALING — Ripping the disguise apart: tearing skin, exploding bone strain
        // =================================================================
        public static readonly ScpVoicePreset RevealingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.55f,
            Pitch = 1.55f,          // Intent: sharp frequency spike as structural tension is released
            Formant = 1.15f,
            FleshCrackle = 1.10f,   // AAA FIX: Extreme granular wet bursts (skin tearing open, sliding off bone)
            DryCrackle = 0.85f,     // Intent: violent release of bound joints snapping outwards
            WetOrganic = 0.90f,     // Intent: high blood-gush and tissue exposure friction
            FormantDrift = 0.60f,   // Intent: violent spectral collapse of the voice model
            Distortion = 0.60f,     // Intent: harsh, aggressive tearing edge amplification
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
            Pitch = 1.70f,          // Intent: absolute muscular/skeletal mechanical high-tension frequency
            Formant = 1.25f,        // Intent: tight, hyper-constricted dry skull chamber resonance
            DryCrackle = 1.15f,     // AAA FIX: Intense, brittle bone snapping and socket friction under load
            FleshCrackle = 1.00f,   // Intent: crushing wet tissue beneath bare phalanges
            Guttural = 0.65f,       // AAA FIX: Severe, rasping ventricular fold strain (lethal exertion grunt)
            Distortion = 1.10f,     // Intent: massive, aggressive waveshaping saturation to punch through VoIP mix
            FormantDrift = 0.50f,
            WetOrganic = 0.75f,
            HighPass = 300f,        // Intent: eliminate mic proximity rumble to emphasize sharp bone snaps
            WetDecay = 0.50f
        };

        public static ScpVoicePreset GetPresetForState(Scp3114VoiceState state)
        {
            switch (state)
            {
                case Scp3114VoiceState.Undisguised:
                    return UndisguisedPreset;

                case Scp3114VoiceState.Disguising:
                    return DisguisingPreset;

                case Scp3114VoiceState.Disguised:
                    return DisguisedPreset;

                case Scp3114VoiceState.Revealing:
                    return RevealingPreset;

                case Scp3114VoiceState.Strangling:
                    return StranglingPreset;

                default:
                    return UndisguisedPreset;
            }
        }
    }
}