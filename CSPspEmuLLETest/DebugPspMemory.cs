using System;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;

namespace CSPspEmuLLETest
{
	public class DebugPspMemory : LazyPspMemory
	{
		public CpuThreadState CpuThreadState;
		public Dma Dma;

		bool LogNormalReads = false;

		public override byte Read1(uint Address)
		{
			if (!this.IsAddressValid(Address)) return (byte)Dma.ReadDMA(1, Address);
			var Value = base.Read1(Address);
			if (LogNormalReads) Console.WriteLine("Read1(0x{0:X8}) <- 0x{1:X2} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
			return Value;
		}

		public override ushort Read2(uint Address)
		{
			if (!this.IsAddressValid(Address)) return (ushort)Dma.ReadDMA(2, Address);
			var Value = base.Read2(Address);
			if (LogNormalReads) Console.WriteLine("Read2(0x{0:X8}) <- 0x{1:X4} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
			return Value;
		}

		public override uint Read4(uint Address)
		{
			if (!this.IsAddressValid(Address)) return (uint)Dma.ReadDMA(4, Address);
			var Value = base.Read4(Address);
			if (LogNormalReads) Console.WriteLine("Read4(0x{0:X8}) <- 0x{1:X8} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
			return Value;
		}

		private static void TrackWrite(uint Address, uint Value)
		{
			if (((Address & PspMemory.MemoryMask) >= 0x1FC00000) && ((Address & PspMemory.MemoryMask) <= 0x20000000))
			{
				//Console.WriteLine("{0:X8}: Write: {1:X8} : {2:X8}", CpuThreadState.PC, Address, Value);
			}
		}

		public override void Write1(uint Address, byte Value)
		{
			TrackWrite(Address, Value);
			if (!this.IsAddressValid(Address)) { Dma.WriteDMA(1, Address, Value); return; }
			if (LogNormalReads) Console.WriteLine("Write1(0x{0:X8}) -> 0x{1:X2} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
			base.Write1(Address, Value);
		}

		public override void Write2(uint Address, ushort Value)
		{
			TrackWrite(Address, Value);
			if (!this.IsAddressValid(Address)) { Dma.WriteDMA(2, Address, Value); return; }
			if (LogNormalReads) Console.WriteLine("Write2(0x{0:X8}) -> 0x{1:X4} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
			base.Write2(Address, Value);
		}

		public override void Write4(uint Address, uint Value)
		{
			TrackWrite(Address, Value);
			if (!this.IsAddressValid(Address)) { Dma.WriteDMA(4, Address, Value); return; }
			if (LogNormalReads) Console.WriteLine("Write4(0x{0:X8}) -> 0x{1:X8} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
			base.Write4(Address, Value);
		}
	}
}
