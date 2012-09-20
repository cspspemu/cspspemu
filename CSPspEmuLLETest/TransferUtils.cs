using System;
using CSPspEmuLLETest;

static public class TransferUtils
{
	static public void Transfer<T>(Dma.Direction Direction, ref T DeviceValue, ref uint MemoryValue)
	{
		if (Direction == Dma.Direction.Read)
		{
			MemoryValue = (uint)(object)DeviceValue;
		}
		else
		{
			DeviceValue = (T)(object)MemoryValue;
		}
	}

	static public void TransferToArray(Dma.Direction Direction, byte[] Array, int Offset, int Size, ref uint MemoryValue)
	{
		if (Direction == Dma.Direction.Read)
		{
			switch (Size)
			{
				case 4: MemoryValue = BitConverter.ToUInt32(Array, Offset); break;
				case 2: MemoryValue = BitConverter.ToUInt16(Array, Offset); break;
				case 1: MemoryValue = Array[Offset]; break;
				default: throw (new NotImplementedException());
			}
		}
		else
		{
			byte[] Bytes;
			switch (Size)
			{
				case 4: Bytes = BitConverter.GetBytes((uint)MemoryValue); break;
				case 2: Bytes = BitConverter.GetBytes((ushort)MemoryValue); break;
				case 1: Bytes = BitConverter.GetBytes((byte)MemoryValue); break;
				default: throw (new NotImplementedException());
			}
			Buffer.BlockCopy(Bytes, 0, Array, Offset, Size);
		}
	}
}