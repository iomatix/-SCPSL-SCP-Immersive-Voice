namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;


    public class ReverbEffect : IAudioEffect
    {
        private readonly float _mix;

        public ReverbEffect(float mix)
        {
            _mix = mix;
        }

        public void Process(float[] pcm, int samples)
        {
            int delay = 125; // ~2.5 ms
            for (int i = delay; i < samples; i++)
            {
                pcm[i] += pcm[i - delay] * _mix;
            }
        }
    }
}
