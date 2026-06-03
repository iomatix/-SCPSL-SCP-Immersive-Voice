namespace SCP_Immersive_Voice.AudioProcessing.Effects
{
    using SCP_Immersive_Voice.AudioProcessing.Interfaces;
    using System;

    /// <summary>
    /// Generates deep subharmonic growl by adding a half‑frequency oscillator 
    /// locked to the input amplitude. Ideal for zombie voices, monstrous growls, 
    /// SCP‑049‑2, SCP‑939, or any creature requiring a deep, guttural undertone.
    /// Includes amplitude tracking, organic modulation, smoothing, and saturation 
    /// for a natural, AAA‑quality monster vocal texture.
    /// </summary>
    public class SubharmonicGrowlEffect : IAudioEffectShort
    {
        private readonly float _amount;

        // Oscillator phase
        private float _phase;

        // Envelope follower for amplitude tracking
        private float _env;

        // Smoothing for natural growl movement
        private float _smooth;

        public SubharmonicGrowlEffect(float amount)
        {
            // amount 0 → no growl
            // amount 1.5 → extremely deep monster growl
            _amount = Clamp(amount, 0f, 1.5f);
        }

        public void Process(short[] pcm, int length)
        {
            for (int i = 0; i < length; i++)
            {
                // Convert PCM to float -1..1
                float x = pcm[i] / 32768f;

                // 1. Envelope follower (how loud the voice is)
                _env += 0.05f * (Math.Abs(x) - _env);

                // 2. Subharmonic oscillator (half frequency)
                _phase += 0.015f + _env * 0.02f; // louder → stronger growl movement
                float osc = (float)Math.Sin(_phase * 0.5f);

                // 3. Organic modulation (creature throat instability)
                float mod = 0.8f + 0.2f * (float)Math.Sin(_phase * 0.13f);

                // 4. Combine oscillator + modulation + amplitude
                float growl = osc * mod * _env * (_amount * 0.6f);

                // 5. Smooth for natural throat resonance
                _smooth += 0.2f * (growl - _smooth);

                // 6. Soft saturation for guttural character
                float saturated = (float)Math.Tanh(_smooth * 2.5f);

                // 7. Mix with original signal
                float mixed = x + saturated;

                // Convert back to PCM
                int sample = (int)(mixed * 32767f);

                // Clamp
                if (sample > short.MaxValue) sample = short.MaxValue;
                if (sample < short.MinValue) sample = short.MinValue;

                pcm[i] = (short)sample;
            }
        }

        private static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }
    }
}
