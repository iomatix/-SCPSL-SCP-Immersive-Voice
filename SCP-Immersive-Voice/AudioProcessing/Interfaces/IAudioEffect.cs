namespace SCP_Immersive_Voice.AudioProcessing.Interfaces
{
    public interface IAudioEffect
    {
        void Process(float[] pcm, int samples);
    }

}
