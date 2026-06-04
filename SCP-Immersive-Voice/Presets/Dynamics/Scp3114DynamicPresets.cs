namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Dynamic identity states for SCP‑3114, tuned for organic instability,
    /// flesh resonance and uncanny transitions between human and non‑human form.
    /// </summary>
    public static class Scp3114DynamicPresets
    {
        // True form: twitchy, wet, unstable, high‑tension organic mass
        public static readonly ScpVoicePreset UndisguisedPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.4f,
            Pitch = 1.60f,
            Formant = 1.12f,
            Distortion = 0.45f,
            HighPass = 200f,
            Reverb = 0.12f,
            FormantDrift = 0.40f,
            FleshCrackle = 0.60f,
            WetOrganic = 0.70f,
            WetDecay = 0.22f
        };

        // Transition into human shape: meaty, wet, collapsing structure
        public static readonly ScpVoicePreset DisguisingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.5f,
            Pitch = 1.28f,
            Formant = 0.90f,
            Distortion = 0.40f,
            LowPass = 2400f,
            Reverb = 0.18f,
            FormantDrift = 0.32f,
            FleshCrackle = 0.70f,
            WetOrganic = 0.80f,
            WetDecay = 0.30f
        };

        // Human mimicry: almost normal, but subtly wrong and too smooth
        public static readonly ScpVoicePreset DisguisedPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.0f,
            Pitch = 1.02f,
            Formant = 1.00f,
            Distortion = 0.00f,
            LowPass = 0f,
            HighPass = 0f,
            Reverb = 0f,
            FormantDrift = 0.02f,
            FleshCrackle = 0.02f,
            WetOrganic = 0.04f,
            WetDecay = 0f
        };

        // Transition out of disguise: tearing, rising pitch, unstable resonance
        public static readonly ScpVoicePreset RevealingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.5f,
            Pitch = 1.48f,
            Formant = 1.05f,
            Distortion = 0.55f,
            LowPass = 2200f,
            Reverb = 0.22f,
            FormantDrift = 0.42f,
            FleshCrackle = 0.80f,
            WetOrganic = 0.85f,
            WetDecay = 0.40f
        };

        // Attack mode: high, aggressive, tearing flesh, unstable identity collapse
        public static readonly ScpVoicePreset StranglingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.65f,
            Pitch = 1.85f,
            Formant = 1.22f,
            Distortion = 0.95f,
            HighPass = 360f,
            Reverb = 0.15f,
            FormantDrift = 0.60f,
            FleshCrackle = 1.00f,
            WetOrganic = 1.00f,
            WetDecay = 0.60f,
            Guttural = 0.30f
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
