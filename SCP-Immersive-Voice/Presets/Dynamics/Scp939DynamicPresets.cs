namespace SCP_Immersive_Voice.Presets.Dynamics
{
    using SCP_Immersive_Voice.Presets;
    using SCP_Immersive_Voice.Presets.Dynamics.Enums;

    /// <summary>
    ///  Dynamic identity states for SCP‑939, tuned for whisper mimicry, deceptive vocal camouflage,
    /// and raw organic throat distortion. Calibrated to preserve absolute verbal intelligibility.
    /// </summary>
    public static class Scp939DynamicPresets
    {
        // =================================================================
        // IDLE WHISPER — Signature unvoiced camouflage layer (Matches core baseline)
        // =================================================================
        public static readonly ScpVoicePreset IdleWhisperPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 1.85f,
            Pitch = 0.55f,          // Intent: deep, predatory harmonic base
            Formant = 0.65f,        // Intent: heavily expanded reptilian vocal tract volume
            LowPass = 2400f,
            HighPass = 260f,
            WhisperAmount = 0.85f,  // Intent: primary unvoiced whisper conversion matrix
            BreathNoise = 0.50f,    // Intent: constant hissing, cold animalistic exhalation
            WetOrganic = 0.45f,     // Intent: dense mucous and saliva coating inside the jaw
            FormantDrift = 0.22f,
            Distortion = 0.08f      // Intent: minimal laryngeal gravel
        };

        // =================================================================
        // MIMICKING — Deceptive human speech with an Uncanny Valley leak
        // =================================================================
        public static readonly ScpVoicePreset MimickingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.00f,
            Pitch = 0.98f,          // Intent: close to real human fundamental frequency
            Formant = 1.00f,        // Intent: standard human vocal tract articulation
            LowPass = 5200f,
            HighPass = 0f,
            WhisperAmount = 0.10f,  // Intent: minimal unvoiced air leakage
            BreathNoise = 0.08f,
            WetOrganic = 0.12f,     //  FIX: Subtle fluid friction — words sound slightly too wet/slimy
            FormantDrift = 0.08f,   //  FIX: Micro LFO pitch drift to make the mimicry sound unstable
            Reverb = 0.05f
        };

        // =================================================================
        // FOCUSED — Predator focus: cold, sharpened, highly directional whisper
        // =================================================================
        public static readonly ScpVoicePreset FocusedPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.10f,
            Pitch = 0.58f,          // Intent: deep, controlled focus
            Formant = 0.68f,
            HighPass = 800f,        //  FIX: High pass filter cutting low rumbles to sharpen voice consonants
            LowPass = 2800f,
            WhisperAmount = 0.95f,  // Intent: maximum whisper conversion for complete sound dampening
            BreathNoise = 0.65f,    // Intent: sharp, intense airflow monitoring
            WetOrganic = 0.50f,
            Distortion = 0.15f,     // Intent: controlled taryngeal growl friction
            FormantDrift = 0.25f
        };

        // =================================================================
        // ATTACKING — Lunge execution: unhinged primal roar with devastating low-end
        // =================================================================
        public static readonly ScpVoicePreset AttackingPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 2.30f,      // Intent: dynamic boost to dominate combat scenes
            Pitch = 0.68f,          // Intent: pitch rises slightly due to raw kinetic violence
            Formant = 0.62f,        // Intent: jaw fully extended for maximum throat volume
            HighPass = 1500f,
            WhisperAmount = 0.70f,  // Intent: blending real vocal cords back in for aggressive energy
            BreathNoise = 1.20f,    // Intent: violent, massive lung exhalation pressure
            WetOrganic = 0.75f,
            WetDecay = 0.42f,
            Distortion = 1.20f,     // Intent: severe waveshaping saturation (tearing vocal cords)
            Guttural = 0.55f,       //  FIX: Heavy false-vocal fold rasping growl
            Subharmonic = 0.60f,    //  FIX: MASSIVE BASSOON: pristine tracking sub-octave chest rumble
            FormantDrift = 0.45f
        };

        // =================================================================
        // AMNESTIC CLOUD — Releasing fog: detached, hallucinogenic, smeared presence
        // =================================================================
        public static readonly ScpVoicePreset AmnesticCloudPreset = new ScpVoicePreset
        {
            Mode = ScpVoicePresetMode.Dynamic,
            Enable = true,
            OutputGain = 1.90f,
            Pitch = 0.85f,
            Formant = 0.80f,
            LowPass = 1200f,        //  FIX: Low-pass cutoff to simulate sound traveling through dense chemical mist
            HighPass = 0f,
            Reverb = 0.55f,         //  FIX: Diffused room reverb to create a detached, dislocated dreamscape
            WhisperAmount = 0.65f,  // Intent: chemical breath substitution
            BreathNoise = 0.40f,
            WetOrganic = 0.35f,
            FormantDrift = 0.30f,   // Intent: unstable, floating frequency envelope
            Distortion = 0.05f
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