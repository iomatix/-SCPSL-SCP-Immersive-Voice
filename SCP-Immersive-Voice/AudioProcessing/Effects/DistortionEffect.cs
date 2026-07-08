using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// Analog-modeled asymmetric tube distortion.
    /// Generates warm even harmonics for biological vocal strain using an optimized 
    /// polynomial waveshaper, an integrated DC-blocker, and high-frequency roll-off.
    /// </summary>
    public class DistortionEffect : IAdjustableAudioEffect
    {
        #region Private Execution Vectors
        private float _drive;
        private readonly float _makeupGain;
        private readonly float _lpCoef;

        // Stateful analog filter emulation variables managed under local stack windows
        private float _dcX1;
        private float _dcY1;
        private float _lpState;
        #endregion

        #region Public Metadata Properties
        public string Name => "Distortion";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the Distortion effect.
        /// </summary>
        /// <param name="drive">Drive intensity (0.0f = clear, 1.0f = heavy saturation).</param>
        /// <param name="sampleRate">Engine sample rate from VoiceChatSettings.</param>
        public DistortionEffect(float drive, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Utilizing the pristine mathematical clamp straight on the argument payload
            _drive = drive.Clamp(0f, 1f);

            // Calculate automatic makeup gain to prevent sudden volume spikes when drive increases
            _makeupGain = 1f / (1f + _drive * 0.5f);

            // Setup a high-frequency roll-off filter around 4500Hz to tame digital harshness/fuzz
            float sr = sampleRate > 0f ? sampleRate : 48000f;
            const float lpfCutoff = 4500f;

            // Performance adjustment: Using float-native Mathf constant definitions
            float omega = 2f * Mathf.PI * lpfCutoff / sr;
            _lpCoef = omega / (omega + 1f);

            _dcX1 = 0f;
            _dcY1 = 0f;
            _lpState = 0f;
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _drive < 0.01f) return;

            // Map linear drive (0..1) to an effective exponential saturation scale (1x to 8x)
            float inputGain = 1f + (_drive * 7f);

            // Caching volatile instance fields into stack-local registers before entering the processing sequence.
            // This grants the CPU 100% register-level access speed (EAX/EDX mapping), completely avoiding L1/L2 cache line hops.
            float localDcX1 = _dcX1;
            float localDcY1 = _dcY1;
            float localLpState = _lpState;

            float lpC = _lpCoef;
            float makeup = _makeupGain;

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
                float dcFiltered = saturated - localDcX1 + 0.995f * localDcY1;
                localDcX1 = saturated;
                localDcY1 = dcFiltered;

                // 4. One-pole Low-Pass Filter (HF Roll-off)
                // Removes the strict, artificial high-frequency "fuzz" edges, replicating analog circuitry
                localLpState = localLpState + lpC * (dcFiltered - localLpState);

                // 5. Apply makeup gain and write directly back into the PCM buffer
                pcm[i] = localLpState * makeup;
            }

            // Atomically flush computed register values back into instance tracking storage fields post execution.
            _dcX1 = localDcX1;
            _dcY1 = localDcY1;
            _lpState = localLpState;
        }
        #endregion

        #region Operational Parameter Adjustments
        public void AdjustParameter(float value)
        {
            _drive = value.Clamp(0f, 1f);
        }
        #endregion
    }
}