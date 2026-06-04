namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// AAA High-performance Bitcrusher utilizing exponential bit-mapping.
    /// Features a thread-isolated fast LCG TPDF dither, optimized mid-tread quantization,
    /// a true 1st-order DC blocker, and a rational soft-clipper. Zero allocations.
    /// </summary>
    public class BitcrushEffect : IAudioEffect
    {
        public string Name => "Bitcrush";

        private readonly float _steps;
        private readonly float _invSteps;
        private readonly float _ditherScale;

        // Stateful history registers for the true DC blocker
        private float _dcX1 = 0f;
        private float _dcY1 = 0f;

        // Thread-isolated fast LCG random state
        private uint _lcgState;

        public BitcrushEffect(float amount)
        {
            amount = Clamp(amount, 0f, 1f);

            // Map linear amount (0..1) exponentially to real bit depth (16 bits down to 2 bits)
            // This mirrors authentic hardware step degradation behavior
            float bits = 16f - (amount * 14f);
            _steps = (float)Math.Pow(2f, bits - 1f); // Account for bidirectional -1..1 range
            _invSteps = 1f / _steps;

            // TPDF Dither amplitude must scale perfectly with the quantization step size
            _ditherScale = _invSteps * 0.5f;

            _lcgState = (uint)Guid.NewGuid().GetHashCode();
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1) return;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // 1. High-speed bitwise LCG TPDF Dithering (Generates high-quality triangular noise floor)
                // We advance the local LCG sequence twice to fetch two independent random nodes
                _lcgState = _lcgState * 1103515245 + 12345;
                float r1 = ((float)(_lcgState & 0xFFFF) / 65535f);

                _lcgState = _lcgState * 1103515245 + 12345;
                float r2 = ((float)(_lcgState & 0xFFFF) / 65535f);

                float tpdfDither = (r1 - r2) * _ditherScale;
                float v = dry + tpdfDither;

                // Absolute structural protection clamp before floating-point truncation
                if (v > 1f) v = 1f;
                else if (v < -1f) v = -1f;

                // 2. High-performance Mid-Tread Quantization Loop
                // Enforces a reconstruction level at exactly zero to prevent gate chatter
                float quantized = (float)Math.Floor(v * _steps + 0.5f) * _invSteps;

                // 3. True Stateful 1st-order Recursive DC Blocker Filter (~10Hz High-Pass)
                float dcFiltered = quantized - _dcX1 + 0.995f * _dcY1;
                _dcX1 = quantized;
                _dcY1 = dcFiltered;

                // 4. Fast Rational Soft-Clipping (Replaces expensive Math.Tanh entirely)
                // Retains maximum signal energy transmission for downstream effects
                float driven = dcFiltered * 1.15f;
                float saturated = driven / (1f + Math.Abs(driven));

                pcm[i] = saturated;
            }
        }
    }
}