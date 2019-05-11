//#define BIT_READER_THREAD_SAFE
//#define DEBUG_BIT_READER

using System;

namespace CSPspEmu.Hle.Media.audio.At3.SUB
{
    public sealed unsafe class MaiBitReader : IDisposable
    {
        public const int MaiBitReaderTypeHigh = 0;
        public const int MaiBitReaderTypeLow = 1;

        MaiQueue0 quene_in;
        int _buffer;
        int _bitsNum;
        int type;

        public MaiBitReader(int byteBufSize, int type = MaiBitReaderTypeHigh)
        {
            quene_in = new MaiQueue0(byteBufSize);
            _buffer = 0;
            _bitsNum = 0;
            this.type = type;
        }

        public void Dispose()
        {
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
            {
                quene_in.Dis();
            }
        }

        public bool MoreByte()
        {
            if (quene_in.GetLength() != 0)
            {
                var temp = new byte[1];
                quene_in.Out(temp, 1);
                if (type == MaiBitReaderTypeHigh)
                {
                    _buffer = (_buffer << 8) | (temp[0] & 0xFF);
                }
                else if (type == MaiBitReaderTypeLow)
                {
                    _buffer |= (temp[0] & 0xFF) << _bitsNum;
                }
                _bitsNum += 8;
                return false;
            }
            else
            {
                return true;
            }
        }

        public int AddData(byte* src, int lenS)
        {
            int ret = _addData(src, lenS);
#if DEBUG_BIT_READER
			Console.WriteLine("addData({0}) : {1}", len_s, BitConverter.ToString(src.GetArray(len_s)));
#endif
            return ret;
        }

        private int _addData(byte* src, int lenS)
        {
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
            {
                quene_in.In(src, lenS);
            }

            return 0;
        }

        public int GetRemainingBitsNum()
        {
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
            {
                int bitsRemain = (quene_in.GetLength() << 3) + _bitsNum;

                return bitsRemain;
            }
        }

        ////[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWithI32Buffer(int bnum, bool getThenDelInBuf = true)
        {
            var value = _getWithI32Buffer(bnum, getThenDelInBuf);
#if DEBUG_BIT_READER
			Console.WriteLine("MaiBitReader.getWithI32Buffer({0}) : {1}", bnum, value);
#endif
            return value;
        }

        ////[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _getWithI32Buffer(int bnum, bool getThenDelInBuf = true)
        {
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
            {
                while (bnum > _bitsNum)
                {
                    if (MoreByte()) break;
                }

                if (bnum <= _bitsNum)
                {
                    int toOut = 0;

                    if (type == MaiBitReaderTypeHigh)
                    {
                        toOut = (_buffer >> (_bitsNum - bnum)) & ((1 << bnum) - 1);
                    }
                    else if (type == MaiBitReaderTypeLow)
                    {
                        toOut = _buffer & ((1 << bnum) - 1);
                    }

                    if (getThenDelInBuf)
                    {
                        _bitsNum -= bnum;

                        if (type == MaiBitReaderTypeHigh)
                        {
                            _buffer = _buffer & ((1 << _bitsNum) - 1);
                        }
                        else if (type == MaiBitReaderTypeLow)
                        {
                            _buffer = (_buffer >> bnum) & ((1 << _bitsNum) - 1);
                        }
                    }

                    return toOut;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}