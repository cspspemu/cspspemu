using System.IO;
using System.Diagnostics;

namespace CSharpUtils
{
    /// From DeflateLib
    /// http://www.koders.com/csharp/fid4D158EE81EB65FC84BC0EE0216F6F1F62FC95BD1.aspx?s=mdef%3Acompute
    /// 
    /// <summary>
    /// Helper class for writing bit values, bit blocks in LSB and MSB bits formats.
    /// </summary>
    public class BitWriter
    {
        uint data = 0;
        int dataLength = 0;
        Stream stream;

        internal Stream BaseStream
        {
            get { return stream; }
        }

        internal int BitsToAligment
        {
            get { return (32 - dataLength) % 8; }
        }

        public BitWriter(Stream stream)
        {
            this.stream = stream;
        }

        public void WriteBit(bool value)
        {
            WriteLSB(value ? 1 : 0, 1);
        }

        public void WriteLSB(int value, int length)
        {
            Debug.Assert(value < 1 << length, "value does not fit in length");

            uint currentData = data | checked((uint) value << dataLength);
            int currentLength = dataLength + length;
            while (currentLength >= 8)
            {
                BaseStream.WriteByte((byte) currentData);
                currentData >>= 8;
                currentLength -= 8;
            }
            data = currentData;
            dataLength = currentLength;
        }

        public void WriteMSB(int value, int length)
        {
            Debug.Assert(value < 1 << length, "value does not fit in length");

            int reversed = 0;
            for (int i = length - 1; i >= 0; i--)
            {
                reversed <<= 1;
                reversed |= value & 1;
                value >>= 1;
            }
            WriteLSB(reversed, length);
        }

        public void Align()
        {
            if (dataLength > 0)
            {
                BaseStream.WriteByte((byte) data);

                data = 0;
                dataLength = 0;
            }
        }
    }
}