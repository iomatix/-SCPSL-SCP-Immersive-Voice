namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using static SCP_Immersive_Voice.AudioProcessing.Utils.MathUtils;
    using System;

    /// <summary>
    /// High-Frequency Agonizing Screech Modulator for SCP-096.
    /// Utilizes dynamic envelope-driven inharmonic sideband modulation combined with 
    /// a high-Q human ear pain-threshold peaking filter (3150Hz) to synthesize glass-shattering shrieks.
    /// </summary>
    public class ScreechModulatorEffect : IAudioEffect
    {
        public string Name => "Screech Modulator";

        private readonly float _amount;
        private readonly float _sampleRate;

        // Persistent filter and oscillator registers
        private float _hpX1, _hpY1; // 1st-order High-Pass tracking registers
        private float _painResonatorB0, _painResonatorB1, _painResonatorB2, _painResonatorA1, _painResonatorA2;
        private float _prX1, _prX2, _prY1, _prY2; // 2nd-order Peaking Filter registers

        private float _carrierPhase = 0f;
        private float _envelope = 0f;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;
        private const float TwoPi = (float)(Math.PI * 2.0);

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreechModulatorEffect"/> class.
        /// </summary>
        public ScreechModulatorEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _envAttackCoef = (float)Math.Exp(-1000.0 / (4f * _sampleRate));   // Fast 4ms voice attack
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (60f * _sampleRate)); // 60ms release decay

            // Configure the  Ear Pain Peaking Filter: Centered at 3150Hz (Peak human ear vulnerability)
            ConfigurePainPeakingFilter(3150f, 4.5f, 12.0f); // CenterFreq: 3150Hz, Q: 4.5, Gain: +12dB
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            float wetMix = _amount * 0.85f;
            float dryMix = 1f - (wetMix * 0.3f);

            // 1st-order High-Pass filter coefficient to pre-clean chest mud (Cutoff ~1400Hz)
            float hpW0 = TwoPi * 1400f / _sampleRate;
            float hpAlpha = (float)Math.Cos(hpW0) / (1f + (float)Math.Sin(hpW0));

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // 1. Core voice envelope tracking
                float absInput = Math.Abs(dry);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                // 2. High-Pass structural pre-filter to isolate upper-mid throat tension
                float hpFiltered = hpAlpha * (_hpY1 + dry - _hpX1);
                _hpX1 = dry;
                _hpY1 = hpFiltered;

                // 3. Dynamic High-Frequency Carrier Modulation
                // The carrier sweeps upwards from 1300Hz to 2100Hz based on how loud the player screams,
                // generating piercing, non-harmonic sidebands that simulate voice shredding.
                float dynamicScreechFreq = 1300f + (_envelope * 800f);
                _carrierPhase += (TwoPi * dynamicScreechFreq) / _sampleRate;
                if (_carrierPhase > TwoPi) _carrierPhase -= TwoPi;

                float modulationCarrier = (float)Math.Sin(_carrierPhase);
                float screechNode = hpFiltered * modulationCarrier;

                // 4. Run the raw screech through the 3150Hz ear pain-threshold resonator
                float painShriek = _painResonatorB0 * screechNode + _painResonatorB1 * _prX1 + _painResonatorB2 * _prX2
                                   - _painResonatorA1 * _prY1 - _painResonatorA2 * _prY2;

                _prX2 = _prX1; _prX1 = screechNode;
                _prY2 = _prY1; _prY1 = painShriek;

                // 5. Asymmetric clipping to introduce organic vocal cord tearing texture
                float drivenShriek = painShriek * 2.2f;
                float distortedShriek = drivenShriek / (1f + Math.Abs(drivenShriek));

                // 6. Linear combination injection back into the VoIP array
                pcm[i] = (dry * dryMix) + (distortedShriek * wetMix * _envelope * 1.5f);
            }
        }

        private void ConfigurePainPeakingFilter(float frequency, float q, float gainDb)
        {
            float w0 = TwoPi * frequency / _sampleRate;
            float alpha = (float)Math.Sin(w0) / (2f * q);
            float a = (float)Math.Pow(10.0, gainDb / 40.0);
            float cosW0 = (float)Math.Cos(w0);

            float a0 = 1f + (alpha / a);
            _painResonatorB0 = (1f + (alpha * a)) / a0;
            _painResonatorB1 = (-2f * cosW0) / a0;
            _painResonatorB2 = (1f - (alpha * a)) / a0;
            _painResonatorA1 = (-2f * cosW0) / a0;
            _painResonatorA2 = (1f - (alpha / a)) / a0;
        }
    }
}