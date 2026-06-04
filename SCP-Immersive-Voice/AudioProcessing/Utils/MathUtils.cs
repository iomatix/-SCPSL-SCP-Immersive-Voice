namespace SCP_Immersive_Voice.AudioProcessing.Utils
{
    public static class MathUtils
    {
        public static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }
        public static float Lerp(float a, float b, float t)
        {
            // No Clamp(t) — caller controls range for performance reasons.
            return a + (b - a) * t;
        }
    }
}