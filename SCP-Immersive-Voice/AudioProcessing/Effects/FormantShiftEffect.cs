namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using UnityEngine;
    public class FormantShiftEffect : IAudioEffect
    {
        private readonly float _formant;

        public FormantShiftEffect(float formant)
        {
            _formant = formant;
        }

        public void Process(float[] pcm, int samples)
        {
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float shift = Mathf.Lerp(1f, _formant, t);
                pcm[i] *= shift;
            }
        }
    }
}
