//#define DEBUG_TRACE_INSTRUCTIONS
//#define DISABLE_JUMP_GOTO

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
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Dynarec
{
	public partial class DynarecFunctionCompiler
	{
        internal class InternalFunctionCompiler
		{
			public static readonly Func<uint, CpuEmitter, AstNodeStm> CpuEmitterInstruction = EmitLookupGenerator.GenerateSwitchDelegateReturn<CpuEmitter, AstNodeStm>("CpuEmitterInstruction", InstructionTable.ALL);
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

			//const int MaxNumberOfInstructions = 3000;
			const int MaxNumberOfInstructions = 32 * 1024;
			//const int MaxNumberOfInstructions = 64 * 1024;
			//const int MaxNumberOfInstructions = 128 * 1024;
			//const int MaxNumberOfInstructions = 60;

			Dictionary<string, uint> GlobalInstructionStats;
			Dictionary<string, uint> InstructionStats;
			Dictionary<string, bool> NewInstruction;

			bool DoLog;
			Action<uint> _ExploreNewPcCallback;

			uint MinPC;
			uint MaxPC;

			public List<uint> CallingPCs = new List<uint>();

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
				//this.InstructionStats = MipsMethodEmitter.InstructionStats;
				this.InstructionStats = new Dictionary<string, uint>();
				this.NewInstruction = new Dictionary<string, bool>();
				this.DoLog = DoLog;

				this.DynarecFunctionCompiler = DynarecFunctionCompiler;
				this.InstructionReader = InstructionReader;
				this.EntryPC = EntryPC;

				if (!PspMemory.IsAddressValid(EntryPC))
				{
					throw (new InvalidOperationException(String.Format("Trying to get invalid function 0x{0:X8}", EntryPC)));
				}
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			internal DynarecFunction CreateFunction()
			{
				CpuEmitter.SpecialName = "";
				
				var Time0 = DateTime.UtcNow;
				
				AnalyzeBranches();
				
				var Time1 = DateTime.UtcNow;

				var Nodes = GenerateCode();

				Nodes = ast.Statements(
					ast.Comment(String.Format("Function {0:X8}-{1:X8}. Entry: {2:X8}", MinPC, MaxPC, EntryPC)),
					//ast.DebugWrite(String.Format("Dynarec:PC:{0:X8}", EntryPC)),
					//ast.Comment("Returns immediately when argument CpuThreadState is null, so we can call it on the generation thread to do prelinking."),
					ast.If(
						ast.Binary(ast.CpuThreadState, "==", ast.Null<CpuThreadState>()),
						ast.Return()
					),
					//ast.Statement(ast.CallInstance(ast.Argument<CpuThreadState>(0, "CpuThreadState"), (Action<uint>)CpuThreadState.Methods._DumpRegistersCpu, EntryPC)),
					Nodes
				);

				var Time2 = DateTime.UtcNow;

				//var MipsMethodEmitterResult = MipsMethodEmitter.CreateDelegate(Nodes, (int)((MaxPC - MinPC) / 4));
				var MipsMethodEmitterResult = MipsMethodEmitter.CreateDelegate(Nodes, AnalyzedPC.Count);

				return new DynarecFunction()
				{
					Name = CpuEmitter.SpecialName,
					CallingPCs = CallingPCs,
					EntryPC = EntryPC,
					MinPC = MinPC,
					MaxPC = MaxPC,
					AstNode = Nodes,
					DisableOptimizations = MipsMethodEmitterResult.DisableOptimizations,
					Delegate = MipsMethodEmitterResult.Delegate,
					InstructionStats = InstructionStats,
					TimeOptimize = MipsMethodEmitterResult.TimeOptimize,
					TimeGenerateIL = MipsMethodEmitterResult.TimeGenerateIL,
					TimeCreateDelegate = MipsMethodEmitterResult.TimeCreateDelegate,
					TimeAnalyzeBranches = Time1 - Time0,
					TimeGenerateAst = Time2 - Time1,
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

			private bool AddressInsideFunction(uint PC)
			{
				return PC >= MinPC && PC <= MaxPC;
			}

			private void UpdateMinMax(uint PC)
			{
				MinPC = Math.Min(MinPC, PC);
				MaxPC = Math.Max(MaxPC, PC);
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

						UpdateMinMax(PC);

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

							var JumpAddress = Instruction.GetJumpAddress(Memory, PC);

							// Located a jump-always instruction with a delayed slot. Process next instruction too.
							if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.AndLink))
							{
								// Just a function call. Continue analyzing.
							}
							else
							{
								if (PspMemory.IsAddressValid(JumpAddress))
								{
									if (!LabelsJump.ContainsKey(JumpAddress))
									{
										if (AddressInsideFunction(JumpAddress))
										{
											//Console.WriteLine("JumpAddress: {0:X8}", JumpAddress);
											LabelsJump[JumpAddress] = AstLabel.CreateLabel(String.Format("Jump_0x{0:X8}", JumpAddress));
											BranchesToAnalyze.Enqueue(JumpAddress);
										}
									}
								}

								EndOfBranchFound = true;
								continue;
							}
						}
						else if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.BranchOrJumpInstruction))
						{
							var BranchAddress = Instruction.GetBranchAddress(PC);
							if (!Labels.ContainsKey(BranchAddress))
							{
								//Console.WriteLine("BranchAddress: {0:X8}", BranchAddress);
								UpdateMinMax(BranchAddress);
								Labels[BranchAddress] = AstLabel.CreateLabel(String.Format("Label_0x{0:X8}", BranchAddress));
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

				//Console.WriteLine("FunctionSegment({0:X8}-{1:X8})", MinPC, MaxPC);

				foreach (var LabelAddress in LabelsJump.Keys.ToArray())
				{
					if (!AddressInsideFunction(LabelAddress))
					{
						LabelsJump.Remove(LabelAddress);
					}
				}

				this.CpuEmitter.BranchCount = Labels.Count;
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

			private AstNodeStmPspInstruction _GetAstCpuInstructionAT(uint PC)
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
				var Call = CpuEmitterInstruction(Instruction, CpuEmitter);
				var AstNodeStm = ProcessGeneratedInstruction(DisassembleInstruction, Call);
				return new AstNodeStmPspInstruction(DisassembleInstruction, AstNodeStm);
			}

			uint PC;
			static private AstMipsGenerator ast = AstMipsGenerator.Instance;

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
				if (Labels.ContainsKey(PC))
				{
					Nodes.AddStatement(EmitInstructionCountIncrement(false));
					Nodes.AddStatement(ast.Label(Labels[PC]));
				}
				if (LabelsJump.ContainsKey(PC)) Nodes.AddStatement(ast.Label(LabelsJump[PC]));
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

					if (_DynarecConfig.UpdatePCEveryInstruction)
					{
						Nodes.AddStatement(ast.AssignPC(PC));
					}

					Nodes.AddStatement(_GetAstCpuInstructionAT(PC));

					return Nodes;
				}
				finally
				{
					PC += 4;
					InstructionsEmitedSinceLastWaypoint++;
				}
			}

			uint InstructionsEmitedSinceLastWaypoint;
			private TimeSpan CreateDelegateTime;
			private TimeSpan GenerateILTime;

			//static int DummyTempCounter = 0;

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
					if (!AnalyzedPC.Contains(PC))
					{
						PC += 4;
						continue;
					}
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

							var DelayedBranchInstruction = _GetAstCpuInstructionAT(PC + 4); // Delayed
							var JumpInstruction = _GetAstCpuInstructionAT(PC + 0); // Jump

