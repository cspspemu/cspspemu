using System;
using System.Collections.Generic;

namespace CSPspEmu.Hle.Formats.Font
{
    public class BitReader : IDisposable
    {
        private byte[] _data;
        private int _bitOffset;
        private int _byteOffset;

        public BitReader(byte[] data) => _data = data;

        public void Reset()
        {
            _byteOffset = 0;
            _bitOffset = 0;
        }

        public int Position
        {
            get => _byteOffset * 8 + _bitOffset;
            set
            {
                _bitOffset = value % 8;
                _byteOffset = value / 8;
            }
            //Console.WriteLine("bit: {0}, byte: {1}", this.BitOffset, this.ByteOffset);
        }

        public int BitsLeft => ((_data.Length - _byteOffset) - 1) * 8 + (8 - _bitOffset);

        public uint ReadBits(int count)
        {
            var value = 0;
            var readOffset = 0;

            while (count > 0)
            {
                int leftInByte = 8 - _bitOffset;
                int readCount = Math.Min(count, leftInByte);
                //Console.WriteLine("Byte[{0}] = {1}", ByteOffset, Data[ByteOffset]);
                value |= ((_data[_byteOffset] >> _bitOffset) & ((1 << readCount) - 1)) << readOffset;

                readOffset += readCount;
                _bitOffset += readCount;
                if (_bitOffset == 8)
                {
                    _bitOffset = 0;
                    _byteOffset++;
                }
                count -= readCount;
            }

            return (uint) value;
        }

        public void SkipBits(int count)
        {
            _bitOffset += count % 8;
            _byteOffset += count / 8;
        }

        public int ReadBitsSigned(int count)
        {
            int value = (int) ReadBits(count);
            if ((value & (1 << count)) != 0)
            {
                value |= ~((1 << count) - 1);
            }
            return value;
        }


        public static uint ReadBitsAt(byte[] data, int offset, int count)
        {
            var bitReader = new BitReader(data) {Position = offset};
            return bitReader.ReadBits(count);
        }

        public static IEnumerable<KeyValuePair<uint, uint>> FixedBitReader(byte[] data, int bitCount = 0,
            int offset = 0)
        {
            using (var bitReader = new BitReader(data))
            {
                bitReader.Position = offset;

                uint index = 0;
                while (bitReader.BitsLeft >= bitCount)
                {
                    yield return new KeyValuePair<uint, uint>(index++, bitReader.ReadBits(bitCount));
                }
            }
        }

        public void Dispose()
        {
            _data = null;
        }
    }
}