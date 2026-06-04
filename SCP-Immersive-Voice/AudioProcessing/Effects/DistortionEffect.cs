namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA Analog-modeled asymmetric tube distortion.
    /// Generates warm even harmonics for biological vocal strain using an optimized 
    /// polynomial waveshaper, an integrated DC-blocker, and high-frequency roll-off.
    /// </summary>
    public class DistortionEffect : IAudioEffect
    {
        public string Name => "Distortion";

        private readonly float _drive;
        private readonly float _makeupGain;

        // Stateful analog filter emulation variables
        private float _dcX1 = 0f;
        private float _dcY1 = 0f;
        private float _lpState = 0f;
        private readonly float _lpCoef;

        /// <summary>
        /// Initializes the Distortion effect.
        /// </summary>
        /// <param name="drive">Drive intensity (0.0f = clear, 1.0f = heavy saturation).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public DistortionEffect(float drive, float sampleRate)
        {
            // Clamp input to a safe studio range (0.0 to 1.0)
            _drive = Clamp(drive, 0f, 1f);

            // Calculate automatic makeup gain to prevent sudden volume spikes when drive increases
            _makeupGain = 1f / (1f + _drive * 0.5f);

            // Setup a high-frequency roll-off filter around 4500Hz to tame digital harshness/fuzz
            float sr = sampleRate > 0f ? sampleRate : 48000f;
            float lpfCutoff = 4500f;
            float omega = 2f * (float)Math.PI * lpfCutoff / sr;
            _lpCoef = omega / (omega + 1f);
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _drive < 0.01f) return;

            // Map linear drive (0..1) to an effective exponential saturation scale (1x to 8x)
            float inputGain = 1f + (_drive * 7f);

            for (int i = 0; i < length; i++)
            {
                float input = pcm[i];

                // 1. Push input into the saturation drive zone
                float driven = input * inputGain;

                // 2. Asymmetric Polynomial Waveshaper (Emulates Triode Valve/Tube characteristic)
                // Positives and negatives are shaped differently to harvest warm, musical even harmonics
                float saturated;
                if (driven > 0f)
                {
                    // Soft exponential curve for positive peaks
                    saturated = driven / (1f + driven);
                }
                else
                {
                    // Slightly tighter, compressed response for negative valleys
                    saturated = driven / (1f - driven * 0.4f);
                }

                // 3. Integrated Stateful DC Blocker (1st-order High-Pass Filter at ~10Hz)
                // Absolute structural protection against DC component drift caused by asymmetric shaping
                float dcFiltered = saturated - _dcX1 + 0.995f * _dcY1;
                _dcX1 = saturated;
                _dcY1 = dcFiltered;

                // 4. One-pole Low-Pass Filter (HF Roll-off)
                // Removes the strict, artificial high-frequency "fuzz" edges, replicating analog circuitry
                _lpState = _lpState + _lpCoef * (dcFiltered - _lpState);

                // 5. Apply makeup gain and write directly back into the PCM buffer
                pcm[i] = _lpState * _makeupGain;
            }
        }
    }
}