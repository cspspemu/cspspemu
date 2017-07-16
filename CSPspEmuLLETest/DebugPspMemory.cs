using System;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;

namespace CSPspEmuLLETest
{
	public class DebugPspMemory : LazyPspMemory
	{
		public CpuThreadState CpuThreadState;
		public Dma Dma;
		public bool MountedPreIpl = true;

		bool LogNormalReads = false;

		private bool IsHwRegister(uint address)
		{
			var maskedAddress = address & PspMemory.MemoryMask;

			if (maskedAddress >= 0x1FD00000 && maskedAddress < 0x1FD00000 + 0x1000) return false;

			if (!MountedPreIpl)
			{
				//if ((Address & PspMemory.MemoryMask) >= VectorsOffset) return true;
				//if ((MaskedAddress >= VectorsOffset) && (MaskedAddress < (VectorsOffset + 0x1000))) return true;

				if ((maskedAddress >= VectorsOffset)) return true;


				//var MaskedAddress = Address;
				//if (MaskedAddress >= 0xbfc00000 && MaskedAddress < 0xbfc00000 + 0x1000) return true;
			}
			else
			{
			}
			return !IsAddressValid(address & PspMemory.MemoryMask);
		}

		public override byte Read1(uint address)
		{
			if (IsHwRegister(address)) return (byte)Dma.ReadDma(1, address);
			var value = base.Read1(address);
			if (LogNormalReads) Console.WriteLine("Read1(0x{0:X8}) <- 0x{1:X2} at 0x{2:X8}", address, value, CpuThreadState.PC);
			return value;
		}

		public override ushort Read2(uint address)
		{
			if (IsHwRegister(address)) return (ushort)Dma.ReadDma(2, address);
			var value = base.Read2(address);
			if (LogNormalReads) Console.WriteLine("Read2(0x{0:X8}) <- 0x{1:X4} at 0x{2:X8}", address, value, CpuThreadState.PC);
			return value;
		}

		public override uint Read4(uint address)
		{
			if (IsHwRegister(address)) return (uint)Dma.ReadDma(4, address);
			var value = base.Read4(address);
			if (LogNormalReads) Console.WriteLine("Read4(0x{0:X8}) <- 0x{1:X8} at 0x{2:X8}", address, value, CpuThreadState.PC);
			return value;
		}

		private static void TrackWrite(uint address, uint value)
		{
			if (((address & PspMemory.MemoryMask) >= 0x1FC00000) && ((address & PspMemory.MemoryMask) <= 0x20000000))
			{
				//Console.WriteLine("{0:X8}: Write: {1:X8} : {2:X8}", CpuThreadState.PC, Address, Value);
			}
		}

		public override void Write1(uint address, byte value)
		{
			TrackWrite(address, value);
			if (IsHwRegister(address)) { Dma.WriteDma(1, address, value); return; }
			if (LogNormalReads) Console.WriteLine("Write1(0x{0:X8}) -> 0x{1:X2} at 0x{2:X8}", address, value, CpuThreadState.PC);
			base.Write1(address, value);
		}

		public override void Write2(uint address, ushort value)
		{
			TrackWrite(address, value);
			if (IsHwRegister(address)) { Dma.WriteDma(2, address, value); return; }
			if (LogNormalReads) Console.WriteLine("Write2(0x{0:X8}) -> 0x{1:X4} at 0x{2:X8}", address, value, CpuThreadState.PC);
			base.Write2(address, value);
		}

		public override void Write4(uint address, uint value)
		{
			TrackWrite(address, value);
			if (IsHwRegister(address)) { Dma.WriteDma(4, address, value); return; }
			if (LogNormalReads) Console.WriteLine("Write4(0x{0:X8}) -> 0x{1:X8} at 0x{2:X8}", address, value, CpuThreadState.PC);
			base.Write4(address, value);
		}
	}
}
