using System;
using System.IO;
using CSharpUtils.Extensions;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class BitWriter2
    {
        /// <summary>
        /// 
        /// </summary>
        public enum Direction
        {
        }

        Stream Stream;
        uint _currentValue;
        readonly int _byteCapacity;

        int BitsCapacity => _byteCapacity * 8;

        int _leftBits;

        int CurrentBits => BitsCapacity - _leftBits;

        bool LSB;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="byteCapacity"></param>
        /// <param name="lsb"></param>
        public BitWriter2(Stream stream, int byteCapacity = 1, bool lsb = false)
        {
            Stream = stream;
            _byteCapacity = byteCapacity;
            LSB = lsb;
            ResetValue();
        }

        private void ResetValue()
        {
            _currentValue = 0;
            _leftBits = BitsCapacity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public BitWriter2 WriteBits(int count, int value)
        {
            WriteBits(count, (uint) value);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public BitWriter2 WriteBits(int count, uint value)
        {
            if (count > _leftBits)
            {
                var bits1 = _leftBits;
                var bits2 = count - _leftBits;
                WriteBits(bits1, value >> (count - bits1));
                WriteBits(bits2, value);
                return this;
            }

            if (LSB)
            {
                _currentValue |= (value & BitUtils.CreateMask(count)) << (CurrentBits);
            }
            else
            {
                _currentValue |= (value & BitUtils.CreateMask(count)) << (_leftBits - count);
            }
            _leftBits -= count;

            if (_leftBits != 0) return this;
            
            //Console.WriteLine("Writting: {0:X" + (ByteCapacity * 2) + "}", CurrentValue);
            switch (_byteCapacity)
            {
                case 1:
                    Stream.WriteByte((byte) _currentValue);
                    break;
                case 2:
                    Stream.WriteStruct((ushort) _currentValue);
                    break;
                case 4:
                    // ReSharper disable once RedundantCast
                    Stream.WriteStruct((uint) _currentValue);
                    break;
                default:
                    throw(new InvalidOperationException());
            }
            ResetValue();

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Align()
        {
            if (CurrentBits > 0)
            {
                WriteBits(_leftBits, 0);
            }
        }
    }
}