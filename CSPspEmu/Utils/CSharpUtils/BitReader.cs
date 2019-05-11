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
        uint _readData;
        int _startPosition;
        int _endPosition;

        internal int InBuffer => _endPosition - _startPosition;

        private Stream stream;

        internal Stream BaseStream => stream;

        internal BitReader(Stream stream)
        {
            this.stream = stream;
        }

        void EnsureData(int bitCount)
        {
            var readBits = bitCount - InBuffer;
            while (readBits > 0)
            {
                var b = BaseStream.ReadByte();

                if (b < 0) throw new Exception("Unexpected end of stream");

                _readData |= checked((uint) b << _endPosition);
                _endPosition += 8;
                readBits -= 8;
            }
        }

        internal bool ReadBit()
        {
            return ReadLsb(1) > 0;
        }

        internal int ReadLsb(int bitCount)
        {
            EnsureData(bitCount);

            var result = (int) (_readData >> _startPosition) & ((1 << bitCount) - 1);
            _startPosition += bitCount;
            if (_endPosition == _startPosition)
            {
                _endPosition = _startPosition = 0;
                _readData = 0;
            }
            else if (_startPosition >= 8)
            {
                _readData >>= _startPosition;
                _endPosition -= _startPosition;
                _startPosition = 0;
            }

            return result;
        }

        internal int ReadMsb(int bitCount)
        {
            var result = 0;
            for (var i = 0; i < bitCount; i++)
            {
                result <<= 1;
                if (ReadBit()) result |= 1;
            }

            return result;
        }

        internal void Align()
        {
            _endPosition = _startPosition = 0;
            _readData = 0;
        }
    }
}