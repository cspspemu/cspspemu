using System.Numerics;
using System.Runtime.CompilerServices;

namespace CSPspEmu.Utils
{
    public static class IntExt
    {
        public static int Clamp(this int value, int min, int max) => value < min ? min : value > max ? max : value;

        public static float Clamp(this float value, float min, float max) =>
            value < min ? min : value > max ? max : value;

        public static Vector2 Clamp(this Vector2 value, float min, float max) =>
            new Vector2(value.X.Clamp(min, max), value.Y.Clamp(min, max));

        public static Vector3 Clamp(this Vector3 value, float min, float max) => new Vector3(value.X.Clamp(min, max),
            value.Y.Clamp(min, max), value.Z.Clamp(min, max));

        public static Vector4 Clamp(this Vector4 value, float min, float max) => new Vector4(value.X.Clamp(min, max),
            value.Y.Clamp(min, max), value.Z.Clamp(min, max), value.W.Clamp(min, max));

        public static int Interpolate(this double ratio, int min, int max) => (int) (min + (max - min) * ratio);
        public static int Interpolate(this float ratio, int min, int max) => (int) (min + (max - min) * ratio);
        public static float RatioInRange(this int value, int min, int max) => (value - min) / (float) (max - min);

        public static int RangeConvert(this int value, int minSrc, int maxSrc, int minDst, int maxDst)
        {
            var srcLen = maxSrc - minSrc;
            var dstLen = maxDst - minDst;
            return minDst + (value - minSrc) * dstLen / srcLen;
        }
    }
}