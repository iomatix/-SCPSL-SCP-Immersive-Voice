namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    public static class Scp939DynamicPresets
    {
        // 1. Idle Whisper — signature SCP-939 sound
        // Soft, breathy, wet, unsettling. Almost ASMR but wrong.
        public static readonly ScpVoicePreset IdleWhisperPreset = new ScpVoicePreset
        {
            Pitch = 0.52f,
            Formant = 0.62f,
            LowPass = 2400f,
            HighPass = 280f,
            WhisperAmount = 0.90f,
            BreathNoise = 0.75f,
            WetOrganic = 0.45f,
            FormantDrift = 0.25f
        };

        // 2. Mimicking — pretending to be human
        // Must sound almost normal, but with subtle uncanny artifacts.
        public static readonly ScpVoicePreset MimickingPreset = new ScpVoicePreset
        {
            Pitch = 0.98f,
            Formant = 1.00f,
            LowPass = 4800f,
            HighPass = 0f,
            WhisperAmount = 0.10f,
            BreathNoise = 0.08f,
            WetOrganic = 0.05f,
            FormantDrift = 0.05f,
            Reverb = 0.04f
        };

        // 3. Focused Hunting — predator mode
        // Whisper becomes sharp, cold, predatory.
        public static readonly ScpVoicePreset FocusedPreset = new ScpVoicePreset
        {
            Pitch = 0.60f,
            Formant = 0.72f,
            HighPass = 1100f,
            LowPass = 2600f,
            WhisperAmount = 0.85f,
            BreathNoise = 0.70f,
            WetOrganic = 0.55f,
            Distortion = 0.12f,
            FormantDrift = 0.30f
        };

        // 4. Attacking / Lunging — full aggression
        // Wet, tearing, breathy, violent whisper-scream.
        public static readonly ScpVoicePreset AttackingPreset = new ScpVoicePreset
        {
            Pitch = 0.72f,
            Formant = 0.70f,
            HighPass = 1500f,
            WhisperAmount = 1.00f,
            BreathNoise = 1.25f,
            WetOrganic = 0.75f,
            WetDecay = 0.45f,
            Distortion = 1.35f,
            Guttural = 0.25f,
            FormantDrift = 0.45f
        };

        // 5. Amnestic Cloud — foggy, dreamlike, distant
        // Voice becomes smeared, soft, unreal.
        public static readonly ScpVoicePreset AmnesticCloudPreset = new ScpVoicePreset
        {
            Pitch = 0.90f,
            Formant = 0.95f,
            LowPass = 1500f,
            HighPass = 0f,
            Reverb = 0.40f,
            WhisperAmount = 0.55f,
            BreathNoise = 0.30f,
            WetOrganic = 0.35f,
            FormantDrift = 0.20f,
            Distortion = 0.08f
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