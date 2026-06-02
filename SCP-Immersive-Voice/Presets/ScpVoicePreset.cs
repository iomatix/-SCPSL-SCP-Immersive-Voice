namespace SCP_Immersive_Voice.Presets
{
    public class ScpVoicePreset
    {
        public bool Enable { get; set; } = true;
        public ScpVoicePresetMode Mode { get; set; } = ScpVoicePresetMode.Default;
        public float Pitch { get; set; } = 1f;
        public float Formant { get; set; } = 1f;
        public float Distortion { get; set; } = 0f;
        public float LowPass { get; set; } = 0f;
        public float HighPass { get; set; } = 0f;
        public float Reverb { get; set; } = 0f;
        public float BreathNoise { get; set; } = 0f;
        public float WhisperAmount { get; set; } = 0f;
        public float StoneCrack { get; set; } = 0f;
        public float StoneGrind { get; set; } = 0f;

    }

}
