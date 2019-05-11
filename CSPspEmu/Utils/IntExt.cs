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
        
        public static int Interpolate(this double ratio, int min, int max)
        {
            return (int) (min + (max - min) * ratio);
        }
    }
}