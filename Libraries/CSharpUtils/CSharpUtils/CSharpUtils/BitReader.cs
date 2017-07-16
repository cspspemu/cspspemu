using System;
using System.IO;

namespace CSharpUtils
{
    /// From DeflateLib
    /// http://www.koders.com/csharp/fid4D158EE81EB65FC84BC0EE0216F6F1F62FC95BD1.aspx?s=mdef%3Acompute
    /// 
    /// <summary>
    /// Helper class for writing bit values, bit blocks in LSB and MSB bits formats.
    /// Doesn't read bytes ahead -- reads next byte if necessary.
    /// </summary>
    public class BitReader
    {
        uint readData = 0;
        int startPosition = 0;
        int endPosition = 0;

        internal int InBuffer
        {
            get { return endPosition - startPosition; }
        }

        private Stream stream;

        internal Stream BaseStream
        {
            get { return stream; }
        }

        internal BitReader(Stream stream)
        {
            this.stream = stream;
        }

        void EnsureData(int bitCount)
        {
            int readBits = bitCount - InBuffer;
            while (readBits > 0)
            {
                int b = BaseStream.ReadByte();

                if (b < 0) throw new Exception("Unexpected end of stream");

                readData |= checked((uint) b << endPosition);
                endPosition += 8;
                readBits -= 8;
            }
        }

        internal bool ReadBit()
        {
            return ReadLSB(1) > 0;
        }

        internal int ReadLSB(int bitCount)
        {
            EnsureData(bitCount);

            int result = (int) (readData >> startPosition) & ((1 << bitCount) - 1);
            startPosition += bitCount;
            if (endPosition == startPosition)
            {
                endPosition = startPosition = 0;
                readData = 0;
            }
            else if (startPosition >= 8)
            {
                readData >>= startPosition;
                endPosition -= startPosition;
                startPosition = 0;
            }

            return result;
        }

        internal int ReadMSB(int bitCount)
        {
            int result = 0;
            for (int i = 0; i < bitCount; i++)
            {
                result <<= 1;
                if (ReadBit()) result |= 1;
            }

            return result;
        }

        internal void Align()
        {
            endPosition = startPosition = 0;
            readData = 0;
        }
    }
}