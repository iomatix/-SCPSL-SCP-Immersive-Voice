namespace SCP_Immersive_Voice.AudioProcessing.Interfaces
{
    public interface IResettableAudioEffect : IAudioEffect
    {
        void ResetState();
    }
}