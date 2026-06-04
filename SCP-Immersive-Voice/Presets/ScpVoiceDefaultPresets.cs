namespace SCP_Immersive_Voice.Presets
{
    using PlayerRoles;
    using System.Collections.Generic;

    /// <summary>
    /// AAA Default SCP voice presets designed for natural, cinematic and character‑accurate timbre.
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
                    OutputGain = 2.8f,
                    Pitch = 0.82f,          // Intent: deeper, aristocratic, controlled pitch
                    Formant = 0.85f,        // Intent: elongated throat cavity mimicking the ceramic beak
                    LowPass = 2200f,        // Intent: acoustic cloth + mask boundary muffling absorption
                    Distortion = 0.14f,     // Intent: subtle structural rasp in the vocal cords
                    Reverb = 0.35f,         // Intent: isolated, hollow cathedral room presence
                    WhisperAmount = 0.12f,  // Intent: faint sound of air flowing beneath the heavy hood
                    BreathNoise = 0.15f     // Intent: audible, calculated mask ventilation
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
                // SCP‑173 (The Sculpture) — Litospheric, zero humanity, crushing mass
                // =================================================================
                [RoleTypeId.Scp173] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 1.15f,
                    Pitch = 1.00f,          // Intent: neutral baseline (mineral has no human pitch)
                    Formant = 0.82f,        // Intent: rigid, heavy concrete block internal resonance
                    StoneCrack = 0.95f,     // Intent: brittle, high-energy discrete Dirac crystal snaps
                    StoneGrind = 1.15f,     // Intent: deep sub-harmonic mass rumble and tectonic block friction
                    Distortion = 0.45f,     // Intent: extreme material hardness waveshaping clipping
                    LowPass = 2000f         // Intent: heavy acoustic dampening through solid concrete layers
                },

                // =================================================================
                // SCP‑106 (The Old Man) — Corroded, decayed, pocket-dimension leakage
                // =================================================================
                [RoleTypeId.Scp106] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.25f,
                    Pitch = 0.52f,          // Intent: ancient, sub-octave dimensional abyss depth
                    Formant = 0.48f,        // Intent: fully collapsed, rotted vocal cavity architecture
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
                // SCP‑079 (The Old AI) — Corrupted low-bandwidth mainframe hardware
                // =================================================================
                [RoleTypeId.Scp079] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 1.15f,
                    Pitch = 1.00f,          // Intent: sterile synthetic pitch normalization
                    Formant = 1.00f,        // Intent: bypass any organic larynx shaping
                    Bitcrush = 0.75f,       // Intent: dynamic quantization step reduction (true 4-bit chip crunch)
                    SampleRateReduce = 0.65f,// Intent: severe clock-divider aliasing artifact generation (~1.2kHz)
                    Glitch = 0.45f,         // Intent: stochastic buffer frame looping and transmission dropouts
                    StaticNoise = 0.40f,    // Intent: heavy background RF analog interference
                    Distortion = 0.25f,     // Intent: transistor circuitry overload warmth
                    HighPass = 400f         // Intent: discard human lower-frequency chest coefficients completely
                },

                // =================================================================
                // SCP‑049‑2 (The Genzombie) — Guttural, sub-harmonic undead rattle
                // =================================================================
                [RoleTypeId.Scp0492] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 1.45f,
                    Pitch = 0.62f,          // Intent: rotting throat depth
                    Formant = 0.75f,        // Intent: deadened, loose tissue volume
                    Guttural = 0.95f,       // Intent: aggressive, rasping ventricular fold false-cord vibration
                    Subharmonic = 0.85f,    // Intent: phase-locked chest growl frequency divider (demonic tone)
                    DryCrackle = 0.55f,     // Intent: rigor mortis joint cracking and bone friction
                    WetDecay = 0.40f,       // Intent: wet decomposition echo trail
                    LowPass = 1400f,        // Intent: deadened, muddy frequency absorption boundaries
                    HighPass = 120f         // Intent: clean out sub-bass microphone proximity mud
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