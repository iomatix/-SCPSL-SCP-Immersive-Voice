namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    ///  Custom Silicon Ring Modulator and Mainframe Enclosure Comb Resonator.
    /// Destroys human harmonic intervals using a tracking low-frequency carrier wave 
    /// and simulates empty server-rack steel cabinet acoustic boundary reflections. Zero allocations.
    /// </summary>
    public class SiliconRingModulatorEffect : IAudioEffect
    {
        public string Name => "Silicon Ring Modulator";

        private readonly float _amount;
        private readonly float _sampleRate;

        // Persistent state registers
        private float _carrierPhase = 0f;
        private float _envelope = 0f;

        // Stack-allocated style fixed circular buffer for server chassis acoustic comb reflections (4ms delay boundary)
        private const int CombDelaySamples = 144;
        private readonly float[] _combBuffer = new float[CombDelaySamples];
        private int _combWritePtr = 0;

        private readonly float _envAttackCoef;
        private readonly float _envReleaseCoef;
        private const float TwoPi = (float)(Math.PI * 2.0);

        /// <summary>
        /// Initializes a new instance of the <see cref="SiliconRingModulatorEffect"/> class.
        /// </summary>
        public SiliconRingModulatorEffect(float amount, float sampleRate)
        {
            _amount = Clamp(amount, 0f, 1f);
            _sampleRate = sampleRate > 0f ? sampleRate : 48000f;

            _envAttackCoef = (float)Math.Exp(-1000.0 / (5f * _sampleRate));   // 5ms tracking response
            _envReleaseCoef = (float)Math.Exp(-1000.0 / (75f * _sampleRate)); // 75ms release smoothing
        }

        public void Process(float[] pcm, int length)
        {
            if (length < 1 || _amount < 0.01f) return;

            // Gain scaling matrix
            float wetMix = _amount * 0.75f;
            float dryMix = 1f - (wetMix * 0.4f);

            for (int i = 0; i < length; i++)
            {
                float dry = pcm[i];

                // 1. Fast RMS-style absolute voice envelope tracking
                float absInput = Math.Abs(dry);
                if (absInput > _envelope)
                    _envelope = _envAttackCoef * _envelope + (1f - _envAttackCoef) * absInput;
                else
                    _envelope = _envReleaseCoef * _envelope + (1f - _envReleaseCoef) * absInput;

                // 2. Cybernetic Inharmonic Carrier Wave Generation
                // The carrier shifts its frequency dynamically under vocal load (from 58Hz up to 92Hz)
                // to disrupt static structures and simulate logic gate power fluctuations.
                float dynamicCarrierFreq = 58f + (_envelope * 34f);
                _carrierPhase += (TwoPi * dynamicCarrierFreq) / _sampleRate;
                if (_carrierPhase > TwoPi) _carrierPhase -= TwoPi;

                float sineCarrier = (float)Math.Cos(_carrierPhase);
                // Reshape to a jagged, cold pseudo-square wave typical of silicon transistors
                float siliconCarrier = sineCarrier > 0f ? 0.7f : -0.7f;

                // 3. Intermodulate: Multiply raw human voice with the mathematical silicon grid
                float modulatedNode = dry * siliconCarrier;

                // 4. Server Rack Steel Chassis Simulation (Comb Filtering Layer)
                // Read the old reflected sample from the fixed matrix
                float delayedSample = _combBuffer[_combWritePtr];

                // 45% feedback coefficient models high-frequency metal scattering boundaries
                float combResonance = modulatedNode + (delayedSample * 0.45f);

                // Write back the current composite node to the circular lattice
                _combBuffer[_combWritePtr] = combResonance;
                _combWritePtr = (_combWritePtr + 1);
                if (_combWritePtr >= CombDelaySamples) _combWritePtr = 0;

                // 5. Finalize staging mix injection
                float wetOutput = combResonance * 1.2f;
                pcm[i] = (dry * dryMix) + (wetOutput * wetMix);
            }
        }
    }
}