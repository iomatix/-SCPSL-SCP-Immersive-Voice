namespace SCP_Immersive_Voice.Presets
{
    using PlayerRoles;
    using System.Collections.Generic;

    public static class ScpVoiceDefaultPresets
    {
        public static Dictionary<RoleTypeId, ScpVoicePreset> Create()
        {
            return new Dictionary<RoleTypeId, ScpVoicePreset>()
            {
                // SCP-049 — "Plague Doctor"
                // Muffled mask, deep, calm, eerie authority.
                [RoleTypeId.Scp049] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.70f,
                    Formant = 0.82f,
                    LowPass = 2800f,
                    Distortion = 0.18f,
                    Reverb = 0.32f,
                    WhisperAmount = 0.15f
                },

                // SCP-096 — "Shy Guy"
                // Calm: trembling, unstable. Rage: handled dynamically.
                [RoleTypeId.Scp096] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.95f,
                    Formant = 1.0f,
                    Distortion = 0.25f,
                    FormantDrift = 0.35f,
                    LowPass = 3600f,
                    WetOrganic = 0.25f
                },

                // SCP-939 — "With Many Voices"
                // Whisper mimicry, breathy, soft, uncanny.
                [RoleTypeId.Scp939] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.55f,
                    Formant = 0.65f,
                    WhisperAmount = 0.85f,
                    BreathNoise = 0.45f,
                    WetOrganic = 0.35f,
                    Distortion = 0.05f
                },

                // SCP-173 — "The Sculpture"
                // Stone grinding, cracking, zero humanity.
                [RoleTypeId.Scp173] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.90f,
                    Formant = 0.95f,
                    StoneCrack = 1.0f,
                    StoneGrind = 1.0f,
                    Distortion = 0.4f,
                    LowPass = 2500f
                },

                // SCP-106 — "Old Man"
                // Wet, decayed, dimensional echo, deep and horrifying.
                [RoleTypeId.Scp106] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.52f,
                    Formant = 0.42f,
                    Distortion = 1.35f,
                    LowPass = 850f,
                    Reverb = 0.55f,
                    WetDecay = 1.0f,
                    WetOrganic = 0.25f,
                    PocketEcho = 0.85f
                },

                // SCP-3114 — "The Flesh That Hates"
                // High, unstable, organic, wet, twitchy.
                [RoleTypeId.Scp3114] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.65f,
                    Formant = 1.15f,
                    WetOrganic = 0.85f,
                    FleshCrackle = 0.55f,
                    Distortion = 0.15f,
                    FormantDrift = 0.45f
                },

                // SCP-079 — "Old AI"
                // Digital corruption, bitcrush, glitch, static.
                [RoleTypeId.Scp079] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.0f,
                    Formant = 1.0f,
                    Bitcrush = 0.75f,
                    SampleRateReduce = 0.65f,
                    Glitch = 0.45f,
                    StaticNoise = 0.35f,
                    Distortion = 0.25f,
                    HighPass = 300f
                },

                // SCP-049-2 — "Zombie"
                // Guttural, subharmonic, rough, decayed but intelligible.
                [RoleTypeId.Scp0492] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.68f,
                    Formant = 0.78f,
                    Distortion = 0.35f,
                    LowPass = 1600f,
                    HighPass = 120f,
                    Guttural = 0.95f,
                    Subharmonic = 0.85f,
                    DryCrackle = 0.55f,
                    WetDecay = 0.35f
                },

                // Flamingo variants — comedic, but still SCP-weird
                [RoleTypeId.Flamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.55f,
                    Formant = 1.25f,
                    Chirp = 0.45f,
                    Distortion = 0.15f
                },
                [RoleTypeId.AlphaFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.45f,
                    Formant = 1.20f,
                    Chirp = 0.40f,
                    Distortion = 0.20f
                },
                [RoleTypeId.ZombieFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.15f,
                    Formant = 0.95f,
                    Chirp = 0.25f,
                    Distortion = 0.45f,
                    Subharmonic = 0.35f
                },
                [RoleTypeId.NtfFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.35f,
                    Formant = 1.10f,
                    Chirp = 0.35f,
                    Distortion = 0.25f
                },
                [RoleTypeId.ChaosFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.25f,
                    Formant = 1.05f,
                    Chirp = 0.30f,
                    Distortion = 0.30f
                },
            };
        }
    }
}