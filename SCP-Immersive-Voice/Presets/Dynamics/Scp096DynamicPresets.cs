namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    public static class Scp096DynamicPresets
    {
        // 096 — Calm (baseline trembling)
        public static readonly ScpVoicePreset CalmPreset = new ScpVoicePreset
        {
            Pitch = 0.92f,
            Formant = 1.0f,
            Distortion = 0.15f,
            WhisperAmount = 0.25f,
            WetOrganic = 0.35f,
            FormantDrift = 0.25f,
            LowPass = 3600f
        };

        // 096 — Crying (unstable, breathy, emotional collapse)
        public static readonly ScpVoicePreset CryingPreset = new ScpVoicePreset
        {
            Pitch = 0.88f,
            Formant = 0.95f,
            Distortion = 0.25f,
            WhisperAmount = 0.45f,
            WetOrganic = 0.55f,
            WetDecay = 0.25f,
            FormantDrift = 0.35f,
            LowPass = 3100f
        };

        // 096 — Trying Not To Cry (voice cracking, unstable)
        public static readonly ScpVoicePreset TryingPreset = new ScpVoicePreset
        {
            Pitch = 0.85f,
            Formant = 0.92f,
            Distortion = 0.35f,
            WhisperAmount = 0.35f,
            WetOrganic = 0.65f,
            WetDecay = 0.35f,
            FormantDrift = 0.45f,
            LowPass = 2800f
        };

        // 096 — Enraging (voice breaking, rising aggression)
        public static readonly ScpVoicePreset EnragingPreset = new ScpVoicePreset
        {
            Pitch = 1.15f,
            Formant = 1.05f,
            Distortion = 1.25f,
            HighPass = 450f,
            WetOrganic = 0.45f,
            WetDecay = 0.55f,
            FormantDrift = 0.55f,
            Subharmonic = 0.35f
        };

        // 096 — Enraged (full scream, tearing throat)
        public static readonly ScpVoicePreset RagePreset = new ScpVoicePreset
        {
            Pitch = 1.38f,
            Formant = 1.15f,
            Distortion = 3.0f,
            HighPass = 900f,
            WetDecay = 0.85f,
            WetOrganic = 0.35f,
            Guttural = 0.45f,
            Subharmonic = 0.55f,
            Reverb = 0.20f,
            FormantDrift = 0.65f
        };

        // 096 — Charging (violent, breath tearing)
        public static readonly ScpVoicePreset ChargingPreset = new ScpVoicePreset
        {
            Pitch = 1.28f,
            Formant = 1.10f,
            Distortion = 2.2f,
            HighPass = 650f,
            WetDecay = 0.65f,
            Guttural = 0.55f,
            Subharmonic = 0.45f,
            FormantDrift = 0.55f
        };

        // 096 — Charged (focused aggression, less chaotic)
        public static readonly ScpVoicePreset ChargedPreset = new ScpVoicePreset
        {
            Pitch = 1.22f,
            Formant = 1.08f,
            Distortion = 2.0f,
            HighPass = 600f,
            WetDecay = 0.55f,
            Guttural = 0.45f,
            Subharmonic = 0.35f,
            FormantDrift = 0.45f
        };

        // 096 — Prying Gate (straining, metal resonance)
        public static readonly ScpVoicePreset PryingPreset = new ScpVoicePreset
        {
            Pitch = 1.05f,
            Formant = 0.95f,
            Distortion = 2.4f,
            LowPass = 1600f,
            WetDecay = 0.45f,
            WetOrganic = 0.35f,
            Guttural = 0.35f,
            Subharmonic = 0.25f
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

                case Scp096VoiceState.Charged:
                    return ChargedPreset;

                case Scp096VoiceState.PryingGate:
                    return PryingPreset;

                default:
                    return CalmPreset;
            }
        }
    }
}