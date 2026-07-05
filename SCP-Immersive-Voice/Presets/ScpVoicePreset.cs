namespace SCP_Immersive_Voice.Presets
{
    /// <summary>
    /// High-performance POCO data container defining the entire floating-native coefficient matrix for DSP voice pipelines.
    /// </summary>
    public class ScpVoicePreset
    {
        #region Core Lifecycle Switches
        public bool Enable { get; set; } = false;
        public ScpVoicePresetMode Mode { get; set; } = ScpVoicePresetMode.Default;
        public bool IsGlobalTransmission { get; set; } = false;
        public float OutputGain { get; set; } = 1f;
        #endregion

        #region Dynamics & Gate Modification Channels
        public bool UseNoiseGate { get; set; } = false;
        public float NoiseGateThreshold { get; set; } = -40.05f;
        #endregion

        #region Fundamental Pitch & Formant Coefficients
        public float Pitch { get; set; } = 1f;
        public float Formant { get; set; } = 1f;
        public float FormantDrift { get; set; } = 0f;
        #endregion

        #region Acoustic Space & Saturation Models
        public float Distortion { get; set; } = 0f;
        public float LowPass { get; set; } = 0f;
        public float HighPass { get; set; } = 0f;
        public float Reverb { get; set; } = 0f;
        #endregion

        #region Biomorphic & Vocal Emulation Filters
        public float Tremolo { get; set; } = 0f;
        public float VocalShriek { get; set; } = 0f;
        public float Guttural { get; set; } = 0f;
        public float Subharmonic { get; set; } = 0f;
        public float LaryngealAsymmetry { get; set; } = 0f;
        public float DeathRattle { get; set; } = 0f;
        public float WhisperAmount { get; set; } = 0f;
        public float BreathNoise { get; set; } = 0f;
        #endregion

        #region Synthetic & Cybernetic Modulators
        public float SiliconModulation { get; set; } = 0f;
        public float ScreechModulation { get; set; } = 0f;
        public float DemonicOctaverMix { get; set; } = 0f;
        public float Bitcrush { get; set; } = 0f;
        public float SampleRateReduce { get; set; } = 0f;
        #endregion

        #region Environmental Texture & Material FX
        public float StoneCrack { get; set; } = 0f;
        public float StoneGrind { get; set; } = 0f;
        public float DryCrackle { get; set; } = 0f;
        public float FleshCrackle { get; set; } = 0f;
        public float WetOrganic { get; set; } = 0f;
        public float WetDecay { get; set; } = 0f;
        public float PocketEcho { get; set; } = 0f;
        #endregion

        #region Digital Degradation & Anomaly Signals
        public float DataBurst { get; set; } = 0f;
        public float Glitch { get; set; } = 0f;
        public float StaticNoise { get; set; } = 0f;
        public float Chirp { get; set; } = 0f;
        #endregion

        #region Atomic Object Cloning Lifecycle
        /// <summary>
        /// Performs an allocation-optimized shallow copy of the coefficient matrix to ensure threat-isolated thread processing.
        /// </summary>
        /// <returns>A clean, independent clone of the current configuration profile.</returns>
        public ScpVoicePreset Clone() => (ScpVoicePreset)MemberwiseClone();
        #endregion
    }
}