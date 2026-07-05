namespace SCP_Immersive_Voice.Presets
{
    /// <summary>
    /// Specifies the evaluation pipeline execution mode for a given voice profile.
    /// </summary>
    public enum ScpVoicePresetMode
    {
        /// <summary>
        /// Static profile loaded directly from configuration file mappings.
        /// </summary>
        Default,

        /// <summary>
        /// Variable state graph driven dynamically by live gameplay triggers (e.g. SCP-096 Rage).
        /// </summary>
        Dynamic
    }
}