using LabApi.Extensions;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using UnityEngine;

namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    /// <summary>
    /// High-Frequency Agonizing Screech Modulator for SCP-096.
    /// Utilizes dynamic envelope-driven inharmonic sideband modulation combined with 
    /// a high-Q human ear pain-threshold peaking filter (3150Hz) to synthesize glass-shattering shrieks.
    /// </summary>
    public class ScreechModulatorEffect : IAdjustableAudioEffect
    {
        #region Private Constants
        private const float TwoPi = 2f * Mathf.PI;
        #endregion

        #region Private Execution Vectors
        private float _amount;
        private readonly float _sampleRate;
        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;

        // Pre-calculated pain resonator biquad coefficients
        private float _painResonatorB0, _painResonatorB1, _painResonatorB2;
        private float _painResonatorA1, _painResonatorA2;

        // Stateful filter and oscillator registers managed via stack frames
        private float _hpX1, _hpY1;
        private float _prX1, _prX2, _prY1, _prY2;
        private float _carrierPhase;
        private float _envelope;
        #endregion

        #region Public Metadata Properties
        public string Name => "Screech Modulator";
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="ScreechModulatorEffect"/> class.
        /// </summary>
        /// <param name="amount">The intensity blend matrix index of the screech layer (0.0f to 1.0f).</param>
        /// <param name="sampleRate">The primary engine VoIP sample rate.</param>
        public ScreechModulatorEffect(float amount, float sampleRate)
        {
            // FLUENT API ALIGNMENT: Enforcing boundary safety straight via math extensions primitives
            _amount = amount.Clamp(0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            // PERFORMANCE FIX: Trigonometry coefficients mapped directly into float-native structures
            _envAttackCoef = Mathf.Exp(-1000f / (4f * _sampleRate));   // Fast 4ms voice attack
            _envReleaseCoef = Mathf.Exp(-1000f / (60f * _sampleRate)); // 60ms release decay

            _carrierPhase = 0f;
            _envelope = 0f;
            _hpX1 = 0f; _hpY1 = 0f;
            _prX1 = 0f; _prX2 = 0f; _prY1 = 0f; _prY2 = 0f;

            // Configure the Ear Pain Peaking Filter: Centered at 3150Hz (Peak human ear vulnerability)
            ConfigurePainPeakingFilter(3150f, 4.5f, 12.0f); // CenterFreq: 3150Hz, Q: 4.5, Gain: +12dB
        }
        #endregion

        #region High-Frequency DSP Hot-Path Loop
        public void Process(float[] pcm, int length)
        {
            if (pcm is null || length < 1 || _amount < 0.01f) return;

            float wetMix = _amount * 0.85f;
            float dryMix = 1f - (wetMix * 0.3f);

            // Pre-computed gain staging coefficients and high-pass boundaries outside the processing loop block.
            float wetGainFactor = wetMix * 1.5f;

            // 1st-order High-Pass filter coefficient to pre-clean chest mud (Cutoff ~1400Hz)
            float hpW0 = TwoPi * 1400f / _sampleRate;
            float hpAlpha = Mathf.Cos(hpW0) / (1f + Mathf.Sin(hpW0));

            // Cache volatile parameters, oscillators, and history registers directly onto stack memory lanes.
            // Bypasses persistent RAM allocation checks completely to lock down native execution speeds.
            float localEnvelope = _envelope;
            float localCarrierPhase = _carrierPhase;
            float localHpX1 = _hpX1;
            float localHpY1 = _hpY1;

            float localPrX1 = _prX1;
            float localPrX2 = _prX2;
            float localPrY1 = _prY1;
            float localPrY2 = _prY2;

            float att = _envAttackCoef;
            float rel = _envReleaseCoef;
            float rate = _sampleRate;

            float rB0 = _painResonatorB0, rB1 = _painResonatorB1, rB2 = _painResonatorB2;
            float rA1 = _painResonatorA1, rA2 = _painResonatorA2;

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // 1. Core voice envelope tracking via fluent mathematical primitives
                float absInput = dry.Abs();
                if (absInput > localEnvelope)
                {
                    localEnvelope = att * localEnvelope + (1f - att) * absInput;
                }
                else
                {
                    localEnvelope = rel * localEnvelope + (1f - rel) * absInput;
                }

                // 2. High-Pass structural pre-filter to isolate upper-mid throat tension
                float hpFiltered = hpAlpha * (localHpY1 + dry - localHpX1);
                localHpX1 = dry;
                localHpY1 = hpFiltered;

                // 3. Dynamic High-Frequency Carrier Modulation
                // The carrier sweeps upwards from 1300Hz to 2100Hz based on how loud the player screams,
                // generating piercing, non-harmonic sidebands that simulate voice shredding.
                float dynamicScreechFreq = 1300f + (localEnvelope * 800f);
                localCarrierPhase += (TwoPi * dynamicScreechFreq) / rate;
                if (localCarrierPhase > TwoPi)
                    localCarrierPhase -= TwoPi;

                // PERFORMANCE FIX: Swapped double precision Math.Sin for float-native SIMD optimized Mathf.Sin
                float modulationCarrier = Mathf.Sin(localCarrierPhase);
                float screechNode = hpFiltered * modulationCarrier;

                // 4. Run the raw screech through the 3150Hz ear pain-threshold resonator inside local stack frames
                float painShriek = rB0 * screechNode + rB1 * localPrX1 + rB2 * localPrX2 - rA1 * localPrY1 - rA2 * localPrY2;

                // Shift time registers inside high-speed local processing lanes
                localPrX2 = localPrX1;
                localPrX1 = screechNode;
                localPrY2 = localPrY1;
                localPrY1 = painShriek;

                // 5. Asymmetric clipping to introduce organic vocal cord tearing texture via fluent abs extensions
                float drivenShriek = painShriek * 2.2f;
                float distortedShriek = drivenShriek / (1f + drivenShriek.Abs());

                // 6. Linear combination injection back into the VoIP array target buffer using pre-computed staging
                pcm[i] = (dry * dryMix) + (distortedShriek * wetGainFactor * localEnvelope);
            }

            // Flush calculated stack structures back into object instance context fields atomically post sweep.
            _envelope = localEnvelope;
            _carrierPhase = localCarrierPhase;
            _hpX1 = localHpX1;
            _hpY1 = localHpY1;
            _prX1 = localPrX1; _prX2 = localPrX2; _prY1 = localPrY1; _prY2 = localPrY2;
        }
        #endregion

        #region Internal Filter Configrators
        private void ConfigurePainPeakingFilter(float frequency, float q, float gainDb)
        {
            // PERFORMANCE FIX: Eradicated double precision metadata calculations (Math.Pow, Math.Sin, Math.Cos)
            float w0 = TwoPi * frequency / _sampleRate;
            float alpha = Mathf.Sin(w0) / (2f * q);
            float a = Mathf.Pow(10f, gainDb / 40f);
            float cosW0 = Mathf.Cos(w0);

            float a0 = 1f + (alpha / a);
            _painResonatorB0 = (1f + (alpha * a)) / a0;
            _painResonatorB1 = (-2f * cosW0) / a0;
            _painResonatorB2 = (1f - (alpha * a)) / a0;
            _painResonatorA1 = (-2f * cosW0) / a0;
            _painResonatorA2 = (1f - (alpha / a)) / a0;
        }
        #endregion

        #region Operational Parameter Adjustments
        public void AdjustParameter(float value)
        {
            _amount = value.Clamp(0f, 1f);
        }
        #endregion
    }
}