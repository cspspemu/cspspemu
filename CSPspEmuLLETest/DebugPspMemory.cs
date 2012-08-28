using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;

namespace CSPspEmuLLETest
{
	public class DebugPspMemory : LazyPspMemory
	{
		public CpuThreadState CpuThreadState;

		public override byte Read1(uint Address)
		{
			var Value = base.Read1(Address); ;
			Console.WriteLine("Read1(0x{0:X8}) <- 0x{1:X2} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
			return Value;
		}

		public override ushort Read2(uint Address)
		{
			var Value = base.Read2(Address); ;
			Console.WriteLine("Read2(0x{0:X8}) <- 0x{1:X4} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
			return Value;
		}

		public override uint Read4(uint Address)
		{
			var Value = base.Read4(Address); ;
			Console.WriteLine("Read4(0x{0:X8}) <- 0x{1:X8} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
			return Value;
		}

		public override void Write1(uint Address, byte Value)
		{
			Console.WriteLine("Write1(0x{0:X8}) -> 0x{1:X2} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
			base.Write1(Address, Value);
		}

		public override void Write2(uint Address, ushort Value)
		{
			Console.WriteLine("Write2(0x{0:X8}) -> 0x{1:X4} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
			base.Write2(Address, Value);
		}

		public override void Write4(uint Address, uint Value)
		{
			Console.WriteLine("Write4(0x{0:X8}) -> 0x{1:X8} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
			base.Write4(Address, Value);
		}
	}
}
