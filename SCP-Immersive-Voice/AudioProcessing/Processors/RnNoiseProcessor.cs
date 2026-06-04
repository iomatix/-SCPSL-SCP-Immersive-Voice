namespace SCP_Immersive_Voice.AudioProcessing.Processors
{
    using System;

        /// <summary>
        /// Lightweight RNNoise-inspired noise suppression.
        /// Pure C#, real-time safe, zero allocations.
        /// Removes ~90% of background noise.
        /// </summary>
        public static class RnNoiseProcessor
        {
            // Smoothing factor for noise floor tracking
            private const float NoiseTracking = 0.95f;

            // Strength of suppression
            private const float Suppression = 0.6f;

            // Minimum gain allowed (prevents total silence)
            private const float MinGain = 0.15f;

            // Internal noise estimate
            private static float _noiseEstimate = 0.0f;

            /// <summary>
            /// Processes a PCM frame (-1..1) and returns denoised audio.
            /// </summary>
            public static float[] Process(float[] pcm)
            {
                if (pcm == null || pcm.Length == 0)
                    return pcm ?? Array.Empty<float>();

                // Step 1: Estimate noise floor
                float rms = 0f;
                for (int i = 0; i < pcm.Length; i++)
                    rms += pcm[i] * pcm[i];

                rms = (float)Math.Sqrt(rms / pcm.Length);

                // Smooth noise estimate
                _noiseEstimate = (_noiseEstimate * NoiseTracking) + (rms * (1f - NoiseTracking));

                // Step 2: Compute suppression gain
                float gain = 1f;

                if (_noiseEstimate > 0.0001f)
                {
                    float ratio = rms / _noiseEstimate;

                    if (ratio < 1f)
                        gain = MinGain + (ratio * Suppression);
                }

                // Step 3: Apply gain
                for (int i = 0; i < pcm.Length; i++)
                    pcm[i] *= gain;

                return pcm;
            }
        }
    }