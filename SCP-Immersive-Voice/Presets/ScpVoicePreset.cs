namespace SCP_Immersive_Voice.Presets
{
    /// <summary>
    /// High-performance POCO data container defining the entire floating-native coefficient matrix for DSP voice pipelines.
    /// </summary>
    public class ScpVoicePreset
    {
        #region Core Lifecycle Switches
        /// <summary> Toggles whether this specific DSP preset block is actively processing the player voice channel. </summary>
        public bool Enable { get; set; } = false;

        /// <summary> Defines the operational context mode layout assigned to this voice profile configuration. </summary>
        public ScpVoicePresetMode Mode { get; set; } = ScpVoicePresetMode.Default;

        /// <summary> Forces the processed buffer to bypass spatial localization rules and transmit globally to all clients. </summary>
        public bool IsGlobalTransmission { get; set; } = false;

        /// <summary> Master linear scale gain amplifier applied directly to the final composite PCM output buffer (0.0f to 2.0f). </summary>
        public float OutputGain { get; set; } = 1f;
        #endregion

        #region Dynamics & Gate Modification Channels
        /// <summary> Toggles the structural RMS energy-based environment background noise gate filter pipeline stage. </summary>
        public bool UseNoiseGate { get; set; } = false;

        /// <summary> The logarithmic noise floor execution threshold boundary mapped in decibels (-96.0f to 0.0f). </summary>
        public float NoiseGateThreshold { get; set; } = -40.05f;
        #endregion

        #region Fundamental Pitch & Formant Coefficients
        /// <summary> Time-domain pitch transposer multiplier scale ratio where 1.0f is clear bypass (0.25f to 4.0f). </summary>
        public float Pitch { get; set; } = 1f;

        /// <summary> Vocal tract cavity geometric scale ratio modifier tracking throat resonance layouts (0.5f to 2.0f). </summary>
        public float Formant { get; set; } = 1f;

        /// <summary> Continuous inharmonic center-frequency drift intensity simulating throat muscle spasms (0.0f to 1.5f). </summary>
        public float FormantDrift { get; set; } = 0f;
        #endregion

        #region Acoustic Space & Saturation Models
        /// <summary> Asymmetric polynomial triode valve distortion saturation drive engine scalar intensity (0.0f to 1.0f). </summary>
        public float Distortion { get; set; } = 0f;

        /// <summary> 2nd-order Butterworth Direct Form I Low-Pass filter frequency boundary threshold (20.0f to Nyquist Hz). </summary>
        public float LowPass { get; set; } = 0f;

        /// <summary> 2nd-order Butterworth Direct Form I High-Pass filter frequency boundary threshold (20.0f to Nyquist Hz). </summary>
        public float HighPass { get; set; } = 0f;

        /// <summary> Schroeder-Moorer parallel delay room matrix early reflection decay wet blend factor (0.0f to 1.0f). </summary>
        public float Reverb { get; set; } = 0f;
        #endregion

        #region Biomorphic & Vocal Emulation Filters
        /// <summary> High-frequency micro-jitter double-phase amplitude modulation layer depth scale vector (0.1f to 20.0f Hz). </summary>
        public float Tremolo { get; set; } = 0f;

        /// <summary> Multi-tap phase-dislocation falsetto fry shriek synthesizer matrix wet mix modifier (0.0f to 1.0f). </summary>
        public float VocalShriek { get; set; } = 0f;

        /// <summary> False vocal fold modulated feedback comb filter biological rasp texture depth multiplier (0.0f to 1.5f). </summary>
        public float Guttural { get; set; } = 0f;

        /// <summary> Zero-crossing flip-flop tracking time-domain subharmonic growl signal injection index (0.0f to 1.5f). </summary>
        public float Subharmonic { get; set; } = 0f;

        /// <summary> Uncanny valley comb-filtering dual asymmetric laryngeal cavity displacement path blend (0.0f to 1.0f). </summary>
        public float LaryngealAsymmetry { get; set; } = 0f;

        /// <summary> Low-frequency bubbling oscillator fluid-filled necrotic breathing texture intensity (0.0f to 1.0f). </summary>
        public float DeathRattle { get; set; } = 0f;

        /// <summary> De-voicing Voss-McCartney pink-noise articulation whisper synthesis layer transformation (0.0f to 1.0f). </summary>
        public float WhisperAmount { get; set; } = 0f;

        /// <summary> Stochastic white noise airflow velocity modeling respiration and hyperventilation cycles (0.0f to 1.0f). </summary>
        public float BreathNoise { get; set; } = 0f;

        /// <summary> Multi-band biological turbulence respiratory camouflage engine intensity designed for SCP-939 (0.0f to 1.0f). </summary>
        public float PredatoryCamouflage { get; set; } = 0f;
        #endregion

        #region Synthetic & Cybernetic Modulators
        /// <summary> Mainframe enclosure inharmonic transistor pseudo-square wave ring modulation blend factor (0.0f to 1.0f). </summary>
        public float SiliconModulation { get; set; } = 0f;

        /// <summary> Piercing glass-shattering ring modulation sweep focused at peak human pain thresholds (0.0f to 1.0f). </summary>
        public float ScreechModulation { get; set; } = 0f;

        /// <summary> High-speed bitwise power-of-two crossfading delay perfect pitch halving network blend (0.0f to 1.0f). </summary>
        public float DemonicOctaverMix { get; set; } = 0f;

        /// <summary> Non-linear mid-tread amplitude step quantizer reduction resolution depth scalar (0.0f to 1.0f). </summary>
        public float Bitcrush { get; set; } = 0f;

        /// <summary> Clock-divider thermal drift sample-and-hold downsampler digital distortion intensity (0.0f to 1.0f). </summary>
        public float SampleRateReduce { get; set; } = 0f;
        #endregion

        #region Environmental Texture & Material FX
        /// <summary> Macro structural fault line failure concrete fracture modeling impulse engine probability (0.0f to 2.0f). </summary>
        public float StoneCrack { get; set; } = 0f;

        /// <summary> Interlocking crystal stick-slip friction tectonic concrete grind abrasive scratch mix (0.0f to 2.0f). </summary>
        public float StoneGrind { get; set; } = 0f;

        /// <summary> Sparse stochastic impulse high-frequency skeletal joint ligament cracking granular density (0.0f to 1.5f). </summary>
        public float DryCrackle { get; set; } = 0f;

        /// <summary> Blended fast bitwise LCG muscle tissue tearing transient biomorphic spike resonance depth (0.0f to 1.5f). </summary>
        public float FleshCrackle { get; set; } = 0f;

        /// <summary> Sub-millisecond mucous phase shifting membrane thickness saliva micro-crackle saturation index (0.0f to 1.5f). </summary>
        public float WetOrganic { get; set; } = 0f;

        /// <summary> Viscous fluid absorption loop high-frequency dampened cavity decay reflection blend (0.0f to 1.5f). </summary>
        public float WetDecay { get; set; } = 0f;

        /// <summary> Non-Euclidean nested all-pass chaotic phase inversion reality bending echo matrix mix (0.0f to 1.5f). </summary>
        public float PocketEcho { get; set; } = 0f;
        #endregion

        #region Digital Degradation & Anomaly Signals
        /// <summary> Transistor square clipping data packet telemetry burst chirp synthesis trigger volume (0.0f to 1.0f). </summary>
        public float DataBurst { get; set; } = 0f;

        /// <summary> Micro-buffer freeze pointer lockup stutter quantization malfunction burst amount (0.0f to 1.5f). </summary>
        public float Glitch { get; set; } = 0f;

        /// <summary> Multi-layered electromagnetic RF intercom interference fizz circuit overdrive floor level (0.0f to 1.0f). </summary>
        public float StaticNoise { get; set; } = 0f;

        /// <summary> High-Q tracking resonator stochastic FM frequency down-sweep syrinx call modulation depth (0.0f to 1.0f). </summary>
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