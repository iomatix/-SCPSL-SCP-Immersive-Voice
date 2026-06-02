namespace SCP_Immersive_Voice.AudioProcessing
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System.Collections.Generic;

    public class AudioEffectPipeline
    {
        private readonly List<IAudioEffect> _effects = new List<IAudioEffect>();

        public void Add(IAudioEffect effect) => _effects.Add(effect);

        public void Process(float[] pcm, int samples)
        {
            foreach (var effect in _effects)
                effect.Process(pcm, samples);
        }
    }

}
