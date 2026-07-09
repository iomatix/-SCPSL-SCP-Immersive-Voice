namespace SCP_Immersive_Voice.Presets
{
    using PlayerRoles;
    using System.Collections.Generic;

    /// <summary>
    /// Default SCP voice presets designed for natural, cinematic and character‑accurate timbre.
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
                    OutputGain = 3.20f,       // TUNING: Boosted from 2.80f to beautifully penetrate through the heavy leather low-pass absorption mask
                    Pitch = 0.82f,
                    Formant = 0.85f,
                    LowPass = 2200f,
                    Distortion = 0.15f,       // TUNING: Mildly smoothed out for clean antique analog tube warmth
                    Guttural = 0.12f,
                    Reverb = 0.22f,
                    WhisperAmount = 0.10f,
                    BreathNoise = 0.15f
                },

                // =================================================================
                // SCP‑096 (The Shy Guy) — Fragile, trembling, sobbing hyperventilation
                // =================================================================
                [RoleTypeId.Scp096] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.45f,       // Aligned with the dynamic fallback configurations
                    Pitch = 1.15f,
                    Formant = 0.85f,
                    LowPass = 3800f,
                    Tremolo = 0.65f,
                    FormantDrift = 0.60f,
                    BreathNoise = 0.80f,
                    WhisperAmount = 0.25f,
                    Distortion = 0.12f,
                    WetOrganic = 0.45f
                },

                // =================================================================
                // SCP‑939 (The Predatory Canine) — Unvoiced mimicry, trochanter depth
                // =================================================================
                [RoleTypeId.Scp939] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 3.20f,           // Aligned with the dynamic baseline configuration
                    Pitch = 0.55f,
                    Formant = 0.65f,
                    LowPass = 2400f,
                    HighPass = 260f,
                    PredatoryCamouflage = 0.65f,
                    BreathNoise = 0.25f,
                    WetOrganic = 0.45f,
                    FormantDrift = 0.22f,
                    Distortion = 0.08f
                },

                // =================================================================
                // SCP‑173 (The Sculpture) — Lithospheric monolithic block, crushing mass
                // =================================================================
                [RoleTypeId.Scp173] = new ScpVoicePreset
                {
                    Enable = true,
                    Mode = ScpVoicePresetMode.Default,
                    OutputGain = 3.55f,       // TUNING: Heavily boosted from 2.60f to restore absolute volume lost to the thick 1100Hz solid concrete barrier absorption
                    Pitch = 0.85f,
                    Formant = 0.50f,
                    StoneCrack = 0.52f,       // TUNING: Reduced from 1.35f. Separates crushing transients so they snap clearly instead of blurring into static hum
                    StoneGrind = 0.40f,       // TUNING: Toned down from 1.20f for crisp friction texture boundaries
                    Distortion = 0.22f,       // TUNING: Reduced from 0.65f to prevent the hard brickwall limiter from turning his voice into square-wave noise
                    LowPass = 1100f,
                    HighPass = 80f
                },

                // =================================================================
                // SCP‑106 (The Old Man) — Corroded, decayed, pocket-dimension leakage
                // =================================================================
                [RoleTypeId.Scp106] = new ScpVoicePreset
                {
                    Enable = true,
                    IsGlobalTransmission = false,
                    OutputGain = 2.65f,       // Aligned with the dynamic proximity configuration
                    Pitch = 0.52f,
                    Formant = 0.46f,
                    DemonicOctaverMix = 0.25f,
                    Distortion = 0.45f,
                    LowPass = 1150f,
                    Reverb = 0.40f,
                    WetDecay = 0.90f,
                    WetOrganic = 0.35f,
                    PocketEcho = 0.80f,
                    FormantDrift = 0.30f
                },

                // =================================================================
                // SCP‑3114 (The Skeleton) — Twitching bones, high-tension torn tissue
                // =================================================================
                [RoleTypeId.Scp3114] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.45f,
                    Pitch = 1.28f,            // Aligned with the optimized dynamic skeleton configuration
                    Formant = 1.15f,
                    DryCrackle = 0.42f,
                    FleshCrackle = 0.35f,
                    FormantDrift = 0.30f,
                    Distortion = 0.12f
                },

                // =================================================================
                // SCP‑079 (The Old AI) — Sterile Machine Mainframe Core
                // =================================================================
                [RoleTypeId.Scp079] = new ScpVoicePreset
                {
                    Enable = true,
                    Mode = ScpVoicePresetMode.Default,
                    OutputGain = 2.50f,
                    Pitch = 0.95f,
                    Formant = 1.05f,
                    DemonicOctaverMix = 0.17f,
                    SiliconModulation = 0.75f, // Digital intermodulation ring modulation artifacts are intended here
                    Bitcrush = 0.60f,
                    SampleRateReduce = 0.55f,
                    Glitch = 0.40f,
                    StaticNoise = 0.045f,
                    Distortion = 0.15f,
                    Subharmonic = 0.20f,
                    DataBurst = 0.70f,
                    HighPass = 70f,
                    LowPass = 7500f
                },

                // =================================================================
                // SCP‑049‑2 (The Cured) — Visceral Necrotic Gasping Corpse
                // =================================================================
                [RoleTypeId.Scp0492] = new ScpVoicePreset
                {
                    Enable = true,
                    Mode = ScpVoicePresetMode.Default,
                    OutputGain = 3.10f,           // Optimized zombie gurgle configuration
                    Pitch = 0.65f,
                    Formant = 0.58f,
                    DeathRattle = 0.75f,
                    DemonicOctaverMix = 0.00f,
                    Guttural = 0.25f,
                    FleshCrackle = 0.20f,
                    WetOrganic = 0.65f,
                    WetDecay = 0.40f,
                    Distortion = 0.15f,
                    LowPass = 1300f,
                    HighPass = 80f
                },

                // =================================================================
                // Flamingos (The Anomalous Avian Horde) — High-pitched syrinx matrix
                // =================================================================
                [RoleTypeId.Flamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.95f,           // TUNING: Boosted from 2.22f to compensate for severe energy loss in high-pitched transpositions
                    Pitch = 1.55f,
                    Formant = 1.20f,
                    Chirp = 0.50f,
                    Distortion = 0.12f
                },
                [RoleTypeId.AlphaFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.95f,           // TUNING: Restored volume presence
                    Pitch = 1.42f,
                    Formant = 1.10f,
                    Chirp = 0.45f,
                    Distortion = 0.15f
                },
                [RoleTypeId.ZombieFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 3.10f,           // TUNING: Boosted further to ensure rotten avian gurgles are readable
                    Pitch = 1.15f,
                    Formant = 0.90f,
                    Chirp = 0.30f,
                    Subharmonic = 0.40f,
                    Distortion = 0.25f
                },
                [RoleTypeId.NtfFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.95f,
                    Pitch = 1.35f,
                    Formant = 1.10f,
                    Chirp = 0.38f,
                    Distortion = 0.15f
                },
                [RoleTypeId.ChaosFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    OutputGain = 2.95f,
                    Pitch = 1.25f,
                    Formant = 1.05f,
                    Chirp = 0.35f,
                    Distortion = 0.18f
                },
            };
        }
    }
}