using System;

namespace CSharpUtils.Ext.Compression.Lz
{
    /// <summary>
    /// 
    /// </summary>
    public class Matcher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="startPosition"></param>
        /// <param name="minLzLength"></param>
        /// <param name="maxLzLength"></param>
        /// <param name="maxLzDistance"></param>
        /// <param name="allowOverlapping"></param>
        /// <param name="byteCallback"></param>
        /// <param name="lzCallback"></param>
        public static void HandleLz(byte[] input, int startPosition, int minLzLength, int maxLzLength,
            int maxLzDistance, bool allowOverlapping, Action<int, byte> byteCallback, Action<int, int, int> lzCallback)
        {
            HandleLzRle(input, startPosition, minLzLength, maxLzLength, maxLzDistance, 0, 0, allowOverlapping,
                byteCallback, lzCallback, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="startPosition"></param>
        /// <param name="minLzLength"></param>
        /// <param name="maxLzLength"></param>
        /// <param name="maxLzDistance"></param>
        /// <param name="minRleLength"></param>
        /// <param name="maxRleLength"></param>
        /// <param name="allowOverlapping"></param>
        /// <param name="byteCallback"></param>
        /// <param name="lzCallback"></param>
        /// <param name="rleCallback"></param>
        public static void HandleLzRle(byte[] input, int startPosition, int minLzLength, int maxLzLength,
            int maxLzDistance, int minRleLength, int maxRleLength, bool allowOverlapping,
            Action<int, byte> byteCallback, Action<int, int, int> lzCallback, Action<int, byte, int> rleCallback)
        {
            var useRle = (rleCallback != null) && (maxRleLength > 0);

            var lzMatcher = new LzMatcher(input, startPosition, maxLzDistance, minLzLength, maxLzLength,
                allowOverlapping);
            var rleMatcher = useRle ? new RleMatcher(input, startPosition) : null;

            for (var n = startPosition; n < input.Length;)
            {
                //Console.WriteLine("{0}", n);
                var result = lzMatcher.FindMaxSequence();

                var rleLength = -1;

                if (useRle)
                {
                    rleLength = rleMatcher.Length;
                    if (rleLength < minRleLength) rleLength = 0;
                    if (rleLength > maxRleLength) rleLength = maxRleLength;
                }

                if (result.Found && (!useRle || (result.Size > rleLength)))
                {
                    //Console.WriteLine("RLE: {0}", RleLength);
                    lzCallback(n, result.Offset - n, result.Size);
                    n += result.Size;
                    lzMatcher.Skip(result.Size);
                    //Console.WriteLine(Result.Size);
                    if (useRle) rleMatcher.Skip(result.Size);
                    continue;
                }

                if (useRle && (rleLength >= minRleLength))
                {
                    rleCallback(n, input[n], rleLength);
                    n += rleLength;
                    //Console.WriteLine(RleLength);
                    lzMatcher.Skip(rleLength);
                    rleMatcher.Skip(rleLength);
                    continue;
                }

                byteCallback(n, input[n]);
                n += 1;
                lzMatcher.Skip();
                if (useRle) rleMatcher.Skip();
            }
        }
    }
}