namespace SCP_Immersive_Voice.Presets
{
    using PlayerRoles;
    using System.Collections.Generic;

    /// <summary>
    ///  Default SCP voice presets designed for natural, cinematic and character‑accurate timbre.
    /// Perfectly calibrated for the reconstructed high-performance stateful DSP pipeline matrix.
    /// </summary>
    public static class ScpVoiceDefaultPresets
    {
        public static Dictionary<RoleTypeId, ScpVoicePreset> Create()
        {
            return new Dictionary<RoleTypeId, ScpVoicePreset>()
            {
                // =================================================================
                // SCP‑049 (The Plague Doctor) — Masked, calm, hollow, antique resonance
                // =================================================================
                [RoleTypeId.Scp049] = new ScpVoicePreset
                {
                    Enable = true,
                    Mode = ScpVoicePresetMode.Default,
                    OutputGain = 2.80f,      // High output gain to compensate for heavy low-pass absorption
                    Pitch = 0.82f,           // Intent: deeper, aristocratic, highly controlled pitch
                    Formant = 0.85f,         // Intent: elongated throat cavity mimicking the ceramic beak shape
                    LowPass = 2200f,         // Intent: heavy leather mask + fabric hood boundary absorption muffling
                    Distortion = 0.18f,      // TWEAK: Antique triode saturation mimicking a 19th-century phonograph cylinder
                    Guttural = 0.12f,        // TWEAK: Subdued laryngeal dry grit representing mature paper-like vocal cords
                    Reverb = 0.22f,          // TWEAK: Tightened chamber decay to simulate mask interior reflections and boost clarity
                    WhisperAmount = 0.10f,   // Intent: dry air friction flowing beneath the heavy hood cloth layers
                    BreathNoise = 0.15f      // Intent: voice-reactive leather mask exhaust valve ventilation
                },

                // =================================================================
                // SCP‑096 (The Shy Guy) — Fragile, trembling, sobbing hyperventilation
                // =================================================================
                [RoleTypeId.Scp096] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.4f,      // Intent: high gain boost to guarantee readability over textures
                    Pitch = 1.06f,          // Intent: tense, shrunken, high-panic vocal chords
                    Formant = 0.88f,        // Intent: uncanny biological mismatch (high pitch + wide hollow throat)
                    FormantDrift = 0.70f,   // Intent: THE CORE CRACKLE: Aggressive LFO modulation creating a weeping, trembling voice
                    BreathNoise = 0.75f,    // Intent: High-intensity air rush emulating severe sobbing hyperventilation
                    WhisperAmount = 0.35f,  // Intent: De-voicing layer to split harmonic chords, making the voice sound broken and crying
                    Distortion = 0.22f,     // Intent: Severe emotional strain / tearing of the vocal tract
                    WetOrganic = 0.38f,     // Intent: High lacrimal fluid/saliva saturation from constant weeping
                    LowPass = 4200f         // Intent: Soften high edges to retain a fragile, miserable character
                },

                // =================================================================
                // SCP‑939 (The Predatory Canine) — Unvoiced mimicry, trochanter depth
                // =================================================================
                [RoleTypeId.Scp939] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 1.85f,
                    Pitch = 0.55f,          // Intent: deep, predatory harmonic base
                    Formant = 0.65f,        // Intent: heavily expanded reptilian vocal tract
                    WhisperAmount = 0.85f,  // Intent: signature unvoiced camouflage layer (human copycat)
                    BreathNoise = 0.50f,    // Intent: constant hissing, cold animalistic exhalation
                    WetOrganic = 0.45f,     // Intent: dense mucous and saliva coating inside the jaw
                    Distortion = 0.08f      // Intent: minimal laryngeal gravel
                },

                // =================================================================
                // SCP‑173 (The Sculpture) — Lithospheric monolithic block, crushing mass
                // =================================================================
                [RoleTypeId.Scp173] = new ScpVoicePreset
                {
                    Enable = true,
                    Mode = ScpVoicePresetMode.Default,
                    OutputGain = 2.60f,      // Boosted to compensate for heavy physical dampening
                    Pitch = 0.85f,           // Intent: heavy, structural mass slowdown
                    Formant = 0.50f,         //  FIX: Extreme throat cavity collapse to destroy human vowels
                    StoneCrack = 1.35f,      // Intent: brutal, explosive discrete lattice structural ruptures
                    StoneGrind = 1.20f,      // Intent: tectonic stick-slip friction scraping layer
                    Distortion = 0.65f,      // Intent: physical material hardness clipping
                    LowPass = 1100f,         //  FIX: Drastic acoustic muffling inside thick solid cured concrete
                    HighPass = 80f           // Remove absolute sub-bass mud
                },

