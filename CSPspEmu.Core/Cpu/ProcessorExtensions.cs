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

namespace CSPspEmu.Core.Cpu
{
	static public class ProcessorExtensions
	{
		static public void ExecuteAssembly(this Processor Processor, String Assembly, bool BreakPoint = false)
		{
			var MipsEmiter = new MipsEmiter();
			var MipsMethodEmiter = new MipsMethodEmiter(MipsEmiter);
			var CpuEmiter = new CpuEmiter(MipsMethodEmiter);
			var EmitLookupGenerator = new EmitLookupGenerator();
			var CpuEmiterInstruction = EmitLookupGenerator.GenerateSwitchDelegate<CpuEmiter>(InstructionTable.ALL);

			var MemoryStream = new MemoryStream();
			var BinaryReader = new BinaryReader(MemoryStream);
			MemoryStream.PreservePositionAndLock(() =>
			{
				var MipsAssembler = new MipsAssembler(MemoryStream);

				MipsAssembler.Assemble(Assembly);
			});

			while (MemoryStream.Available() > 0)
			{
				uint InstructionValue = BinaryReader.ReadUInt32();
				//Console.WriteLine("{0:X}", InstructionValue);
				CpuEmiterInstruction(CpuEmiter.Instruction.Value = InstructionValue, CpuEmiter);
			}

			if (BreakPoint) IsDebuggerPresentDebugBreak();
			MipsMethodEmiter.CreateDelegate()(Processor);
		}

		static public void IsDebuggerPresentDebugBreak()
		{
			if (IsDebuggerPresent()) DebugBreak();
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern bool IsDebuggerPresent();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern void DebugBreak();
	}
}
