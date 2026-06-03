namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    public static class Scp3114DynamicPresets
    {
        // 3114 — Undisguised (true form)
        // High, unstable, wet, twitchy, organic.
        public static readonly ScpVoicePreset UndisguisedPreset = new ScpVoicePreset
        {
            Pitch = 1.75f,
            Formant = 1.18f,
            Distortion = 0.55f,
            HighPass = 220f,
            Reverb = 0.15f,
            FormantDrift = 0.45f,
            FleshCrackle = 0.65f,
            WetOrganic = 0.75f,
            WetDecay = 0.25f
        };

        // 3114 — Disguising (transition into human form)
        // Meaty, wet, unstable, but lowering pitch and formant.
        public static readonly ScpVoicePreset DisguisingPreset = new ScpVoicePreset
        {
            Pitch = 1.35f,
            Formant = 0.92f,
            Distortion = 0.45f,
            LowPass = 2400f,
            Reverb = 0.22f,
            FormantDrift = 0.35f,
            FleshCrackle = 0.75f,
            WetOrganic = 0.85f,
            WetDecay = 0.35f
        };

        // 3114 — Disguised (pretending to be human)
        // Must sound almost normal, but with subtle uncanny artifacts.
        public static readonly ScpVoicePreset DisguisedPreset = new ScpVoicePreset
        {
            Pitch = 1.02f,
            Formant = 1.00f,
            Distortion = 0.0f,
            LowPass = 0f,
            HighPass = 0f,
            Reverb = 0f,
            FormantDrift = 0.02f,
            FleshCrackle = 0.03f,
            WetOrganic = 0.05f,
            WetDecay = 0f
        };

        // 3114 — Revealing (transition out of disguise)
        // Organic tearing, rising pitch, unstable formants.
        public static readonly ScpVoicePreset RevealingPreset = new ScpVoicePreset
        {
            Pitch = 1.55f,
            Formant = 1.05f,
            Distortion = 0.65f,
            LowPass = 2200f,
            Reverb = 0.25f,
            FormantDrift = 0.45f,
            FleshCrackle = 0.85f,
            WetOrganic = 0.90f,
            WetDecay = 0.45f
        };

        // 3114 — Strangling (attack mode)
        // High, aggressive, wet, tearing flesh, unstable.
        public static readonly ScpVoicePreset StranglingPreset = new ScpVoicePreset
        {
            Pitch = 1.95f,
            Formant = 1.28f,
            Distortion = 1.1f,
            HighPass = 380f,
            Reverb = 0.18f,
            FormantDrift = 0.65f,
            FleshCrackle = 1.0f,
            WetOrganic = 1.0f,
            WetDecay = 0.65f,
            Guttural = 0.35f
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