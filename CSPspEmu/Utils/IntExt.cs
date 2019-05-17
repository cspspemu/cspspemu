using System.Runtime.CompilerServices;

namespace CSPspEmu.Utils
{
    public static class IntExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(this int value, int min, int max) => value < min ? min : value > max ? max : value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(this float value, float min, float max) => value < min ? min : value > max ? max : value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Interpolate(this double ratio, int min, int max) => (int) (min + (max - min) * ratio);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Interpolate(this float ratio, int min, int max) => (int) (min + (max - min) * ratio);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RatioInRange(this int value, int min, int max) => (float) (value - min) / (float) (max - min);

        public static int RangeConvert(this int value, int minSrc, int maxSrc, int minDst, int maxDst)
        {
            var srcLen = (maxSrc - minSrc);
            var dstLen = (maxDst - minDst);
            return minDst + ((value - minSrc) * dstLen) / srcLen;
        }

    }
}