#if !DISABLE_JUMP_GOTO
							var JumpInstruction2 = CpuEmitter.LoadAT(PC + 0);
							var JumpDisasm = MipsDisassembler.Disassemble(PC + 0, JumpInstruction2);
							var JumpJumpPC = JumpDisasm.Instruction.GetJumpAddress(Memory, JumpDisasm.InstructionPC);
							
							// An internal jump.
							if (
								(JumpDisasm.InstructionInfo.Name == "j")
								&& (LabelsJump.ContainsKey(JumpJumpPC))
							)
							{
								JumpInstruction = new AstNodeStmPspInstruction(JumpDisasm, ast.GotoAlways(LabelsJump[JumpJumpPC]));

								//Console.WriteLine(
								//	"{0}: {1} : Function({2:X8}-{3:X8})",
								//	DummyTempCounter,
								//	GeneratorCSharpPsp.GenerateString<GeneratorCSharpPsp>(AstOptimizerPsp.GlobalOptimize(CpuProcessor, JumpInstruction)),
								//	MinPC, MaxPC
								//);

								//DummyTempCounter++;
							}
							else if (JumpDisasm.InstructionInfo.Name == "j" || JumpDisasm.InstructionInfo.Name == "jal")
							{
								CallingPCs.Add(JumpJumpPC);
							}
#endif

							// Put delayed instruction first.
							Nodes.AddStatement(DelayedBranchInstruction);
							// A jump outside the current function.
							Nodes.AddStatement(JumpInstruction);

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
									throw (new InvalidOperationException("!Labels.ContainsKey(BranchAddress)"));
								}
							}
							else
							{
								throw (new InvalidOperationException("Invalid branch!"));
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
				ShowInstructionStats();

				//if (BreakPoint) IsDebuggerPresentDebugBreak();

				return Nodes;
			}

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
						ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.White, () =>
						{
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
						});
					}
				}

				//if (DoLog)
				//{
				//	Console.WriteLine("----------------------------");
				//	foreach (var Instruction in MipsMethodEmiter.SafeILGenerator.GetEmittedInstructions()) Console.WriteLine(Instruction);
				//}
			}
		}
	}
}
