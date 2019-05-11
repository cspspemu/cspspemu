using System;
using System.Runtime;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        // http://www.lambda-computing.com/publications/articles/generics2/
        // http://www.codeproject.com/KB/cs/genericoperators.aspx
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static T Clamp<T>(T value, T min, T max) where T : IComparable
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float start, float end, float percent)
        {
            return (start + percent * (end - start));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge0"></param>
        /// <param name="edge1"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static float SmoothStep(float edge0, float edge1, float x)
        {
            var t = Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
            return t * t * (3.0f - 2.0f * t);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void NormalizeMax(ref float[] items)
        {
            var max = Max(items);
            for (var n = 0; n < items.Length; n++) items[n] /= max;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void NormalizeMax(ref float a, ref float b)
        {
            var div = Math.Max(a, b);
            a /= div;
            b /= div;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void NormalizeSum(ref float a, ref float b)
        {
            var div = a + b;
            a /= div;
            b /= div;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FastClamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastClamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static byte FastClampToByte(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte) value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T a, ref T b)
        {
            LanguageUtils.Swap(ref a, ref b);
        }

        /// <summary>
        /// Useful for converting LittleEndian to BigEndian and viceversa.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static ushort ByteSwap(ushort value)
        {
            return (ushort) ((value >> 8) | (value << 8));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint ByteSwap(uint value)
        {
            return (
                ((uint) ByteSwap((ushort) (value >> 0)) << 16) |
                ((uint) ByteSwap((ushort) (value >> 16)) << 0)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static ulong ByteSwap(ulong value)
        {
            return (
                ((ulong) ByteSwap((uint) (value >> 0)) << 32) |
                ((ulong) ByteSwap((uint) (value >> 32)) << 0)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static unsafe float ByteSwap(float value)
        {
            var valueSw = ByteSwap(*(uint*) &value);
            return *(float*) &valueSw;
        }

        /// <summary>
        /// Returns the upper minimum value that will be divisible by AlignValue.
        /// </summary>
        /// <example>
        /// Align(0x1200, 0x800) == 0x1800
        /// </example>
        /// <param name="value"></param>
        /// <param name="alignValue"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static long Align(long value, long alignValue)
        {
            if ((value % alignValue) != 0)
            {
                value += alignValue - (value % alignValue);
            }
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long RequiredBlocks(long size, long blockSize)
        {
            if ((size % blockSize) != 0)
            {
                return (size / blockSize) + 1;
            }
            else
            {
                return size / blockSize;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="alignment"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PrevAligned(uint value, int alignment)
        {
            if ((value % alignment) != 0)
            {
                value -= (uint) ((value % alignment));
            }
            return value;
        }

        /*
        // NOT WORKING!
        public static uint NextAligned2(uint Value, uint Alignment)
        {
            return (Value + Alignment) & ~Alignment;
        }

        public static long NextAligned2(long Value, long Alignment)
        {
            return (Value + Alignment) & ~Alignment;
        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="alignment"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint NextAligned(uint value, int alignment)
        {
            return (uint) NextAligned((long) value, alignment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="alignment"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long NextAligned(long value, long alignment)
        {
            if (alignment != 0 && (value % alignment) != 0)
            {
                value += alignment - (value % alignment);
            }
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseValue"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextPowerOfTwo(int baseValue)
        {
            var nextPowerOfTwoValue = 1;
            while (nextPowerOfTwoValue < baseValue) nextPowerOfTwoValue <<= 1;
            return nextPowerOfTwoValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(params float[] items)
        {
            var maxValue = items[0];
            foreach (var item in items) if (maxValue < item) maxValue = item;
            return maxValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(params int[] items)
        {
            var maxValue = items[0];
            foreach (var item in items) if (maxValue < item) maxValue = item;
            return maxValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Max(params uint[] items)
        {
            var maxValue = items[0];
            foreach (var item in items) if (maxValue < item) maxValue = item;
            return maxValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint NumberOfSetBits(uint i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool IsPowerOfTwo(uint value)
        {
            return (value & (value - 1)) == 0;
            //return NumberOfSetBits(Value) == 1;
        }
    }
}