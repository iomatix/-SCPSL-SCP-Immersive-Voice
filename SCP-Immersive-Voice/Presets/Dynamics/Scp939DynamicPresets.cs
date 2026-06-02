namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    public static class Scp939DynamicPresets
    {
        // 1. Default idle whisper
        public static readonly ScpVoicePreset IdleWhisperPreset = new ScpVoicePreset()
        {
            Pitch = 0.5f,
            Formant = 0.6f,
            LowPass = 2500f,
            HighPass = 300f,
            Distortion = 0f,
            Reverb = 0f,
            BreathNoise = 0.6f,
            WhisperAmount = 0.8f

        };

        // 2. Mimicking human voice
        public static readonly ScpVoicePreset MimickingPreset = new ScpVoicePreset()
        {
            Pitch = 0.985f,
            Formant = 1.0f,
            LowPass = 5000f,
            HighPass = 0f,
            Distortion = 0f,
            Reverb = 0.033f,
            BreathNoise = 0.065f,
            WhisperAmount = 0.085f
        };

        // 3. Focused hunting mode
        public static readonly ScpVoicePreset FocusedPreset = new ScpVoicePreset()
        {
            Pitch = 0.6f,
            Formant = 0.7f,
            HighPass = 1200f,
            LowPass = 2500f,
            Distortion = 0.1f,
            BreathNoise = 0.6f,
            WhisperAmount = 0.8f

        };

        // 4. Attacking / lunging
        public static readonly ScpVoicePreset AttackingPreset = new ScpVoicePreset()
        {
            Pitch = 0.7f,
            Formant = 0.7f,
            HighPass = 1500f,
            Distortion = 1.2f,
            BreathNoise = 1.4f,
            WhisperAmount = 1.0f
        };

        // 5. Amnestic cloud
        public static readonly ScpVoicePreset AmnesticCloudPreset = new ScpVoicePreset()
        {
            LowPass = 1500f,
            HighPass = 0f,
            Reverb = 0.35f,
            Distortion = 0.1f,
            BreathNoise = 0.33f,
            WhisperAmount = 0.44f
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
