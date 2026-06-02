namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    public class DistortionEffect : IAudioEffect
    {
        private readonly float _drive;

        public DistortionEffect(float drive)
        {
            _drive = drive;
        }

        public void Process(float[] pcm, int samples)
        {
            for (int i = 0; i < samples; i++)
            {
                float x = pcm[i] * _drive;

                if (x > 0.8f) x = 0.8f;
                if (x < -0.8f) x = -0.8f;

                pcm[i] = x;
            }
        }
    }

}
