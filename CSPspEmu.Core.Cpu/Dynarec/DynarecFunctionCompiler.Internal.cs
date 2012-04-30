using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Codegen;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Dynarec
{
	unsafe public partial class DynarecFunctionCompiler
	{
		unsafe internal class InternalFunctionCompiler
		{
			DynamicMethod DynamicMethod;
			SafeILGenerator SafeILGenerator;
			DynarecFunctionCompiler DynarecFunctionCompiler;
			InstructionReader InstructionReader;
			uint EntryPC;
			SortedDictionary<uint, SafeLabel> Labels = new SortedDictionary<uint, SafeLabel>();

			//const int MaxNumberOfInstructions = 8 * 1024;
			const int MaxNumberOfInstructions = 64 * 1024;
			//const int MaxNumberOfInstructions = 128 * 1024;
			//const int MaxNumberOfInstructions = 60;

			internal InternalFunctionCompiler(DynarecFunctionCompiler DynarecFunctionCompiler, InstructionReader InstructionReader, uint EntryPC)
			{
				//this.DynamicMethod = new DynamicMethod();
				//this.SafeILGenerator = new SafeILGenerator();

				this.DynarecFunctionCompiler = DynarecFunctionCompiler;
				this.InstructionReader = InstructionReader;
				this.EntryPC = EntryPC;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			internal DynarecFunction CreateFunction()
			{
				AnalyzeBranches();
				throw (new NotImplementedException());
			}

			private void LogInstruction(Instruction Instruction)
			{
#if false
				if (CpuProcessor.PspConfig.LogInstructionStats)
				{
					var InstructionName = GetInstructionName(CpuEmiter.Instruction.Value, null);

					if (!InstructionStats.ContainsKey(InstructionName)) InstructionStats[InstructionName] = 0;
					InstructionStats[InstructionName]++;

					if (!GlobalInstructionStats.ContainsKey(InstructionName))
					{
						NewInstruction[InstructionName] = true;
						GlobalInstructionStats[InstructionName] = 0;
					}

					GlobalInstructionStats[InstructionName]++;
					//var GlobalInstructionStats = CpuProcessor.GlobalInstructionStats;
					//var InstructionStats = new Dictionary<string, uint>();
					//var NewInstruction = new Dictionary<string, bool>();
				}
#endif
			}

			/// <summary>
			/// PASS 1: Analyze Branches
			/// </summary>
			private void AnalyzeBranches()
			{
				var AnalyzedPC = new HashSet<uint>();
				var BranchesToAnalyze = new Queue<uint>();

				Labels[EntryPC] = SafeILGenerator.DefineLabel("EntryPoint");

				uint PC = EntryPC;
				uint EndPC = (uint)0xFFFFFFF0;
				uint MinPC = uint.MaxValue, MaxPC = uint.MinValue;

				BranchesToAnalyze.Enqueue(EntryPC);

				while (BranchesToAnalyze.Count > 0)
				{
				HandleNewBranch: ;
					bool EndOfBranchFound = false;

					for (PC = BranchesToAnalyze.Dequeue(); PC < EndPC; PC += 4)
					{
						// If already analyzed, stop scanning this branch.
						if (AnalyzedPC.Contains(PC)) break;
						AnalyzedPC.Add(PC);
						//Console.WriteLine("%08X".Sprintf(PC));

						if (AnalyzedPC.Count > MaxNumberOfInstructions)
						{
							throw (new InvalidDataException("Code sequence too long: >= " + MaxNumberOfInstructions + ""));
						}

						MinPC = Math.Min(MinPC, PC);
						MaxPC = Math.Max(MaxPC, PC);

						//Console.WriteLine("    PC:{0:X}", PC);

						var Instruction = InstructionReader[PC];

						var BranchInfo = DynarecBranchAnalyzer.GetBranchInfo(Instruction);

						//LogInstruction(Instruction);

						// Branch instruction.
						if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.JumpAlways))
						{
							//Console.WriteLine("Instruction");

							// Located a jump-always instruction with a delayed slot.
							EndOfBranchFound = true;
							continue;
						}
						else if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.BranchOrJumpInstruction))
						{
							var BranchAddress = Instruction.GetBranchAddress(PC);
							Labels[BranchAddress] = SafeILGenerator.DefineLabel("" + BranchAddress);
							BranchesToAnalyze.Enqueue(BranchAddress);
						}
						else if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.SyscallInstruction))
						{
							// On this special Syscall
							if (Instruction.CODE == SyscallInfo.NativeCallSyscallCode)
							{
								//PC += 4;
								goto HandleNewBranch;
							}
						}

						// Jump-Always found. And we have also processed the delayed branch slot. End the branch.
						if (EndOfBranchFound)
						{
							goto HandleNewBranch;
						}
					}
				}
			}
		}
	}
}
