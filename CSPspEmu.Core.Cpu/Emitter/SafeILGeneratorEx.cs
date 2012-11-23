using SafeILGenerator;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public class SafeILGeneratorEx : CSafeILGenerator
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
