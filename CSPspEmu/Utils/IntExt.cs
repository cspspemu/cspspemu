namespace CSPspEmu.Utils
{
    static public class IntExt
    {
        
        static public int Clamp(this int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        static public float Clamp(this float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Interpolate(this double ratio, int min, int max) => (int) (min + (max - min) * ratio);
        public static int Interpolate(this float ratio, int min, int max) => (int) (min + (max - min) * ratio);

        public static float RatioInRange(this int value, int min, int max) => (float) (value - min) / (float) (max - min);
    }
}