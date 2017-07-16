using System;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace CSharpUtils
{
    public static class MathUtils
    {
        // http://www.lambda-computing.com/publications/articles/generics2/
        // http://www.codeproject.com/KB/cs/genericoperators.aspx
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static T Clamp<T>(T Value, T Min, T Max) where T : IComparable
        {
            if (Value.CompareTo(Min) < 0) return Min;
            if (Value.CompareTo(Max) > 0) return Max;
            return Value;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static float Lerp(float start, float end, float percent)
        {
            return (start + percent * (end - start));
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static float SmoothStep(float edge0, float edge1, float x)
        {
            var t = Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
            return t * t * (3.0f - 2.0f * t);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void NormalizeMax(ref float[] Items)
        {
            var Max = MathUtils.Max(Items);
            for (int n = 0; n < Items.Length; n++) Items[n] /= Max;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void NormalizeMax(ref float a, ref float b)
        {
            var Div = Math.Max(a, b);
            a /= Div;
            b /= Div;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void NormalizeSum(ref float a, ref float b)
        {
            var Div = a + b;
            a /= Div;
            b /= Div;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static float FastClamp(float Value, float Min, float Max)
        {
            if (Value < Min) return Min;
            if (Value > Max) return Max;
            return Value;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int FastClamp(int Value, int Min, int Max)
        {
            if (Value < Min) return Min;
            if (Value > Max) return Max;
            return Value;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static byte FastClampToByte(int Value)
        {
            if (Value < 0) return 0;
            if (Value > 255) return 255;
            return (byte) Value;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Swap<Type>(ref Type A, ref Type B)
        {
            LanguageUtils.Swap(ref A, ref B);
        }

        /// <summary>
        /// Useful for converting LittleEndian to BigEndian and viceversa.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static ushort ByteSwap(ushort Value)
        {
            return (ushort) ((Value >> 8) | (Value << 8));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint ByteSwap(uint Value)
        {
            return (
                ((uint) ByteSwap((ushort) (Value >> 0)) << 16) |
                ((uint) ByteSwap((ushort) (Value >> 16)) << 0)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static ulong ByteSwap(ulong Value)
        {
            return (
                ((ulong) ByteSwap((uint) (Value >> 0)) << 32) |
                ((ulong) ByteSwap((uint) (Value >> 32)) << 0)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        unsafe public static float ByteSwap(float Value)
        {
            var ValueSW = ByteSwap(*(uint*) &Value);
            return *(float*) &ValueSW;
        }

        /// <summary>
        /// Returns the upper minimum value that will be divisible by AlignValue.
        /// </summary>
        /// <example>
        /// Align(0x1200, 0x800) == 0x1800
        /// </example>
        /// <param name="Value"></param>
        /// <param name="AlignValue"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static long Align(long Value, long AlignValue)
        {
            if ((Value % AlignValue) != 0)
            {
                Value += (AlignValue - (Value % AlignValue));
            }
            return Value;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static long RequiredBlocks(long Size, long BlockSize)
        {
            if ((Size % BlockSize) != 0)
            {
                return (Size / BlockSize) + 1;
            }
            else
            {
                return Size / BlockSize;
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint PrevAligned(uint Value, int Alignment)
        {
            if ((Value % Alignment) != 0)
            {
                Value -= (uint) ((Value % Alignment));
            }
            return Value;
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

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint NextAligned(uint Value, int Alignment)
        {
            return (uint) NextAligned((long) Value, (long) Alignment);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static long NextAligned(long Value, long Alignment)
        {
            if (Alignment != 0)
            {
                if ((Value % Alignment) != 0)
                {
                    Value += (long) (Alignment - (Value % Alignment));
                }
            }
            return Value;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int NextPowerOfTwo(int BaseValue)
        {
            int NextPowerOfTwoValue = 1;
            while (NextPowerOfTwoValue < BaseValue) NextPowerOfTwoValue <<= 1;
            return NextPowerOfTwoValue;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static float Max(params float[] Items)
        {
            var MaxValue = Items[0];
            foreach (var Item in Items) if (MaxValue < Item) MaxValue = Item;
            return MaxValue;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Max(params int[] Items)
        {
            var MaxValue = Items[0];
            foreach (var Item in Items) if (MaxValue < Item) MaxValue = Item;
            return MaxValue;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint Max(params uint[] Items)
        {
            var MaxValue = Items[0];
            foreach (var Item in Items) if (MaxValue < Item) MaxValue = Item;
            return MaxValue;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint NumberOfSetBits(uint i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool IsPowerOfTwo(uint Value)
        {
            return (Value & (Value - 1)) == 0;
            //return NumberOfSetBits(Value) == 1;
        }
    }
}