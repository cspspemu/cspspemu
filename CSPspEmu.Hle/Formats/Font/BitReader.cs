using System;
using System.Collections.Generic;

namespace CSPspEmu.Hle.Formats.Font
{
	public class BitReader : IDisposable
	{
		private byte[] Data;
		private int BitOffset;
		private int ByteOffset;

		public BitReader(byte[] Data)
		{
			this.Data = Data;
		}

		public void Reset()
		{
			this.ByteOffset = 0;
			this.BitOffset = 0;
		}

		public int Position
		{
			get {
				return this.ByteOffset * 8 + this.BitOffset;
			}
			set
			{
				this.BitOffset = value % 8;
				this.ByteOffset = value / 8;
			}
			//Console.WriteLine("bit: {0}, byte: {1}", this.BitOffset, this.ByteOffset);
		}

		public int BitsLeft
		{
			get
			{
				return ((Data.Length - ByteOffset) - 1) * 8 + (8 - BitOffset);
			}
		}

		public uint ReadBits(int Count)
		{
			int Value = 0;
			int ReadOffset = 0;

			while (Count > 0)
			{
				int LeftInByte = 8 - this.BitOffset;
				int ReadCount = Math.Min(Count, LeftInByte);
				//Console.WriteLine("Byte[{0}] = {1}", ByteOffset, Data[ByteOffset]);
				Value |= ((Data[ByteOffset] >> this.BitOffset) & ((1 << ReadCount) - 1)) << ReadOffset;

				ReadOffset += ReadCount;
				BitOffset += ReadCount;
				if (BitOffset == 8)
				{
					BitOffset = 0;
					ByteOffset++;
				}
				Count -= ReadCount;
			}

			return (uint)Value;
		}

		public void SkipBits(int Count)
		{
			BitOffset += Count % 8;
			ByteOffset += Count / 8;
		}

		public int ReadBitsSigned(int Count)
		{
			int Value = (int)ReadBits(Count);
			if ((Value & (1 << Count)) != 0)
			{
				Value |= ~((1 << Count) - 1);
			}
			return Value;
		}


		static public uint ReadBitsAt(byte[] Data, int Offset, int Count)
		{
			var BitReader = new BitReader(Data);
			BitReader.Position = Offset;
			return BitReader.ReadBits(Count);
		}

		static public IEnumerable<KeyValuePair<uint, uint>> FixedBitReader(byte[] Data, int BitCount = 0, int Offset = 0)
		{
			using (var BitReader = new BitReader(Data))
			{
				BitReader.Position = Offset;

				uint Index = 0;
				while (BitReader.BitsLeft >= BitCount)
				{
					yield return new KeyValuePair<uint, uint>(Index++, BitReader.ReadBits(BitCount));
				}
			}
		}

		public void Dispose()
		{
			Data = null;
		}
	}
}
