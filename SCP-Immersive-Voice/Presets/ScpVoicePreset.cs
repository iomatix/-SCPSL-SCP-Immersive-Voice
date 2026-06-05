namespace SCP_Immersive_Voice.Presets
{
    public class ScpVoicePreset
    {
        public bool Enable { get; set; } = false;
        public ScpVoicePresetMode Mode { get; set; } = ScpVoicePresetMode.Default;
        public bool IsGlobalTransmission { get; set; } = false;
        public float OutputGain { get; set; } = 1f;
        public bool UseNoiseGate { get; set; } = false;
        public float NoiseGateThreshold { get; set; } = -40.05f;
        public float Pitch { get; set; } = 1f;
        public float Formant { get; set; } = 1f;
        public float Distortion { get; set; } = 0f;
        public float LowPass { get; set; } = 0f;
        public float HighPass { get; set; } = 0f;
        public float Reverb { get; set; } = 0f;
        public float Tremolo { get; set; } = 0f;
        public float SiliconModulation { get; set; } = 0f;
        public float ScreechModulation { get; set; } = 0f;
        public float BreathNoise { get; set; } = 0f;
        public float WhisperAmount { get; set; } = 0f;
        public float DemonicOctaverMix { get; set; } = 0f;
        public float StoneCrack { get; set; } = 0f;
        public float StoneGrind { get; set; } = 0f;
        public float WetDecay { get; set; } = 0f;
        public float PocketEcho { get; set; } = 0f;
        public float FormantDrift { get; set; } = 0f;
        public float FleshCrackle { get; set; } = 0f;
        public float WetOrganic { get; set; } = 0f;
        public float Bitcrush { get; set; } = 0f;
        public float DataBurst { get; set; } = 0f;
        public float SampleRateReduce { get; set; } = 0f;
        public float Glitch { get; set; } = 0f;
        public float StaticNoise { get; set; } = 0f;
        public float Guttural { get; set; } = 0f;
        public float DryCrackle { get; set; } = 0f;
        public float Subharmonic { get; set; } = 0f;
        public float Chirp { get; set; } = 0f;


    }

}
