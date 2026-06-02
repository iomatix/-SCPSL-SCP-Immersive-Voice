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
                // Calm, deep, muffled voice behind a mask. Slightly eerie but controlled.
                [RoleTypeId.Scp049] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.72f,
                    Formant = 0.85f,
                    Distortion = 0.15f,
                    LowPass = 3200f,
                    Reverb = 0.25f
                },

                // SCP-096 — "Shy Guy"
                // Quiet trembling voice normally, but extremely distorted and high-pitched during rage.
                [RoleTypeId.Scp096] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.0f,
                    Formant = 1.0f,
                    Distortion = 0.2f,
                    LowPass = 3500f,
                },

                // SCP-939 — "With Many Voices"
                // Whisper mimicry creature. Soft, low-pitch, breathy voice.
                [RoleTypeId.Scp939] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.5f,
                    Formant = 0.6f,
                    Distortion = 0f
                },

                // SCP-173 — "The Sculpture"
                // Does not speak. If forced, should sound like stone grinding or cracking.
                [RoleTypeId.Scp173] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1f,
                    Formant = 1f,
                    Distortion = 3.0f,
                    StoneCrack = 1.0f,
                    StoneGrind = 0.8f
                },

                // SCP-106 — "Old Man"
                // Deep, wet, decayed voice. Echoing from another dimension.
                [RoleTypeId.Scp106] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.55f,
                    Formant = 0.45f,
                    Distortion = 1.2f,
                    LowPass = 900f,
                    Reverb = 0.45f,
                    WetDecay = 0.9f,
                    PocketEcho = 0.7f
                },

                // SCP-3114 — "The Flesh That Hates"
                // High-pitched, organic, unstable voice.
                [RoleTypeId.Scp3114] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.8f,
                    Formant = 1f,
                    Distortion = 0f
                },

                // SCP-079 — "Old AI / Computer"
                // Robotic, glitchy, bitcrushed voice. Not proximity-based.
                [RoleTypeId.Scp079] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1f,
                    Formant = 1f,
                    Distortion = 0.25f,
                    Bitcrush = 0.65f,
                    SampleRateReduce = 0.55f,
                    Glitch = 0.35f,
                    StaticNoise = 0.25f
                },

                // SCP-049-2 — "Zombie"
                // Growly and guttural, but not too distorted. Should still be intelligible, just very rough.
                [RoleTypeId.Scp0492] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 0.75f,
                    Formant = 0.8f,
                    Distortion = 0.3f,
                    LowPass = 1800f,
                    HighPass = 150f,
                    Guttural = 0.9f,
                    DryCrackle = 0.6f,
                    Subharmonic = 0.7f
                },

                // Flamingo variants — comedic SCP, but lets give them a slightly weird pitch
                [RoleTypeId.Flamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.4f,
                    Formant = 1f,
                    Distortion = 1.1f
                },
                [RoleTypeId.AlphaFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.4f,
                    Formant = 1f,
                    Distortion = 1.1f
                },
                [RoleTypeId.ZombieFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.4f,
                    Formant = 1f,
                    Distortion = 1.1f
                },
                [RoleTypeId.NtfFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.4f,
                    Formant = 1f,
                    Distortion = 1.1f
                },
                [RoleTypeId.ChaosFlamingo] = new ScpVoicePreset
                {
                    Enable = true,
                    Pitch = 1.4f,
                    Formant = 1f,
                    Distortion = 1.1f
                },
            };
        }
    }

}
