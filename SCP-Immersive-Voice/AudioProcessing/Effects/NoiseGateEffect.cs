namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;
    using static Utils.MathUtils;

    /// <summary>
    /// Simple amplitude-based noise gate.
    /// Used only for SCP-173 to suppress constant stone noise.
    /// </summary>
    public class NoiseGateEffect : IAudioEffect
    {
        public string Name => "Noise Gate";

        private readonly float _threshold;
        private float _envelope;

        public NoiseGateEffect(float threshold)
        {
            _threshold = Clamp(threshold, 0f, 0.2f);
        }

        public void Process(float[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                float v = Math.Abs(pcm[i]);

                // Envelope follower
                _envelope = _envelope * 0.95f + v * 0.05f;

                // If below threshold → attenuate
                if (_envelope < _threshold)
                    pcm[i] *= 0.1f; // reduce by 90%
            }
        }
    }
}
