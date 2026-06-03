namespace SCP_Immersive_Voice.AudioProcessing
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System.Collections.Generic;

    public class AudioEffectPipeline
    {
        private readonly List<IAudioEffectShort> _effects = new List<IAudioEffectShort>();

        public void Add(IAudioEffectShort effect) => _effects.Add(effect);

        public void Process(short[] pcm, int samples)
        {
            foreach (var effect in _effects)
                effect.Process(pcm, samples);
        }
    }

}