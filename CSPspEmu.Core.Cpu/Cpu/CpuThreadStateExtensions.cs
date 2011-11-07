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
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu.Cpu.Emiter;
using System.Diagnostics;
using CSharpUtils.Threading;

namespace CSPspEmu.Core.Cpu
{
	static public class ProcessorExtensions
	{
		static public MipsEmiter MipsEmiter = new MipsEmiter();
		static public Action<uint, CpuEmiter> CpuEmiterInstruction = EmitLookupGenerator.GenerateSwitchDelegate<CpuEmiter>(InstructionTable.ALL);
		static public Func<uint, CpuBranchAnalyzer.Flags> GetBranchInfo = EmitLookupGenerator.GenerateInfoDelegate<CpuBranchAnalyzer, CpuBranchAnalyzer.Flags>(EmitLookupGenerator.GenerateSwitchDelegateReturn<CpuBranchAnalyzer, CpuBranchAnalyzer.Flags>(InstructionTable.ALL, ThrowOnUnexistent: false), new CpuBranchAnalyzer());

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
			var InstructionReader = new InstructionReader(MemoryStream);
			var MipsMethodEmiter = new MipsMethodEmiter(MipsEmiter, CpuThreadState);
			var ILGenerator = MipsMethodEmiter.ILGenerator;
			var CpuEmiter = new CpuEmiter(MipsMethodEmiter, InstructionReader);

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
			Debug.WriteLine("PASS1: (PC={0:X}, EndPC={1:X})", PC, EndPC);

			while (BranchesToAnalyze.Count > 0)
			{
				bool EndOfBranchFound = false;

				for (PC = BranchesToAnalyze.Dequeue(); PC < EndPC; PC += 4)
				{
					// If already analyzed, stop scanning this branch.
					if (AnalyzedPC.Contains(PC)) break;
					AnalyzedPC.Add(PC);
					//Console.WriteLine("%08X".Sprintf(PC));

					if (AnalyzedPC.Count > 8 * 1024)
					//if (AnalyzedPC.Count > 64)
					{
						throw(new InvalidDataException());
					}

					MinPC = Math.Min(MinPC, PC);
					MaxPC = Math.Max(MaxPC, PC);

					//Console.WriteLine("    PC:{0:X}", PC);

					CpuEmiter.LoadAT(PC);

					var BranchInfo = GetBranchInfo(CpuEmiter.Instruction.Value);

					// Branch instruction.
					if ((BranchInfo & CpuBranchAnalyzer.Flags.JumpInstruction) != 0)
					{
						//Console.WriteLine("Instruction");
						EndOfBranchFound = true;
						continue;
					}
					else if ((BranchInfo & CpuBranchAnalyzer.Flags.BranchOrJumpInstruction) != 0)
					{
						var BranchAddress = CpuEmiter.Instruction.GetBranchAddress(PC);
						Labels[BranchAddress] = ILGenerator.DefineLabel();
						BranchesToAnalyze.Enqueue(BranchAddress);

						// Jump Always performed.
						/*
						if ((BranchInfo & CpuBranchAnalyzer.Flags.JumpAlways) != 0)
						{
							EndOfBranchFound = true;
							continue;
						}
						*/
					}

					// A Jump Always found. And we have also processed the delayed branch slot. End the branch.
					if (EndOfBranchFound)
					{
						EndOfBranchFound = false;
						break;
					}
				}
			}

			// PASS2: Generate code and put labels;
			Action<uint> _EmitCpuInstructionAT = (_PC) =>
			{
				CpuEmiter.LoadAT(_PC);
				CpuEmiterInstruction(CpuEmiter.Instruction.Value, CpuEmiter);
			};

			uint InstructionsEmitedSinceLastWaypoint = 0;

			Action StorePC = () =>
			{
				MipsMethodEmiter.SavePC(() =>
				{
					ILGenerator.Emit(OpCodes.Ldc_I4, PC);
				});
			};

