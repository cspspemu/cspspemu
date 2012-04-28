using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Emiter;
using CSPspEmu.Core.Cpu.Table;
using System.IO;
using CSPspEmu.Core.Cpu.Assembler;
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
			var Method = CpuThreadState.CpuProcessor.CreateDelegateForString(Assembly, BreakPoint);
			Method(CpuThreadState);
		}

		static public Action<CpuThreadState> CreateDelegateForString(this CpuProcessor CpuProcessor, String Assembly, bool BreakPoint = false)
		{
			CpuProcessor.MethodCache.Clear();

			Assembly += "\r\nbreak\r\n";
			var MemoryStream = new MemoryStream();
			MemoryStream.PreservePositionAndLock(() =>
			{
				var MipsAssembler = new MipsAssembler(MemoryStream);

				MipsAssembler.Assemble(Assembly);
			});

			//Console.WriteLine(Assembly);

			return (_CpuThreadState) =>
			{
				_CpuThreadState.PC = 0;

				//Console.WriteLine("PC: {0:X}", _CpuThreadState.PC);
				try
				{
					while (true)
					{
						//Console.WriteLine("PC: {0:X}", _CpuThreadState.PC);
						var Delegate = CpuProcessor.CreateDelegateForPC(MemoryStream, _CpuThreadState.PC);
						_CpuThreadState.StepInstructionCount = 1000000;
						Delegate.Delegate(_CpuThreadState);
					}
				}
				catch (PspBreakException)
				{
				}
			};
		}

		static public PspMethodStruct CreateDelegateForPC(this CpuProcessor CpuProcessor, Stream MemoryStream, uint EntryPC)
		{
			return FunctionGenerator.CreateDelegateForPC(CpuProcessor, MemoryStream, EntryPC);
		}

		static public PspMethodStruct CreateAndCacheDelegateForPC(this CpuProcessor CpuProcessor, Stream MemoryStream, uint EntryPC)
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
