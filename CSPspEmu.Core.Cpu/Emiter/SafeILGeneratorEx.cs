using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codegen;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Emiter
{
	public class SafeILGeneratorEx : SafeILGenerator
	{
		public SafeILGeneratorEx(ILGenerator ILGenerator, bool CheckTypes, bool DoDebug)
			: base(ILGenerator, CheckTypes, DoDebug)
		{
		}

		public void LoadArgument0CpuThreadState()
		{
			LoadArgument<CpuThreadState>(0);
		}
	}
}
