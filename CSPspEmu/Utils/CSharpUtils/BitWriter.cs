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
        uint _data;
        int _dataLength;
        Stream stream;

        internal Stream BaseStream => stream;
        internal int BitsToAligment => (32 - _dataLength) % 8;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public BitWriter(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteBit(bool value)
        {
            WriteLsb(value ? 1 : 0, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        public void WriteLsb(int value, int length)
        {
            Debug.Assert(value < 1 << length, "value does not fit in length");

            var currentData = _data | checked((uint) value << _dataLength);
            var currentLength = _dataLength + length;
            while (currentLength >= 8)
            {
                BaseStream.WriteByte((byte) currentData);
                currentData >>= 8;
                currentLength -= 8;
            }
            _data = currentData;
            _dataLength = currentLength;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        public void WriteMsb(int value, int length)
        {
            Debug.Assert(value < 1 << length, "value does not fit in length");

            var reversed = 0;
            for (var i = length - 1; i >= 0; i--)
            {
                reversed <<= 1;
                reversed |= value & 1;
                value >>= 1;
            }
            WriteLsb(reversed, length);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Align()
        {
            if (_dataLength > 0)
            {
                BaseStream.WriteByte((byte) _data);

                _data = 0;
                _dataLength = 0;
            }
        }
    }
}