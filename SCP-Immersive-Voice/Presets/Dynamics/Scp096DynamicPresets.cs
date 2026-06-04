namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    /// Dynamic emotional states for SCP‑096, tuned for cinematic instability and escalating terror.
    /// Designed to blend seamlessly with the corrected DSP pipeline.
    /// </summary>
    public static class Scp096DynamicPresets
    {
        // Baseline trembling, fragile but controlled
        public static readonly ScpVoicePreset CalmPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.01f,
            Pitch = 0.92f,
            Formant = 1.00f,
            Distortion = 0.12f,
            WhisperAmount = 0.20f,
            WetOrganic = 0.28f,
            FormantDrift = 0.22f,
            LowPass = 3600f
        };

        // Emotional collapse, breathy instability, soft crying resonance
        public static readonly ScpVoicePreset CryingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 1.85f,
            Pitch = 0.88f,
            Formant = 0.95f,
            Distortion = 0.22f,
            WhisperAmount = 0.40f,
            WetOrganic = 0.50f,
            WetDecay = 0.22f,
            FormantDrift = 0.32f,
            LowPass = 3100f
        };

        // Voice cracking, unstable throat tension, rising panic
        public static readonly ScpVoicePreset TryingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.15f,
            Pitch = 0.84f,
            Formant = 0.92f,
            Distortion = 0.30f,
            WhisperAmount = 0.32f,
            WetOrganic = 0.60f,
            WetDecay = 0.32f,
            FormantDrift = 0.42f,
            LowPass = 2800f
        };

        // Aggression rising, throat tearing, unstable resonance
        public static readonly ScpVoicePreset EnragingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.35f,
            Pitch = 1.12f,
            Formant = 1.05f,
            Distortion = 1.10f,
            HighPass = 420f,
            WetOrganic = 0.40f,
            WetDecay = 0.50f,
            FormantDrift = 0.50f,
            Subharmonic = 0.30f
        };

        // Full scream, tearing vocal folds, dimensional resonance
        public static readonly ScpVoicePreset RagePreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.45f,
            Pitch = 1.32f,
            Formant = 1.12f,
            Distortion = 2.40f,
            HighPass = 850f,
            WetDecay = 0.80f,
            WetOrganic = 0.30f,
            Guttural = 0.40f,
            Subharmonic = 0.50f,
            Reverb = 0.18f,
            FormantDrift = 0.60f
        };

        // Violent motion, breath tearing, chaotic throat pressure
        public static readonly ScpVoicePreset ChargingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.25f,
            Pitch = 1.24f,
            Formant = 1.08f,
            Distortion = 1.90f,
            HighPass = 600f,
            WetDecay = 0.60f,
            Guttural = 0.50f,
            Subharmonic = 0.42f,
            FormantDrift = 0.50f
        };

        // Focused aggression, less chaotic but still tearing
        public static readonly ScpVoicePreset ChargedPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.35f,
            Pitch = 1.18f,
            Formant = 1.06f,
            Distortion = 1.70f,
            HighPass = 550f,
            WetDecay = 0.50f,
            Guttural = 0.42f,
            Subharmonic = 0.32f,
            FormantDrift = 0.42f
        };

        // Straining against metal, throat pressure + material resonance
        public static readonly ScpVoicePreset PryingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            OutputGain = 2.45f,
            Pitch = 1.02f,
            Formant = 0.95f,
            Distortion = 2.10f,
            LowPass = 1700f,
            WetDecay = 0.42f,
            WetOrganic = 0.30f,
            Guttural = 0.32f,
            Subharmonic = 0.22f
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