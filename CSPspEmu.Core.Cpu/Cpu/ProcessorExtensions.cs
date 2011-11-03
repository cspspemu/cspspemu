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
using CSPspEmu.Core.Cpu.Cpu;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu
{
	static public class ProcessorExtensions
	{
		static public Action<uint, CpuEmiter> CpuEmiterInstruction = EmitLookupGenerator.GenerateSwitchDelegate<CpuEmiter>(InstructionTable.ALL);
		static public Func<uint, bool> IsDelayedBranchInstruction = EmitLookupGenerator.GenerateSwitchDelegateReturn<bool>(InstructionTable.ALL, (ILGenerator, InstructionInfo) =>
		{
			var IsBranch = ((InstructionInfo != null) && (InstructionInfo.InstructionType & InstructionType.B) != 0);
			ILGenerator.Emit(IsBranch ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
		});

		static public void ExecuteAssembly(this Processor Processor, String Assembly, bool BreakPoint = false)
		{
			var MemoryStream = new MemoryStream();
			MemoryStream.PreservePositionAndLock(() =>
			{
				var MipsAssembler = new MipsAssembler(MemoryStream);

				MipsAssembler.Assemble(Assembly);
			});
			Processor.CreateDelegateForPC(MemoryStream, 0)(Processor);
		}

		static public Action<Processor> CreateDelegateForPC(this Processor Processor, Stream MemoryStream, uint StartPC)
		{
			var MipsEmiter = new MipsEmiter();
			var InstructionReader = new InstructionReader(MemoryStream);
			var MipsMethodEmiter = new MipsMethodEmiter(MipsEmiter);
			var CpuEmiter = new CpuEmiter(MipsMethodEmiter);

			uint PC;
			uint EndPC = (uint)MemoryStream.Length;
			uint MinPC = uint.MaxValue, MaxPC = uint.MinValue;

			var Labels = new SortedDictionary<uint, Label>();
			var BranchesToAnalyze = new Queue<uint>();
			var AnalyzedPC = new HashSet<uint>();

			BranchesToAnalyze.Enqueue(StartPC);

			// PASS1: Analyze and find labels.
			PC = StartPC;
			//Console.WriteLine("PASS1: (PC={0:X}, EndPC={1:X})", PC, EndPC);
			while (BranchesToAnalyze.Count > 0)
			{
				PC = BranchesToAnalyze.Dequeue();

				while (PC < EndPC)
				{
					// If already analyzed, stop scanning this branch.
					if (AnalyzedPC.Contains(PC)) break;
					AnalyzedPC.Add(PC);

					MinPC = Math.Min(MinPC, PC);
					MaxPC = Math.Max(MaxPC, PC);

					//Console.WriteLine("    PC:{0:X}", PC);

					CpuEmiter.Instruction = InstructionReader[PC];

					// Delayed branch instruction.
					if (IsDelayedBranchInstruction(CpuEmiter.Instruction.Value))
					{
						var BranchAddress = CpuEmiter.Instruction.GetBranchAddress(PC);
						Labels[BranchAddress] = MipsMethodEmiter.ILGenerator.DefineLabel();
						BranchesToAnalyze.Enqueue(BranchAddress);
					}

					PC += 4;
				}
			}

			// PASS2: Generate code and put labels;
			Action EmitCpuInstruction = () =>
			{
				CpuEmiter.Instruction = InstructionReader[PC];
				CpuEmiterInstruction(CpuEmiter.Instruction.Value, CpuEmiter);
				//Console.WriteLine("{0:X}", CpuEmiter.Instruction.Value);
				PC += 4;
			};

			//Console.WriteLine("PASS2: MinPC:{0:X}, MaxPC:{1:X}", MinPC, MaxPC);

			for (PC = MinPC; PC <= MaxPC; )
			{
				// Marks label.
				if (Labels.ContainsKey(PC))
				{
					MipsMethodEmiter.ILGenerator.MarkLabel(Labels[PC]);
				}

				// Delayed branch instruction.
				if (IsDelayedBranchInstruction(InstructionReader[PC].Value))
				{
					var BranchAddress = InstructionReader[PC].GetBranchAddress(PC);

					// Branch instruction.
					EmitCpuInstruction();

					// Delayed instruction.
					EmitCpuInstruction();

					CpuEmiter._branch_post(Labels[BranchAddress]);
				}
				// Normal instruction.
				else
				{
					EmitCpuInstruction();
				}
			}

			//if (BreakPoint) IsDebuggerPresentDebugBreak();
			return MipsMethodEmiter.CreateDelegate();
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
