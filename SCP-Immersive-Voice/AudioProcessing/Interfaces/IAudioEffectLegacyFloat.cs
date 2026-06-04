namespace SCP_Immersive_Voice.AudioProcessing.Interfaces
{
    /// <summary>
    /// Base interface for all real-time audio effects operating on normalized float PCM (-1..1).
    /// </summary>
    public interface IAudioEffect
    {
        string Name { get; }

        /// <summary>
        /// Processes the given PCM buffer in-place.
        /// </summary>
        /// <param name="pcm">Float PCM buffer (-1..1), mono.</param>
        /// <param name="length">Number of valid samples in the buffer.</param>
        void Process(float[] pcm, int length);

    }
}
