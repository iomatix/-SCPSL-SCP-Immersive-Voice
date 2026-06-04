namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Dynamic emotional states for SCP‑939, tuned for whisper mimicry, predatory intent
    /// and organic wetness. Designed to preserve intelligibility while remaining uncanny.
    /// </summary>
    public static class Scp939DynamicPresets
    {
        // Signature whisper: soft, breathy, unsettling presence
        public static readonly ScpVoicePreset IdleWhisperPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 1.85f,
            Pitch = 0.58f,
            Formant = 0.70f,
            LowPass = 2400f,
            HighPass = 260f,
            WhisperAmount = 0.92f,
            BreathNoise = 0.70f,
            WetOrganic = 0.42f,
            FormantDrift = 0.22f
        };

        // Human mimicry: almost normal, but subtly wrong
        public static readonly ScpVoicePreset MimickingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 1.95f,
            Pitch = 0.98f,
            Formant = 1.00f,
            LowPass = 4800f,
            HighPass = 0f,
            WhisperAmount = 0.12f,
            BreathNoise = 0.10f,
            WetOrganic = 0.06f,
            FormantDrift = 0.04f,
            Reverb = 0.03f
        };

        // Predator focus: cold, controlled, sharpened whisper
        public static readonly ScpVoicePreset FocusedPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.05f,
            Pitch = 0.62f,
            Formant = 0.74f,
            HighPass = 1050f,
            LowPass = 2600f,
            WhisperAmount = 0.88f,
            BreathNoise = 0.72f,
            WetOrganic = 0.52f,
            Distortion = 0.10f,
            FormantDrift = 0.28f
        };

        // Lunge attack: violent whisper‑scream, tearing breath
        public static readonly ScpVoicePreset AttackingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.25f,
            Pitch = 0.72f,
            Formant = 0.68f,
            HighPass = 1500f,
            WhisperAmount = 1.00f,
            BreathNoise = 1.20f,
            WetOrganic = 0.72f,
            WetDecay = 0.42f,
            Distortion = 1.20f,
            Guttural = 0.22f,
            FormantDrift = 0.40f
        };

        // Amnestic fog: distant, dreamlike, smeared presence
        public static readonly ScpVoicePreset AmnesticCloudPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 1.87f,
            Pitch = 0.90f,
            Formant = 0.95f,
            LowPass = 1500f,
            HighPass = 0f,
            Reverb = 0.38f,
            WhisperAmount = 0.52f,
            BreathNoise = 0.28f,
            WetOrganic = 0.32f,
            FormantDrift = 0.18f,
            Distortion = 0.06f
        };

        public static ScpVoicePreset GetPresetForState(Scp939VoiceState state)
        {
            switch (state)
            {
                case Scp939VoiceState.IdleWhisper:
                    return IdleWhisperPreset;

                case Scp939VoiceState.Mimicking:
                    return MimickingPreset;

                case Scp939VoiceState.Focused:
                    return FocusedPreset;

                case Scp939VoiceState.Attacking:
                    return AttackingPreset;

                case Scp939VoiceState.AmnesticCloud:
                    return AmnesticCloudPreset;

                default:
                    return IdleWhisperPreset;
            }
        }
    }
}
