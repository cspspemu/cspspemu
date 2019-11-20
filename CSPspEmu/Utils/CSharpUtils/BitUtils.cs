using System;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="http://blogs.microsoft.co.il/blogs/sasha/archive/2012/01/20/aggressive-inlining-in-the-clr-4-5-jit.aspx"/>
    public static class BitUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        
        
        //public static uint CreateMask(int size) => (size == 0) ? 0 : (uint) ((1 << size) - 1);
        public static uint CreateMask(this int size) => (uint) ((1 << size) - 1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"> </param>
        /// <param name="count"> </param>
        /// <param name="valueToInsert"> </param>
        /// <returns></returns>
        
        
        public static void Insert(ref uint value, int offset, int count, uint valueToInsert) =>
            value = Insert(value, offset, count, valueToInsert);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialValue"> </param>
        /// <param name="offset"> </param>
        /// <param name="count"> </param>
        /// <param name="valueToInsert"> </param>
        /// <returns></returns>
        
        
        public static uint Insert(this uint initialValue, int offset, int count, uint valueToInsert) =>
            InsertWithMask(initialValue, offset, CreateMask(count), valueToInsert);

        /// <summary>
        /// </summary>
        /// <param name="initialValue"> </param>
        /// <param name="offset"> </param>
        /// <param name="count"> </param>
        /// <param name="valueToInsert"> </param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        
        
        public static void
            InsertScaled(ref uint initialValue, int offset, int count, uint valueToInsert, uint maxValue) =>
            initialValue = InsertScaled(initialValue, offset, count, valueToInsert, maxValue);

        /// <summary>
        /// </summary>
        /// <param name="initialValue"> </param>
        /// <param name="offset"> </param>
        /// <param name="count"> </param>
        /// <param name="valueToInsert"> </param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        
        
        public static uint InsertScaled(this uint initialValue, int offset, int count, uint valueToInsert, uint maxValue) =>
            InsertWithMask(initialValue, offset, CreateMask(count),
                valueToInsert * CreateMask(count) / maxValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialValue"></param>
        /// <param name="offset"></param>
        /// <param name="mask"></param>
        /// <param name="valueToInsert"></param>
        /// <returns></returns>
        
        
        private static uint InsertWithMask(this uint initialValue, int offset, uint mask, uint valueToInsert) =>
            (initialValue & ~(mask << offset)) | ((valueToInsert & mask) << offset);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialValue"> </param>
        /// <param name="offset"> </param>
        /// <param name="count"> </param>
        /// <returns></returns>
        
        
        public static uint Extract(this uint initialValue, int offset, int count) =>
            (initialValue >> offset) & CreateMask(count);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialValue"> </param>
        /// <param name="offset"> </param>
        /// <param name="count"> </param>
        /// <param name="scale"> </param>
        /// <returns></returns>
        
        
        public static uint ExtractScaled(this uint initialValue, int offset, int count, int scale) =>
            (uint) (Extract(initialValue, offset, count) * scale / CreateMask(count));

        public static uint ExtractScaled(this ushort initialValue, int offset, int count, int scale) =>
            ExtractScaled((uint) initialValue, offset, count, scale);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialValue"> </param>
        /// <param name="offset"> </param>
        /// <returns></returns>
        
        
        public static bool ExtractBool(this uint initialValue, int offset) => Extract(initialValue, offset, 1) != 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialValue"> </param>
        /// <param name="offset"> </param>
        /// <param name="count"> </param>
        /// <returns></returns>
        
        
        public static int ExtractSigned(this uint initialValue, int offset, int count)
        {
            var mask = CreateMask(count);
            var signBit = (uint) (1 << (offset + (count - 1)));
            var value = (initialValue >> offset) & mask;
            if ((value & signBit) != 0)
            {
                value |= ~mask;
            }
            return (int) value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="offset"> </param>
        /// <param name="count"> </param>
        /// <param name="scale"> </param>
        /// <returns></returns>
        
        public static float ExtractUnsignedScaled(this uint value, int offset, int count, float scale = 1.0f)
        {
            return (float) Extract(value, offset, count) / CreateMask(count) * scale;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrays"></param>
        /// <returns></returns>
        public static byte[] XorBytes(params byte[][] arrays)
        {
            var length = arrays[0].Length;
            foreach (var array in arrays)
                if (array.Length != length) throw new InvalidOperationException("Arrays sizes must match");
            var bytes = new byte[length];
            foreach (var array in arrays)
            {
                for (var n = 0; n < length; n++) bytes[n] ^= array[n];
            }
            return bytes;
        }

        static readonly int[] MultiplyDeBruijnBitPosition =
        {
            0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
            31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        
        public static int GetFirstBit1(this uint v) => MultiplyDeBruijnBitPosition[(uint) ((v & -v) * 0x077CB531U) >> 27];
    }
}