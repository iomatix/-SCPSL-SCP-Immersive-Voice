namespace SCP_Immersive_Voice.Presets
{
    using PlayerRoles;
    using System.Collections.Generic;

    /// <summary>
    /// Default SCP voice presets designed for natural, cinematic and character‑accurate timbre.
    /// Tuned for the corrected DSP pipeline (Pitch, Formant, Filters, Organic layers).
    /// </summary>
    public static class ScpVoiceDefaultPresets
    {
        public static Dictionary<RoleTypeId, ScpVoicePreset> Create()
        {
            return new Dictionary<RoleTypeId, ScpVoicePreset>()
            {
                // SCP‑049 — masked, calm, authoritative, slightly hollow
                [RoleTypeId.Scp049] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.78f,          // intent: deeper, controlled tone
                    Formant = 0.88f,        // intent: masked resonance
                    LowPass = 2600f,        // intent: cloth + beak muffling
                    Distortion = 0.12f,     // intent: subtle rasp
                    Reverb = 0.28f,         // intent: cathedral presence
                    WhisperAmount = 0.10f   // intent: breath under the mask
                },

                // SCP‑096 — trembling, unstable, fragile (rage handled dynamically)
                [RoleTypeId.Scp096] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.92f,          // intent: fragile but human
                    Formant = 1.00f,        // intent: neutral timbre baseline
                    Distortion = 0.18f,     // intent: emotional strain
                    FormantDrift = 0.25f,   // intent: instability
                    LowPass = 3400f,        // intent: soft, trembling tone
                    WetOrganic = 0.18f      // intent: subtle throat wetness
                },

                // SCP‑939 — whisper mimicry, breathy, uncanny
                [RoleTypeId.Scp939] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.60f,          // intent: predatory depth
                    Formant = 0.72f,        // intent: widened throat cavity
                    WhisperAmount = 0.85f,  // intent: signature whisper
                    BreathNoise = 0.40f,    // intent: soft, animalistic breath
                    WetOrganic = 0.30f,     // intent: moist vocal tract
                    Distortion = 0.04f      // intent: minimal rasp
                },

                // SCP‑173 — stone, grinding, zero humanity
                [RoleTypeId.Scp173] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.95f,          // intent: neutral pitch (stone has no voice)
                    Formant = 0.90f,        // intent: heavy, material resonance
                    StoneCrack = 0.85f,     // intent: brittle texture
                    StoneGrind = 0.90f,     // intent: mass and friction
                    Distortion = 0.32f,     // intent: harsh edges
                    LowPass = 2400f         // intent: dense, heavy material
                },

                // SCP‑106 — decayed, dimensional, wet, horrifying
                [RoleTypeId.Scp106] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.58f,          // intent: ancient, decayed depth
                    Formant = 0.55f,        // intent: collapsed vocal cavity
                    Distortion = 0.55f,     // intent: corroded texture
                    LowPass = 900f,         // intent: damp, suffocating tone
                    Reverb = 0.48f,         // intent: dimensional echo
                    WetDecay = 0.85f,       // intent: dripping, rotten wetness
                    WetOrganic = 0.22f,     // intent: decayed throat
                    PocketEcho = 0.70f      // intent: pocket dimension resonance
                },

                // SCP‑3114 — flesh, twitchy, wet, unstable
                [RoleTypeId.Scp3114] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.45f,          // intent: frantic, high‑tension tone
                    Formant = 1.12f,        // intent: tightened, strained throat
                    WetOrganic = 0.75f,     // intent: wet, fleshy texture
                    FleshCrackle = 0.50f,   // intent: twitching tissue
                    Distortion = 0.12f,     // intent: subtle tearing
                    FormantDrift = 0.40f    // intent: unstable identity
                },

                // SCP‑079 — corrupted digital voice
                [RoleTypeId.Scp079] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.00f,          // intent: neutral synthetic pitch
                    Formant = 1.00f,        // intent: no organic shaping
                    Bitcrush = 0.70f,       // intent: digital degradation
                    SampleRateReduce = 0.60f,// intent: corrupted bandwidth
                    Glitch = 0.40f,         // intent: unstable processing
                    StaticNoise = 0.30f,    // intent: analog interference
                    Distortion = 0.20f,     // intent: circuitry strain
                    HighPass = 350f         // intent: remove low organic tones
                },

                // SCP‑049‑2 — guttural, subharmonic, decayed
                [RoleTypeId.Scp0492] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.65f,          // intent: guttural depth
                    Formant = 0.80f,        // intent: decayed throat
                    Distortion = 0.30f,     // intent: rough texture
                    LowPass = 1500f,        // intent: deadened tone
                    HighPass = 120f,        // intent: remove mud
                    Guttural = 0.90f,       // intent: throat resonance
                    Subharmonic = 0.80f,    // intent: undead growl
                    DryCrackle = 0.50f,     // intent: bone friction
                    WetDecay = 0.30f        // intent: moist decomposition
                },

                // Flamingo variants — comedic but SCP‑weird
                [RoleTypeId.Flamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.55f,          // intent: comedic brightness
                    Formant = 1.20f,        // intent: small vocal cavity
                    Chirp = 0.45f,          // intent: bird‑like tone
                    Distortion = 0.12f      // intent: light rasp
                },
                [RoleTypeId.AlphaFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.45f,
                    Formant = 1.15f,
                    Chirp = 0.40f,
                    Distortion = 0.18f
                },
                [RoleTypeId.ZombieFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.20f,
                    Formant = 0.95f,
                    Chirp = 0.25f,
                    Distortion = 0.40f,
                    Subharmonic = 0.30f
                },
                [RoleTypeId.NtfFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.35f,
                    Formant = 1.10f,
                    Chirp = 0.35f,
                    Distortion = 0.22f
                },
                [RoleTypeId.ChaosFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.25f,
                    Formant = 1.05f,
                    Chirp = 0.30f,
                    Distortion = 0.28f
                },
            };
        }
    }
}
