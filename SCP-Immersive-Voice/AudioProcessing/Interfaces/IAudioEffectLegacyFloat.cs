namespace SCP_Immersive_Voice.AudioProcessing.Interfaces
{
    public interface IAudioEffectLegacyFloat
    {
        void Process(float[] pcm, int samples);
    }

}
