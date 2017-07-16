using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpUtils
{
	unsafe public class ByteRingBufferWrapper
	{
		private byte* DataPointer;
		private int DataLength;
		private long ReadPosition = 0;
		private long WritePosition = 0;

		private ByteRingBufferWrapper()
		{
		}

		static public ByteRingBufferWrapper FromPointer(byte* Pointer, int DataLength)
		{
			return new ByteRingBufferWrapper()
			{
				DataPointer = Pointer,
				DataLength = DataLength,
			};
		}

		public int Capacity
		{
			get
			{
				return this.DataLength;
			}
		}

		public long ReadAvailable
		{
			get
			{
				return WritePosition - ReadPosition;
			}
		}

		public long WriteAvailable
		{
			get
			{
				return Capacity - ReadAvailable;
			}
		}

		public void Write(byte Item)
		{
			if (WriteAvailable <= 0) throw (new OverflowException("RingBuffer is full"));
			this.DataPointer[WritePosition++ % Capacity] = Item;
		}

		public byte Read()
		{
			if (ReadAvailable <= 0) throw (new OverflowException("RingBuffer is empty"));
			return this.DataPointer[ReadPosition++ % Capacity];
		}

		public int Write(byte[] TransferData, int Offset = 0, int Length = -1)
		{
			if (Length == -1) Length = DataLength - Offset;
			Length = Math.Min(Length, (int)this.WriteAvailable);
			int Transferred = 0;
			while (Length-- > 0)
			{
				Write(TransferData[Offset++]);
				Transferred++;
			}
			return Transferred;
		}

		public int Read(byte[] TransferData, int Offset = 0, int Length = -1)
		{
			if (Length == -1) Length = DataLength - Offset;
			Length = Math.Min(Length, (int)this.ReadAvailable);
			int Transferred = 0;
			while (Length-- > 0)
			{
				TransferData[Offset++] = Read();
				Transferred++;
			}
			return Transferred;
		}
	}

	public class RingBuffer<T>
	{
		private T[] Data;
		private long ReadPosition = 0;
		private long WritePosition = 0;

		public RingBuffer(int Capacity)
		{
			this.Data = new T[Capacity];
		}

		public int Capacity
		{
			get
			{
				return this.Data.Length;
			}
		}

		public long ReadAvailable
		{
			get
			{
				return WritePosition - ReadPosition;
			}
		}

		public long WriteAvailable
		{
			get
			{
				return Capacity - ReadAvailable;
			}
		}

		public void Write(T Item)
		{
			if (WriteAvailable <= 0) throw (new OverflowException("RingBuffer is full"));
			this.Data[WritePosition++ % Capacity] = Item;
		}

		public T Read()
		{
			if (ReadAvailable <= 0) throw (new OverflowException("RingBuffer is empty"));
			return this.Data[ReadPosition++ % Capacity];
		}

		public int Write(T[] TransferData, int Offset = 0, int Length = -1)
		{
			if (Length == -1) Length = Data.Length - Offset;
			Length = Math.Min(Length, (int)this.WriteAvailable);
			int Transferred = 0;
			while (Length-- > 0)
			{
				Write(TransferData[Offset++]);
				Transferred++;
			}
			return Transferred;
		}

		public int Read(T[] TransferData, int Offset = 0, int Length = -1)
		{
			if (Length == -1) Length = Data.Length - Offset;
			Length = Math.Min(Length, (int)this.ReadAvailable);
			int Transferred = 0;
			while (Length-- > 0)
			{
				TransferData[Offset++] = Read();
				Transferred++;
			}
			return Transferred;
		}
	}
}
