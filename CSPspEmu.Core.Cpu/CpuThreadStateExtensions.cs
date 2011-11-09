using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Emiter;
using CSPspEmu.Core.Cpu.Table;
using System.IO;
using CSPspEmu.Core.Cpu.Assembler;
using CSharpUtils.Extensions;
using System.Runtime.InteropServices;
using CSPspEmu.Core.Cpu;
using System.Reflection.Emit;
using System.Threading;
using CSPspEmu.Core;
using System.Diagnostics;
using CSharpUtils.Threading;

namespace CSPspEmu.Core.Cpu
{
	static public class ProcessorExtensions
	{
		static public void ExecuteAssembly(this CpuThreadState Processor, String Assembly, bool BreakPoint = false)
		{
			Processor.CreateDelegateForString(Assembly, BreakPoint)(Processor);
		}

		static public Action<CpuThreadState> CreateDelegateForString(this CpuThreadState Processor, String Assembly, bool BreakPoint = false)
		{
			var MemoryStream = new MemoryStream();
			MemoryStream.PreservePositionAndLock(() =>
			{
				var MipsAssembler = new MipsAssembler(MemoryStream);

				MipsAssembler.Assemble(Assembly);
			});
			var Delegate = Processor.CreateDelegateForPC(MemoryStream, 0);
			return (CpuThreadState) =>
			{
				CpuThreadState.StepInstructionCount = 1000000;
				Delegate(CpuThreadState);
			};
		}

		static public Action<CpuThreadState> CreateDelegateForPC(this CpuThreadState CpuThreadState, Stream MemoryStream, uint EntryPC)
		{
			return FunctionGenerator.CreateDelegateForPC(CpuThreadState, MemoryStream, EntryPC);
		}

		static public void IsDebuggerPresentDebugBreak()
		{
			DebugUtils.IsDebuggerPresentDebugBreak();
		}
	}
}
