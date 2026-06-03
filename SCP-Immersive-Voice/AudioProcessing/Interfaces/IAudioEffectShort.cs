namespace SCP_Immersive_Voice.AudioProcessing.Interfaces
{
    public interface IAudioEffectShort
    {
        void Process(short[] pcm, int length);
    }
}
