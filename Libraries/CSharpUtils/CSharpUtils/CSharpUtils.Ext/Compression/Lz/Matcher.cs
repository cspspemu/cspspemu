using CSharpUtils.Compression.Lz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpUtils.Ext.Compression.Lz
{
    public class Matcher
    {
        static public void HandleLz(byte[] Input, int StartPosition, int MinLzLength, int MaxLzLength,
            int MaxLzDistance, bool AllowOverlapping, Action<int, byte> ByteCallback, Action<int, int, int> LzCallback)
        {
            HandleLzRle(Input, StartPosition, MinLzLength, MaxLzLength, MaxLzDistance, 0, 0, AllowOverlapping,
                ByteCallback, LzCallback, null);
        }

        static public void HandleLzRle(byte[] Input, int StartPosition, int MinLzLength, int MaxLzLength,
            int MaxLzDistance, int MinRleLength, int MaxRleLength, bool AllowOverlapping,
            Action<int, byte> ByteCallback, Action<int, int, int> LzCallback, Action<int, byte, int> RleCallback)
        {
            bool UseRle = (RleCallback != null) && (MaxRleLength > 0);

            var LzMatcher = new LzMatcher(Input, StartPosition, MaxLzDistance, MinLzLength, MaxLzLength,
                AllowOverlapping);
            var RleMatcher = UseRle ? new RleMatcher(Input, StartPosition) : null;

            for (int n = StartPosition; n < Input.Length;)
            {
                //Console.WriteLine("{0}", n);
                var Result = LzMatcher.FindMaxSequence();

                int RleLength = -1;

                if (UseRle)
                {
                    RleLength = RleMatcher.Length;
                    if (RleLength < MinRleLength) RleLength = 0;
                    if (RleLength > MaxRleLength) RleLength = MaxRleLength;
                }

                if (Result.Found && (!UseRle || (Result.Size > RleLength)))
                {
                    //Console.WriteLine("RLE: {0}", RleLength);
                    LzCallback(n, Result.Offset - n, Result.Size);
                    n += Result.Size;
                    LzMatcher.Skip(Result.Size);
                    //Console.WriteLine(Result.Size);
                    if (UseRle) RleMatcher.Skip(Result.Size);
                    continue;
                }

                if (UseRle && (RleLength >= MinRleLength))
                {
                    RleCallback(n, Input[n], RleLength);
                    n += RleLength;
                    //Console.WriteLine(RleLength);
                    LzMatcher.Skip(RleLength);
                    RleMatcher.Skip(RleLength);
                    continue;
                }

                ByteCallback(n, Input[n]);
                n += 1;
                LzMatcher.Skip(1);
                if (UseRle) RleMatcher.Skip(1);
            }
        }
    }
}