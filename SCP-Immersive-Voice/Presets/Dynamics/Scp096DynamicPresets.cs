namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    public static class Scp096DynamicPresets
    {
        public static ScpVoicePreset CalmPreset = new ScpVoicePreset()
        {
            Pitch = 1.0f,
            Distortion = 0.2f,
            LowPass = 3500f
        };

        public static ScpVoicePreset CryingPreset = new ScpVoicePreset()
        {
            Pitch = 0.95f,
            Distortion = 0.3f,
            LowPass = 3000f
        };

        public static ScpVoicePreset TryingPreset = new ScpVoicePreset()
        {
            Pitch = 0.9f,
            Distortion = 0.4f,
            LowPass = 2800f
        };

        public static ScpVoicePreset EnragingPreset = new ScpVoicePreset()
        {
            Pitch = 1.2f,
            Distortion = 1.5f,
            HighPass = 500f
        };

        public static ScpVoicePreset RagePreset = new ScpVoicePreset()
        {
            Pitch = 1.35f,
            Distortion = 3.0f,
            HighPass = 800f,
            Reverb = 0.15f
        };

        public static ScpVoicePreset ChargingPreset = new ScpVoicePreset()
        {
            Pitch = 1.25f,
            Distortion = 2.0f,
            HighPass = 600f
        };

        public static ScpVoicePreset PryingPreset = new ScpVoicePreset()
        {
            Pitch = 1.1f,
            Distortion = 2.5f,
            LowPass = 1500f
        };

        public static ScpVoicePreset GetPresetForState(Scp096VoiceState state)
        {
            switch (state)
            {
                case Scp096VoiceState.Calm:
                    return CalmPreset;
                case Scp096VoiceState.Crying:
                    return CryingPreset;
                case Scp096VoiceState.TryingNotToCry:
                    return TryingPreset;
                case Scp096VoiceState.Enraging:
                    return EnragingPreset;
                case Scp096VoiceState.Enraged:
                    return RagePreset;
                case Scp096VoiceState.Charging:
                    return ChargingPreset;
                case Scp096VoiceState.PryingGate:
                    return PryingPreset;
                default:
                    return CalmPreset;
            }
        }
    }
}
