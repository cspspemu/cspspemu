//#define DEBUG_TRACE_INSTRUCTIONS
//#define ENABLE_JUMP_GOTO

#define ENABLE_NATIVE_CALLS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu.Emitter;
using CSPspEmu.Core.Cpu.Table;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast;
using CSPspEmu.Core.Cpu.Dynarec.Ast;
using System.Diagnostics;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu.Dynarec
{
	public partial class DynarecFunctionCompiler
	{
        internal class InternalFunctionCompiler
		{
			public static Func<uint, CpuEmitter, AstNodeStm> CpuEmitterInstruction = EmitLookupGenerator.GenerateSwitchDelegateReturn<CpuEmitter, AstNodeStm>(InstructionTable.ALL);
			static MipsDisassembler MipsDisassembler = new MipsDisassembler();
			CpuEmitter CpuEmitter;
			DynarecFunctionCompiler DynarecFunctionCompiler;
			IInstructionReader InstructionReader;

			[Inject]
			CpuProcessor CpuProcessor;

			[Inject]
			PspMemory Memory;

			MipsMethodEmitter MipsMethodEmitter;

			uint EntryPC;
			SortedDictionary<uint, AstLabel> Labels = new SortedDictionary<uint, AstLabel>();
			SortedDictionary<uint, AstLabel> LabelsJump = new SortedDictionary<uint, AstLabel>();

			//const int MaxNumberOfInstructions = 8 * 1024;
			const int MaxNumberOfInstructions = 64 * 1024;
			//const int MaxNumberOfInstructions = 128 * 1024;
			//const int MaxNumberOfInstructions = 60;

			Dictionary<string, uint> GlobalInstructionStats;
			Dictionary<string, uint> InstructionStats;
			Dictionary<string, bool> NewInstruction;

			bool DoLog;
			Action<uint> _ExploreNewPcCallback;

			uint MinPC;
			uint MaxPC;

			uint InstructionsProcessed;

			HashSet<uint> AnalyzedPC;

			/// <summary>
			/// Instructions to SKIP code generation, because they have been grouped in other instruction.
			/// </summary>
			HashSet<uint> SkipPC;

			public void ExploreNewPcCallback(uint PC)
			{
				if (_ExploreNewPcCallback != null) _ExploreNewPcCallback(PC);
			}

			internal InternalFunctionCompiler(InjectContext InjectContext, MipsMethodEmitter MipsMethodEmitter, DynarecFunctionCompiler DynarecFunctionCompiler, IInstructionReader InstructionReader, Action<uint> _ExploreNewPcCallback, uint EntryPC, bool DoLog)
			{
				InjectContext.InjectDependencesTo(this);
				this._ExploreNewPcCallback = _ExploreNewPcCallback;
				this.MipsMethodEmitter = MipsMethodEmitter;
				this.CpuEmitter = new CpuEmitter(InjectContext, MipsMethodEmitter, InstructionReader);
				this.GlobalInstructionStats = CpuProcessor.GlobalInstructionStats;
				this.InstructionStats = MipsMethodEmitter.InstructionStats;
				this.NewInstruction = new Dictionary<string, bool>();
				this.DoLog = DoLog;

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
				var Nodes = GenerateCode();

				Nodes = ast.Statements(
					ast.Comment("Returns immediately when argument CpuThreadState is null, so we can call it on the generation thread to do prelinking."),
					ast.IfElse(
						ast.Binary(MipsMethodEmitter.CpuThreadStateArgument(), "==", ast.Null<CpuThreadState>()),
						ast.Return()
					),
					Nodes
				);

				return new DynarecFunction()
				{
					EntryPC = EntryPC,
					MinPC = MinPC,
					MaxPC = MaxPC,
					AstNode = Nodes,
					Delegate = MipsMethodEmitter.CreateDelegate(Nodes),
				};
			}


			private void LogInstruction(uint PC, Instruction Instruction)
			{
				if (CpuProcessor.CpuConfig.LogInstructionStats)
				{
					var DisassembledInstruction = MipsDisassembler.Disassemble(PC, Instruction);
					//Console.WriteLine("{0}", DisassembledInstruction);
					//var InstructionName = GetInstructionName(CpuEmitter.Instruction.Value, null);
					var InstructionName = DisassembledInstruction.InstructionInfo.Name;

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
			}

			/// <summary>
			/// PASS 1: Analyze Branches
			/// </summary>
			private void AnalyzeBranches()
			{
				SkipPC = new HashSet<uint>();
				AnalyzedPC = new HashSet<uint>();
				var BranchesToAnalyze = new Queue<uint>();

				Labels[EntryPC] = AstLabel.CreateLabel("EntryPoint");

				uint EndPC = (uint)InstructionReader.EndPC;
				PC = EntryPC;
				MinPC = uint.MaxValue;
				MaxPC = uint.MinValue;

				BranchesToAnalyze.Enqueue(EntryPC);

				while (true)
				{
				HandleNewBranch: ;
					bool EndOfBranchFound = false;

					if (BranchesToAnalyze.Count == 0) break;

					for (PC = BranchesToAnalyze.Dequeue(); PC <= EndPC; PC += 4)
					{
						// If already analyzed, stop scanning this branch.
						if (AnalyzedPC.Contains(PC)) break;
						AnalyzedPC.Add(PC);
						//Console.WriteLine("%08X".Sprintf(PC));

						if (AnalyzedPC.Count > MaxNumberOfInstructions)
						{
							throw (new InvalidDataException(String.Format("Code sequence too long: >= {0} at 0x{1:X8}", MaxNumberOfInstructions, EntryPC)));
						}

						MinPC = Math.Min(MinPC, PC);
						MaxPC = Math.Max(MaxPC, PC);

						//Console.WriteLine("    PC:{0:X}", PC);

						var Instruction = InstructionReader[PC];

						var BranchInfo = DynarecBranchAnalyzer.GetBranchInfo(Instruction);
						var DisassemblerInfo = MipsDisassembler.Disassemble(PC, Instruction);

						LogInstruction(PC, Instruction);

						// Break
						if (DisassemblerInfo.InstructionInfo.Name == "break")
						{
							break;
						}
						// Branch instruction.
						//else if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.JumpAlways))
						else if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.JumpInstruction))
						{
							//Console.WriteLine("Instruction");

// This breaks things out!
#if ENABLE_JUMP_GOTO
							var JumpAddress = Instruction.GetJumpAddress(PC);
							if (!LabelsJump.ContainsKey(JumpAddress))
							{
								LabelsJump[JumpAddress] = AstLabel.CreateDelayedWithName(String.Format("0x{0:X8}", JumpAddress));
							}
#endif
							// Located a jump-always instruction with a delayed slot. Process next instruction too.
							if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.AndLink))
							{
								// Just a function call. Continue analyzing.
#if !ENABLE_NATIVE_CALLS
								EndOfBranchFound = true;
								continue;
#endif
							}
							else
							{
								EndOfBranchFound = true;
								continue;
							}
						}
						else if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.BranchOrJumpInstruction))
						{
							var BranchAddress = Instruction.GetBranchAddress(PC);
							//if (!Labels.ContainsKey(BranchAddress))
							{
								Labels[BranchAddress] = AstLabel.CreateLabel(String.Format("0x{0:X8}", BranchAddress));
								BranchesToAnalyze.Enqueue(BranchAddress);
							}
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
							EndOfBranchFound = false;
							goto HandleNewBranch;
						}
					}
				}
			}

			private AstNodeStm ProcessGeneratedInstruction(MipsDisassembler.Result Disasm, AstNodeStm AstNodeStm)
			{
				var PC = Disasm.InstructionPC;
				return ast.Statements(
#if DEBUG_TRACE_INSTRUCTIONS
					ast.DebugWrite(String.Format("0x{0:X8}: {1}", PC, Disasm)),
#endif
					AstNodeStm
				);
			}

			private AstNodeStmPspInstruction _EmitCpuInstructionAT(uint PC)
			{
				// Skip emit instruction.
				if (SkipPC.Contains(PC)) return null;

				/*
				if (CpuProcessor.CpuConfig.TraceJIT)
				{
					SafeILGenerator.LoadArgument<CpuThreadState>(0);
					SafeILGenerator.Push((int)_PC);
					SafeILGenerator.Call((Action<uint>)CpuThreadState.Methods.Trace);
					Console.WriteLine("     PC=0x{0:X}", _PC);
				}
				*/

				var Instruction = CpuEmitter.LoadAT(PC);
				var DisassembleInstruction = MipsDisassembler.Disassemble(PC, Instruction);
				return new AstNodeStmPspInstruction(
					DisassembleInstruction,
					ProcessGeneratedInstruction(DisassembleInstruction, CpuEmitterInstruction(Instruction, CpuEmitter))
				);
			}

			uint PC;
			static private AstGenerator ast = AstGenerator.Instance;

			private AstNodeStm StorePC()
			{
				//MipsMethodEmiter.SavePC(PC);
				//Console.Error.WriteLine("Not implemented!");
				return ast.Statement();
			}

			private AstNodeStm EmitInstructionCountIncrement(bool CheckForYield)
			{
				// CountInstructionsAndYield
				if (!CpuProcessor.CpuConfig.CountInstructionsAndYield)
				{
					return ast.Statement();
				}

				throw (new NotImplementedException("EmitInstructionCountIncrement"));

				////Console.WriteLine("EmiteInstructionCountIncrement: {0},{1}", InstructionsEmitedSinceLastWaypoint, CheckForYield);
				//if (InstructionsEmitedSinceLastWaypoint > 0)
				//{
				//	MipsMethodEmiter.SaveStepInstructionCount(() =>
				//	{
				//		MipsMethodEmiter.LoadStepInstructionCount();
				//		SafeILGenerator.Push((int)InstructionsEmitedSinceLastWaypoint);
				//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned);
				//	});
				//	//ILGenerator.Emit(OpCodes.Ldc_I4, 100);
				//	//ILGenerator.EmitCall(OpCodes.Call, typeof(Console).GetMethod("WriteLine"), new Type[] { typeof(int) });
				//	InstructionsEmitedSinceLastWaypoint = 0;
				//}
				//
				//if (CheckForYield)
				//{
				//	if (!CpuProcessor.CpuConfig.BreakInstructionThreadSwitchingForSpeed)
				//	{
				//		var NoYieldLabel = SafeILGenerator.DefineLabel("NoYieldLabel");
				//		MipsMethodEmiter.LoadStepInstructionCount();
				//		SafeILGenerator.Push((int)0);
				//		SafeILGenerator.BranchBinaryComparison(SafeBinaryComparison.GreaterThanSigned, NoYieldLabel);
				//		//ILGenerator.Emit(OpCodes.Ldc_I4, 1000000);
				//		//ILGenerator.Emit(OpCodes.Blt, NoYieldLabel);
				//		MipsMethodEmiter.SaveStepInstructionCount(() =>
				//		{
				//			SafeILGenerator.Push((int)0);
				//		});
				//		StorePC();
				//		SafeILGenerator.LoadArgument0CpuThreadState();
				//		SafeILGenerator.Call((Action)CpuThreadState.Methods.Yield);
				//		//ILGenerator.Emit(OpCodes.Call, typeof(GreenThread).GetMethod("Yield"));
				//		NoYieldLabel.Mark();
				//	}
				//}
			}

			private void TryPutLabelAT(uint PC, AstNodeStmContainer Nodes)
			{
				// Marks label.
				if (Labels.ContainsKey(PC))
				{
					Nodes.AddStatement(EmitInstructionCountIncrement(false));
					Nodes.AddStatement(ast.Label(Labels[PC]));
					//Labels[PC].Mark();
				}

				// Marks label.
				if (LabelsJump.ContainsKey(PC))
				{
					Nodes.AddStatement(EmitInstructionCountIncrement(false));
					Nodes.AddStatement(ast.Label(LabelsJump[PC]));
					//Labels[PC].Mark();
				}
			}

			public static void IsDebuggerPresentDebugBreak()
			{
				if (Debugger.IsAttached) Debugger.Break();
			}

			private AstNodeStm EmitCpuInstruction()
			{
				try
				{
					var Nodes = ast.Statements();

					if (CpuProcessor.NativeBreakpoints.Contains(PC))
					{
						Nodes.AddStatement(ast.Statement(ast.CallStatic((Action)IsDebuggerPresentDebugBreak)));
					}

					TryPutLabelAT(PC, Nodes);

					Nodes.AddStatement(_EmitCpuInstructionAT(PC));

					return Nodes;
				}
				finally
				{
					PC += 4;
					InstructionsEmitedSinceLastWaypoint++;
				}
			}

			uint InstructionsEmitedSinceLastWaypoint;

			/// <summary>
			/// PASS 2: Generate code and put labels;
			/// </summary>
			private AstNodeStmContainer GenerateCode()
			{
				foreach (var Label in Labels.ToArray())
				{
					if (!(Label.Key >= MinPC && Label.Key <= MaxPC))
					{
						Labels.Remove(Label.Key);
					}
				}
				//AnalyzedPC

				InstructionsEmitedSinceLastWaypoint = 0;

				//Debug.WriteLine("PASS2: MinPC:{0:X}, MaxPC:{1:X}", MinPC, MaxPC);

				// Jumps to the entry point.
				var Nodes = new AstNodeStmContainer();

				Nodes.AddStatement(ast.GotoAlways(Labels[EntryPC]));

				for (PC = MinPC; PC <= MaxPC; )
				{
					uint CurrentInstructionPC = PC;
					Instruction CurrentInstruction = InstructionReader[PC];
					InstructionsProcessed++;

					var BranchInfo = DynarecBranchAnalyzer.GetBranchInfo(CurrentInstruction.Value);

					// Delayed branch instruction.
					if ((BranchInfo & DynarecBranchAnalyzer.JumpFlags.BranchOrJumpInstruction) != 0)
					{
						InstructionsEmitedSinceLastWaypoint += 2;
						Nodes.AddStatement(EmitInstructionCountIncrement(true));

						var BranchAddress = CurrentInstruction.GetBranchAddress(PC);

						if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.JumpInstruction))
						{
							TryPutLabelAT(PC, Nodes);

							var DelayedBranchInstruction = _EmitCpuInstructionAT(PC + 4); // Delayed
							var JumpInstruction = _EmitCpuInstructionAT(PC + 0); // Jump

							// Put delayed instruction first.
							Nodes.AddStatement(DelayedBranchInstruction);

#if true
							var JumpDisasm = JumpInstruction.DisassembledResult;
							var JumpJumpPC = JumpDisasm.Instruction.GetJumpAddress(Memory, JumpDisasm.InstructionPC);
							// An internal jump.
							if (
								(JumpDisasm.InstructionInfo.Name == "j")
								&& (LabelsJump.ContainsKey(JumpJumpPC))
							)
							{
								Nodes.AddStatement(ast.Statements(
									ast.Comment(String.Format("{0:X8}: j 0x{1:X8}", JumpDisasm.InstructionPC, JumpJumpPC)),
									ast.GotoAlways(LabelsJump[JumpJumpPC])
								));
							}
							// A jump outside the current function.
							else
#endif
							{
								Nodes.AddStatement(JumpInstruction);
							}
							PC += 8;
						}
						else
						{
							// Branch instruction.
							Nodes.AddStatement(EmitCpuInstruction());

							//if ((BranchInfo & CpuBranchAnalyzer.Flags.Likely) != 0)
							if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.Likely))
							{
								//Console.WriteLine("Likely");
								// Delayed instruction.
								Nodes.AddStatement(CpuEmitter._branch_likely(EmitCpuInstruction()));
							}
							else
							{
								//Console.WriteLine("Not Likely");
								// Delayed instruction.
								Nodes.AddStatement(EmitCpuInstruction());
							}

							if (CurrentInstructionPC + 4 != BranchAddress)
							{
								if (Labels.ContainsKey(BranchAddress))
								{
									Nodes.AddStatement(CpuEmitter._branch_post(Labels[BranchAddress], BranchAddress));
								}
								// Code not reached.
								else
								{
								}
							}
						}
					}
					// Normal instruction.
					else
					{
						// Syscall instruction.
						if ((BranchInfo & DynarecBranchAnalyzer.JumpFlags.SyscallInstruction) != 0)
						{
							Nodes.AddStatement(StorePC());
						}
						Nodes.AddStatement(EmitCpuInstruction());
						if ((BranchInfo & DynarecBranchAnalyzer.JumpFlags.SyscallInstruction) != 0)
						{
							// On this special Syscall
							if (CurrentInstruction.CODE == SyscallInfo.NativeCallSyscallCode)
							{
								//PC += 4;
								break;
							}
						}
					}
				}

				//MipsMethodEmiter.GenerateIL(Nodes);
				//ShowInstructionStats();

				//if (BreakPoint) IsDebuggerPresentDebugBreak();

				return Nodes;
			}

			/*
			private void ShowInstructionStats()
			{
				if (CpuProcessor.CpuConfig.ShowInstructionStats)
				{
					bool HasNew = false;
					foreach (var Pair in InstructionStats.OrderByDescending(Item => Item.Value))
					{
						if (NewInstruction.ContainsKey(Pair.Key))
						{
							HasNew = true;
						}
					}

					if (!CpuProcessor.CpuConfig.ShowInstructionStatsJustNew || HasNew)
					{
						Console.Error.WriteLine("-------------------------- {0:X}-{1:X} ", MinPC, MaxPC);
						foreach (var Pair in InstructionStats.OrderByDescending(Item => Item.Value))
						{
							var IsNew = NewInstruction.ContainsKey(Pair.Key);
							if (!CpuProcessor.CpuConfig.ShowInstructionStatsJustNew || IsNew)
							{
								Console.Error.Write("{0} : {1}", Pair.Key, Pair.Value);
								if (IsNew) Console.Error.Write(" <-- NEW!");
								Console.Error.WriteLine("");
							}
						}
					}
				}

				if (DoLog)
				{
					Console.WriteLine("----------------------------");
					foreach (var Instruction in MipsMethodEmiter.SafeILGenerator.GetEmittedInstructions()) Console.WriteLine(Instruction);
				}
			}
			*/
		}
	}
}
