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
using System.Threading;
using CSPspEmu.Core.Debug;

namespace CSPspEmu.Core.Cpu
{
	static public class ProcessorExtensions
	{
		static public MipsEmiter MipsEmiter = new MipsEmiter();
		static public Action<uint, CpuEmiter> CpuEmiterInstruction = EmitLookupGenerator.GenerateSwitchDelegate<CpuEmiter>(InstructionTable.ALL);
		//static public Func<uint, bool> IsDelayedBranchInstruction = EmitLookupGenerator.GenerateSwitchDelegateReturn<bool>(InstructionTable.ALL, (ILGenerator, InstructionInfo) =>
		static public Func<uint, bool> IsDelayedBranchInstruction = EmitLookupGenerator.GenerateSwitchDelegateReturn<bool>(InstructionTable.ALL_BRANCHES, (ILGenerator, InstructionInfo) =>
		{
			var IsBranch = ((InstructionInfo != null) && (InstructionInfo.InstructionType & InstructionType.B) != 0);
			ILGenerator.Emit(IsBranch ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
		});

		static public Func<uint, bool> IsLikelyInstruction = EmitLookupGenerator.GenerateSwitchDelegateReturn<bool>(InstructionTable.ALL_BRANCHES, (ILGenerator, InstructionInfo) =>
		{
			var IsLikely = ((InstructionInfo != null) && (InstructionInfo.InstructionType & InstructionType.Likely) != 0);
			ILGenerator.Emit(IsLikely ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
		});

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
			return Processor.CreateDelegateForPC(MemoryStream, 0);
		}

		static public Action<CpuThreadState> CreateDelegateForPC(this CpuThreadState Processor, Stream MemoryStream, uint EntryPC)
		{
			//var MipsEmiter = new MipsEmiter();
			var InstructionReader = new InstructionReader(MemoryStream);
			var MipsMethodEmiter = new MipsMethodEmiter(MipsEmiter, Processor);
			var ILGenerator = MipsMethodEmiter.ILGenerator;
			var CpuEmiter = new CpuEmiter(MipsMethodEmiter);

			uint PC;
			uint EndPC = (uint)MemoryStream.Length;
			uint MinPC = uint.MaxValue, MaxPC = uint.MinValue;

			var Labels = new SortedDictionary<uint, Label>();
			var BranchesToAnalyze = new Queue<uint>();
			var AnalyzedPC = new HashSet<uint>();

			Labels[EntryPC] = ILGenerator.DefineLabel();

			BranchesToAnalyze.Enqueue(EntryPC);

			// PASS1: Analyze and find labels.
			PC = EntryPC;
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

					// Branch instruction.
					if (IsDelayedBranchInstruction(CpuEmiter.Instruction.Value))
					{
						var BranchAddress = CpuEmiter.Instruction.GetBranchAddress(PC);
						Labels[BranchAddress] = ILGenerator.DefineLabel();
						BranchesToAnalyze.Enqueue(BranchAddress);
					}

					PC += 4;
				}
			}

			// PASS2: Generate code and put labels;
			Action EmitCpuInstruction = () =>
			{
				// Marks label.
				if (Labels.ContainsKey(PC))
				{
					ILGenerator.MarkLabel(Labels[PC]);
				}

				CpuEmiter.Instruction = InstructionReader[PC];
				CpuEmiterInstruction(CpuEmiter.Instruction.Value, CpuEmiter);
				//Console.WriteLine("{0:X}", CpuEmiter.Instruction.Value);
				PC += 4;
			};

			//Console.WriteLine("PASS2: MinPC:{0:X}, MaxPC:{1:X}", MinPC, MaxPC);

			// Jumps to the entry point.



			ILGenerator.Emit(OpCodes.Call, typeof(ProcessorExtensions).GetMethod("IsDebuggerPresentDebugBreak"));
			ILGenerator.Emit(OpCodes.Br, Labels[EntryPC]);

			for (PC = MinPC; PC <= MaxPC; )
			{
				uint CurrentInstructionPC = PC;
				Instruction CurrentInstruction = InstructionReader[PC];

				// Delayed branch instruction.
				if (IsDelayedBranchInstruction(CurrentInstruction.Value))
				{
					var BranchAddress = CurrentInstruction.GetBranchAddress(PC);

					// Branch instruction.
					EmitCpuInstruction();

					if (IsLikelyInstruction(CurrentInstruction.Value))
					{
						//Console.WriteLine("Likely");
						// Delayed instruction.
						CpuEmiter._branch_likely(() =>
						{
							EmitCpuInstruction();
						});
					}
					else
					{
						//Console.WriteLine("Not Likely");
						// Delayed instruction.
						EmitCpuInstruction();
					}

					if (CurrentInstructionPC + 4 != BranchAddress)
					{
						CpuEmiter._branch_post(Labels[BranchAddress]);
					}
				}
				// Normal instruction.
				else
				{
					EmitCpuInstruction();
				}
			}

			//if (BreakPoint) IsDebuggerPresentDebugBreak();
			Action<CpuThreadState> Delegate = MipsMethodEmiter.CreateDelegate();
			return Delegate;
		}

		static public void IsDebuggerPresentDebugBreak()
		{
			//DebugUtils.IsDebuggerPresentDebugBreak();
		}
	}
}