                // =================================================================
                // SCP‑106 (The Old Man) — Corroded, decayed, pocket-dimension leakage
                // =================================================================
                [RoleTypeId.Scp106] = new ScpVoicePreset
                {
                    Enable = true,
                    IsGlobalTransmission = true,
                    OutputGain = 2.45f,
                    Pitch = 0.52f,          // Intent: ancient, sub-octave dimensional abyss depth
                    Formant = 0.48f,        // Intent: fully collapsed, rotted vocal cavity architecture
                    DemonicOctaverMix = 0.25f,     // Sub-bass expander anchoring his physical weight in the sub-frequency floor
                    Distortion = 0.65f,     // Intent: severe acidic corrosion texture destroying wave boundaries
                    LowPass = 850f,         // Intent: extreme, suffocating damp mud and subterranean dirt muffling
                    Reverb = 0.40f,         // Intent: baseline environment containment leakage
                    WetDecay = 0.95f,       // Intent: visceral viscous absorption (flooded slime walls)
                    WetOrganic = 0.35f,     // Intent: slimy throat decomposition mechanics
                    PocketEcho = 0.85f      // Intent: maximum non-Euclidean phase-inversion echo matrix
                },

                // =================================================================
                // SCP‑3114 (The Skeleton) — Twitching bones, high-tension torn tissue
                // =================================================================
                [RoleTypeId.Scp3114] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.45f,
                    Pitch = 1.45f,          // Intent: frantic, high-velocity nerve tension frequency
                    Formant = 1.20f,        // Intent: shrunken, hollow calcium skull space resonance
                    DryCrackle = 0.90f,     // Intent: dry bone-on-bone friction and snapping skeletal ligaments
                    FleshCrackle = 0.70f,   // Intent: rapid wet transients of leftover twitching organic tissue
                    FormantDrift = 0.45f,   // Intent: unstable bone alignment shifting timbre dynamically
                    Distortion = 0.20f      // Intent: sharp, jagged tearing edges
                },

                // =================================================================
                // SCP‑079 (The Old AI) — Sterile Machine Mainframe Core
                // =================================================================
                [RoleTypeId.Scp079] = new ScpVoicePreset
                {
                    Enable = true,
                    Mode = ScpVoicePresetMode.Default,
                    OutputGain = 2.50f,
                    Pitch = 0.95f,           // Keep fundamental clear for AI bot voice texturing
                    Formant = 1.05f,
                    DemonicOctaverMix = 0.17f,      // Generates a pixelated dual-voice supercomputer entity layer
                    SiliconModulation = 0.75f, // Cold, inhuman metallic intermodulation synthesis
                    Bitcrush = 0.60f,        // Increased to violently fracture the sub-octave into binary steps
                    SampleRateReduce = 0.55f, // Heavy digital aliasing artifacts
                    Glitch = 0.40f,
                    StaticNoise = 0.045f,     // A little of analog fuzz
                    Distortion = 0.15f,
                    Subharmonic = 0.20f,     // Sub-bass room hum
                    DataBurst = 0.70f,       // High-frequency binary tracks
                    HighPass = 70f,          // Unblock the new deep octaver floor
                    LowPass = 7500f
                },

                // =================================================================
                // SCP‑049‑2 (The Cured) — Visceral Necrotic Gasping Corpse
                // =================================================================
                [RoleTypeId.Scp0492] = new ScpVoicePreset
                {
                    Enable = true,
                    Mode = ScpVoicePresetMode.Default,
                    OutputGain = 2.45f,
                    Pitch = 0.76f,           // TUNING: Heavier biological drag (more sluggish)
                    Formant = 0.50f,         // TUNING: Fully expanded rotted trachea
                    DeathRattle = 0.85f,     // R&D INJECTION: Dominant visceral choking gurgle
                    DemonicOctaverMix = 0.20f, // BALANCE: Dropped to a subtle low-end shadow (no longer occult lord)
                    Guttural = 0.80f,        // Massive false-vocal cord larynx sandpaper abrasion
                    FleshCrackle = 0.65f,    // Wet snapping bone and stretching tissue transients
                    Distortion = 0.35f,
                    LowPass = 1500f,         // Muffled high-frequencies matching heavy physical decay
                    HighPass = 90f
                },

                // =================================================================
                // Flamingos (The Anomalous Avian Horde) — High-pitched syrinx matrix
                // =================================================================
                [RoleTypeId.Flamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.22f,
                    Pitch = 1.55f,          // Intent: hyper-tension avian pitch signature
                    Formant = 1.20f,        // Intent: tiny bird beak cavity resonance
                    Chirp = 0.50f,          // Intent: stochastic avian syrinx FM frequency micro-sweeps
                    Distortion = 0.12f      // Intent: light beak crunch
                },
                [RoleTypeId.AlphaFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.22f,
                    Pitch = 1.42f,          // Intent: slightly larger alpha variant frame
                    Formant = 1.12f,
                    Chirp = 0.45f,
                    Distortion = 0.20f
                },
                [RoleTypeId.ZombieFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.22f,
                    Pitch = 1.15f,          // Intent: rotted, dropped avian speed
                    Formant = 0.90f,
                    Chirp = 0.30f,
                    Subharmonic = 0.40f,    // Intent: undead bird gut rattle
                    Distortion = 0.45f
                },
                [RoleTypeId.NtfFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.22f,
                    Pitch = 1.35f,
                    Formant = 1.10f,
                    Chirp = 0.38f,
                    Distortion = 0.22f
                },
                [RoleTypeId.ChaosFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.25f,
                    Pitch = 1.25f,
                    Formant = 1.05f,
                    Chirp = 0.35f,
                    Distortion = 0.30f
                },
            };
        }
    }
}