using Codegen;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Emiter
{
	public class SafeILGeneratorEx : SafeILGenerator
	{
		public SafeILGeneratorEx(ILGenerator ILGenerator, bool CheckTypes, bool DoDebug, bool DoLog)
			: base(ILGenerator, CheckTypes, DoDebug, DoLog)
		{
		}

		public void LoadArgument0CpuThreadState()
		{
			//this.Comment("LoadArgument0CpuThreadState");
			LoadArgument<CpuThreadState>(0);
		}
	}
}
