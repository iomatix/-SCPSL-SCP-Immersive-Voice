namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    public static class Scp3114DynamicPresets
    {
        // Real shape - high, organic, unstable
        public static readonly ScpVoicePreset UndisguisedPreset = new ScpVoicePreset
        {
            Pitch = 1.8f,
            Formant = 1.2f,
            Distortion = 0.4f,
            HighPass = 250f,
            Reverb = 0.12f,
            FormantDrift = 0.35f,
            FleshCrackle = 0.45f,
            WetOrganic = 0.45f,
        };

        // Disgusing – "meaty" transition
        public static readonly ScpVoicePreset DisguisingPreset = new ScpVoicePreset
        {
            Pitch = 1.4f,
            Formant = 0.9f,
            Distortion = 0.6f,
            LowPass = 2600f,
            Reverb = 0.2f,
            FormantDrift = 0.25f,
            FleshCrackle = 0.6f,
            WetOrganic = 0.7f,
        };

        // Pretends to be human – must sound like a normal human
        public static readonly ScpVoicePreset DisguisedPreset = new ScpVoicePreset
        {
            Pitch = 1.01f,
            Formant = 0.99f,
            Distortion = 0f,
            LowPass = 0f,
            HighPass = 0f,
            Reverb = 0f,
            FormantDrift = 0.01f,
            FleshCrackle = 0.01f,
            WetOrganic = 0.02f,
        };

        // Reveling - the opposite of Disguising, high, organic, unstable, but more "revealing" and less "meaty" than Undisguised
        public static readonly ScpVoicePreset RevealingPreset = new ScpVoicePreset
        {
            Pitch = 1.6f,
            Formant = 1.0f,
            Distortion = 0.7f,
            LowPass = 2400f,
            Reverb = 0.22f,
            FormantDrift = 0.3f,
            FleshCrackle = 0.7f,
            WetOrganic = 0.65f,
        };

        // Strangling – high, aggressive, organic
        public static readonly ScpVoicePreset StranglingPreset = new ScpVoicePreset
        {
            Pitch = 2.0f,
            Formant = 1.3f,
            Distortion = 0.8f,
            HighPass = 400f,
            Reverb = 0.18f,
            FormantDrift = 0.5f,
            FleshCrackle = 1.0f,
            WetOrganic = 0.9f,
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