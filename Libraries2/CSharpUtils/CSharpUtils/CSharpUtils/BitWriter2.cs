using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpUtils
{
	public class BitWriter2
	{
		public enum Direction
		{
		}
		Stream Stream;
		uint CurrentValue;
		int ByteCapacity = 1;
		int BitsCapacity { get { return ByteCapacity * 8; } }
		int LeftBits;
		int CurrentBits { get { return BitsCapacity - LeftBits; } }
		bool LSB;

		public BitWriter2(Stream Stream, int ByteCapacity = 1, bool LSB = false)
		{
			this.Stream = Stream;
			this.ByteCapacity = ByteCapacity;
			this.LSB = LSB;
			ResetValue();
		}

		private void ResetValue()
		{
			CurrentValue = 0;
			LeftBits = BitsCapacity;
		}

		public BitWriter2 WriteBits(int Count, int Value)
		{
			WriteBits(Count, (uint)Value);
			return this;
		}

		public BitWriter2 WriteBits(int Count, uint Value)
		{
			if (Count > LeftBits)
			{
				int Bits1 = LeftBits;
				int Bits2 = Count - LeftBits;
				WriteBits(Bits1, Value >> (Count - Bits1));
				WriteBits(Bits2, Value);
				return this;
			}

			if (LSB)
			{
				CurrentValue |= (uint)((Value & BitUtils.CreateMask(Count)) << (CurrentBits));
			}
			else
			{
				CurrentValue |= (uint)((Value & BitUtils.CreateMask(Count)) << (LeftBits - Count));
			}
			LeftBits -= Count;

			if (LeftBits == 0)
			{
				//Console.WriteLine("Writting: {0:X" + (ByteCapacity * 2) + "}", CurrentValue);
				switch (ByteCapacity)
				{
					case 1: Stream.WriteByte((byte)CurrentValue); break;
					case 2: Stream.WriteStruct((ushort)CurrentValue); break;
					case 4: Stream.WriteStruct((uint)CurrentValue); break;
					throw(new InvalidOperationException());
				}
				ResetValue();
			}

			return this;
		}

		public void Align()
		{
			if (CurrentBits > 0)
			{
				WriteBits(LeftBits, 0);
			}
		}
	}
}