			Action<bool> EmiteInstructionCountIncrement = (bool CheckForYield) =>
			{
				//Console.WriteLine("EmiteInstructionCountIncrement: {0},{1}", InstructionsEmitedSinceLastWaypoint, CheckForYield);
				if (InstructionsEmitedSinceLastWaypoint > 0)
				{
					MipsMethodEmiter.SaveStepInstructionCount(() =>
					{
						MipsMethodEmiter.LoadStepInstructionCount();
						ILGenerator.Emit(OpCodes.Ldc_I4, InstructionsEmitedSinceLastWaypoint);
						//ILGenerator.Emit(OpCodes.Add);
						ILGenerator.Emit(OpCodes.Sub);
					});
					//ILGenerator.Emit(OpCodes.Ldc_I4, 100);
					//ILGenerator.EmitCall(OpCodes.Call, typeof(Console).GetMethod("WriteLine"), new Type[] { typeof(int) });
					InstructionsEmitedSinceLastWaypoint = 0;
				}

				if (CheckForYield)
				{
					var NoYieldLabel = ILGenerator.DefineLabel();
					MipsMethodEmiter.LoadStepInstructionCount();
					ILGenerator.Emit(OpCodes.Ldc_I4_0);
					ILGenerator.Emit(OpCodes.Bgt, NoYieldLabel);
					//ILGenerator.Emit(OpCodes.Ldc_I4, 1000000);
					//ILGenerator.Emit(OpCodes.Blt, NoYieldLabel);
					MipsMethodEmiter.SaveStepInstructionCount(() =>
					{
						ILGenerator.Emit(OpCodes.Ldc_I4_0);
					});
					StorePC();
					ILGenerator.Emit(OpCodes.Ldarg_0);
					ILGenerator.Emit(OpCodes.Call, typeof(CpuThreadState).GetMethod("Yield"));
					//ILGenerator.Emit(OpCodes.Call, typeof(GreenThread).GetMethod("Yield"));
					ILGenerator.MarkLabel(NoYieldLabel);
				}
			};

			Action EmitCpuInstruction = () =>
			{
				if (CpuThreadState.Processor.NativeBreakpoints.Contains(PC))
				{
					ILGenerator.Emit(OpCodes.Call, typeof(ProcessorExtensions).GetMethod("IsDebuggerPresentDebugBreak"));	
				}

				// Marks label.
				if (Labels.ContainsKey(PC))
				{
					EmiteInstructionCountIncrement(false);
					ILGenerator.MarkLabel(Labels[PC]);
				}

				_EmitCpuInstructionAT(PC);
				PC += 4;
				InstructionsEmitedSinceLastWaypoint++;
			};

			Debug.WriteLine("PASS2: MinPC:{0:X}, MaxPC:{1:X}", MinPC, MaxPC);

			// Jumps to the entry point.
			//ILGenerator.Emit(OpCodes.Call, typeof(ProcessorExtensions).GetMethod("IsDebuggerPresentDebugBreak"));
			ILGenerator.Emit(OpCodes.Br, Labels[EntryPC]);

			for (PC = MinPC; PC <= MaxPC; )
			{
				uint CurrentInstructionPC = PC;
				Instruction CurrentInstruction = InstructionReader[PC];

				/*
				if (!AnalyzedPC.Contains(CurrentInstructionPC))
				{
					// Marks label.
					if (Labels.ContainsKey(PC))
					{
						ILGenerator.MarkLabel(Labels[PC]);
					}


					PC += 4;
					continue;
				}
				*/

				var BranchInfo = GetBranchInfo(CurrentInstruction.Value);

				// Delayed branch instruction.
				if ((BranchInfo & CpuBranchAnalyzer.Flags.BranchOrJumpInstruction) != 0)
				{
					InstructionsEmitedSinceLastWaypoint += 2;
					EmiteInstructionCountIncrement(true);

					var BranchAddress = CurrentInstruction.GetBranchAddress(PC);

					if ((BranchInfo & CpuBranchAnalyzer.Flags.JumpInstruction) != 0)
					{
						// Marks label.
						if (Labels.ContainsKey(PC))
						{
							ILGenerator.MarkLabel(Labels[PC]);
						}

						_EmitCpuInstructionAT(PC + 4);
						_EmitCpuInstructionAT(PC + 0);
						PC += 8;
					}
					else
					{
						// Branch instruction.
						EmitCpuInstruction();

						if ((BranchInfo & CpuBranchAnalyzer.Flags.Likely) != 0)
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
				}
				// Normal instruction.
				else
				{
					// Syscall instruction.
					if ((BranchInfo & CpuBranchAnalyzer.Flags.SyscallInstruction) != 0)
					{
						StorePC();
					}
					EmitCpuInstruction();
				}
			}

			//if (BreakPoint) IsDebuggerPresentDebugBreak();
			Action<CpuThreadState> Delegate = MipsMethodEmiter.CreateDelegate();
			return Delegate;
		}

		static public void IsDebuggerPresentDebugBreak()
		{
			DebugUtils.IsDebuggerPresentDebugBreak();
		}
	}
}
