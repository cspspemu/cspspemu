//#define BIT_READER_THREAD_SAFE
//#define DEBUG_BIT_READER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Hle.Formats.audio.At3.SUB
{
	public class MaiBitReader : IDisposable
	{
		public const int MaiBitReaderTypeHigh = 0;
		public const int MaiBitReaderTypeLow = 1;

		MaiQueue0 quene_in;
		int buffer;
		int bits_num;
		int type;

		public MaiBitReader(int byte_buf_size, int type = MaiBitReaderTypeHigh)
		{
			quene_in = new MaiQueue0(byte_buf_size);
			buffer = 0;
			bits_num = 0;
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

		public bool moreByte()
		{
			if (quene_in.GetLength() != 0)
			{
				var temp = new byte[1];
				quene_in.Out(temp, 1);
				if (type == MaiBitReaderTypeHigh)
				{
					buffer = (buffer << 8) | (temp[0] & 0xFF);
				}
				else if (type == MaiBitReaderTypeLow)
				{
					buffer |= (temp[0] & 0xFF) << bits_num;
				}
				bits_num += 8;
				return false;
			}
			else
			{
				return true;
			}
		}

		public int addData(ManagedPointer<byte> src, int len_s)
		{
			int ret = _addData(src, len_s);
#if DEBUG_BIT_READER
			Console.WriteLine("addData({0}) : {1}", len_s, BitConverter.ToString(src.GetArray(len_s)));
#endif
			return ret;
		}

		private int _addData(ManagedPointer<byte> src, int len_s)
		{
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
			{
				quene_in.In(src, len_s);
			}

			return 0;
		}

		public int getRemainingBitsNum()
		{
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
			{
				int bits_remain = (quene_in.GetLength() << 3) + bits_num;

				return bits_remain;
			}
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int getWithI32Buffer(int bnum, bool get_then_del_in_buf = true)
		{
			var value = _getWithI32Buffer(bnum, get_then_del_in_buf);
#if DEBUG_BIT_READER
			Console.WriteLine("MaiBitReader.getWithI32Buffer({0}) : {1}", bnum, value);
#endif
			return value;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int _getWithI32Buffer(int bnum, bool get_then_del_in_buf = true)
		{
#if BIT_READER_THREAD_SAFE
			lock (this)
#endif
			{

				while (bnum > bits_num)
				{
					if (moreByte()) break;
				}

				if (bnum <= bits_num)
				{
					int to_out = 0;

					if (type == MaiBitReaderTypeHigh)
					{
						to_out = (buffer >> (bits_num - bnum)) & ((1 << bnum) - 1);
					}
					else if (type == MaiBitReaderTypeLow)
					{
						to_out = buffer & ((1 << bnum) - 1);
					}

					if (get_then_del_in_buf)
					{
						bits_num -= bnum;

						if (type == MaiBitReaderTypeHigh)
						{
							buffer = buffer & ((1 << bits_num) - 1);
						}
						else if (type == MaiBitReaderTypeLow)
						{
							buffer = (buffer >> bnum) & ((1 << bits_num) - 1);
						}
					}

					return to_out;
				}
				else
				{
					return 0;
				}
			}
		}
	}
}
