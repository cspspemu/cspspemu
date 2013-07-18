using CSharpUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.VFpu
{
	public struct VfpuRegisterInt
	{
		public uint Value;

		static public implicit operator int(VfpuRegisterInt Value) { return (int)Value.Value; }
		static public implicit operator VfpuRegisterInt(int Value) { return new VfpuRegisterInt() { Value = (uint)Value }; }
		static public implicit operator uint(VfpuRegisterInt Value) { return Value.Value; }
		static public implicit operator VfpuRegisterInt(uint Value) { return new VfpuRegisterInt() { Value = Value }; }
	}

}
