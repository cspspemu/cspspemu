namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class IntegerExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte RotateLeft(this byte value, int count) => (byte) ((value << count) | (value >> (8 - count)));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte RotateRight(this byte value, int count) =>
            (byte) ((value >> count) | (value << (8 - count)));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static uint RotateLeft(this uint value, int count) => (value << count) | (value >> (32 - count));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static uint RotateRight(this uint value, int count) => (value >> count) | (value << (32 - count));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static ulong RotateLeft(this ulong value, int count) => (value << count) | (value >> (64 - count));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static ulong RotateRight(this ulong value, int count) => (value >> count) | (value << (64 - count));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repeatedValue"></param>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] Repeat<T>(this T repeatedValue, int count)
        {
            var list = new T[count];
            for (var n = 0; n < count; n++) list[n] = repeatedValue;
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static uint ExtractUnsigned(this ushort value, int offset, int count)
        {
            var mask = (1 << count) - 1;
            return (uint) ((value >> offset) & mask);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static uint ExtractUnsignedScale(this ushort value, int offset, int count, int scale)
        {
            var mask = (1 << count) - 1;
            return (uint) ((value.ExtractUnsigned(offset, count) * scale) / mask);
        }
    }
}