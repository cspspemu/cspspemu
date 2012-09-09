using System;

namespace CSPspEmuLLETest
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="http://hitmen.c02.at/files/yapspd/psp_doc/chap8.html#sec8.6"/>
	public class LleNAND : ILleDma
	{
		[Flags]
		public enum EnumControlRegister : uint
		{
			CalculateECCWhenWritting = (1 << 17),
			CalculateECCWhenReading = (1 << 16),
		}

		[Flags]
		public enum EnumStatus : uint
		{
			WriteProtected = (1 << 7),
			Busy = (1 << 0),
		}

		EnumControlRegister ControlRegister;
		EnumStatus Status;
		uint Command;

		/// <summary>
		/// Physical page to access
		/// </summary>
		uint Address;

		private static void _Transfer<T>(Dma.Direction Direction, ref T DeviceValue, ref uint MemoryValue)
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

		private static void Reset()
		{
			Console.WriteLine("Reset NAND controller to default state?");
		}

		public void Transfer(Dma.Direction Direction, int Size, uint Address, ref uint Value)
		{
			switch (Address) 
			{
				case 0xBD101000: _Transfer(Direction, ref ControlRegister, ref Value); break;
				case 0xBD101004: _Transfer(Direction, ref Status, ref Value); break;
				case 0xBD101008: _Transfer(Direction, ref Command, ref Value); break;
				case 0xBD10100C: _Transfer(Direction, ref Address, ref Value); break;
				case 0xBD101014:
					if (Direction == Dma.Direction.Write)
					{
						Reset();
					}
				break;
			}
		}
	}
}
