namespace SCP_Immersive_Voice.AudioProcessing.Interfaces
{
    /// <summary>
    /// Defines a contract for real-time audio effects that support runtime scalar parameter adjustments without reflection.
    /// </summary>
    public interface IAdjustableAudioEffect : IAudioEffect
    {
        /// <summary>
        /// Updates the primary operational parameter of the effect dynamically.
        /// </summary>
        /// <param name="value">The new scalar control voltage or configuration value.</param>
        void AdjustParameter(float value);
    }
}