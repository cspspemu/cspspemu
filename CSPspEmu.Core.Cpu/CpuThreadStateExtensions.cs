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
		static public void ExecuteAssembly(this CpuThreadState CpuThreadState, String Assembly, bool BreakPoint = false)
		{
			CpuThreadState.CpuProcessor.CreateDelegateForString(Assembly, BreakPoint)(CpuThreadState);
		}

		static public Action<CpuThreadState> CreateDelegateForString(this CpuProcessor CpuProcessor, String Assembly, bool BreakPoint = false)
		{
			var MemoryStream = new MemoryStream();
			MemoryStream.PreservePositionAndLock(() =>
			{
				var MipsAssembler = new MipsAssembler(MemoryStream);

				MipsAssembler.Assemble(Assembly);
			});
			var Delegate = CpuProcessor.CreateDelegateForPC(MemoryStream, 0);
			return (_CpuThreadState) =>
			{
				_CpuThreadState.StepInstructionCount = 1000000;
				Delegate(_CpuThreadState);
			};
		}

		static public Action<CpuThreadState> CreateDelegateForPC(this CpuProcessor CpuProcessor, Stream MemoryStream, uint EntryPC)
		{
			return FunctionGenerator.CreateDelegateForPC(CpuProcessor, MemoryStream, EntryPC);
		}

		static public Action<CpuThreadState> CreateAndCacheDelegateForPC(this CpuProcessor CpuProcessor, Stream MemoryStream, uint EntryPC)
		{
			var Delegate = CpuProcessor.MethodCache.TryGetMethodAt(EntryPC);
			if (Delegate == null)
			{
				Delegate = CpuProcessor.CreateDelegateForPC(MemoryStream, EntryPC);
				CpuProcessor.MethodCache.SetMethodAt(EntryPC, Delegate);
			}
			return Delegate;
		}
	}
}
