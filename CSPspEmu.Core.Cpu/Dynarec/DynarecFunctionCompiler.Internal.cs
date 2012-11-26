#define OPTIMIZE_LWL_LWR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SafeILGenerator;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu.Emitter;
using CSPspEmu.Core.Cpu.Table;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast;

namespace CSPspEmu.Core.Cpu.Dynarec
{
    public partial class DynarecFunctionCompiler
	{
        internal class InternalFunctionCompiler
		{
			public static Func<uint, CpuEmitter, AstNodeStm> CpuEmitterInstruction = EmitLookupGenerator.GenerateSwitchDelegateReturn<CpuEmitter, AstNodeStm>(InstructionTable.ALL);
			static MipsDisassembler MipsDisassembler = new MipsDisassembler();
			CpuEmitter CpuEmitter;
			MipsMethodEmitter MipsMethodEmiter;
			SafeILGeneratorEx SafeILGenerator;
			DynarecFunctionCompiler DynarecFunctionCompiler;
			IInstructionReader InstructionReader;
			CpuProcessor CpuProcessor;
			uint EntryPC;
			SortedDictionary<uint, SafeLabel> Labels = new SortedDictionary<uint, SafeLabel>();

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

			internal InternalFunctionCompiler(CpuProcessor CpuProcessor, MipsMethodEmitter MipsMethodEmiter, DynarecFunctionCompiler DynarecFunctionCompiler, IInstructionReader InstructionReader, Action<uint> _ExploreNewPcCallback, uint EntryPC, bool DoLog)
			{
				this._ExploreNewPcCallback = _ExploreNewPcCallback;
				this.CpuEmitter = new CpuEmitter(MipsMethodEmiter, InstructionReader, CpuProcessor);
				this.MipsMethodEmiter = MipsMethodEmiter;
				this.GlobalInstructionStats = CpuProcessor.GlobalInstructionStats;
				this.InstructionStats = MipsMethodEmiter.InstructionStats;
				this.SafeILGenerator = MipsMethodEmiter.SafeILGenerator;
				this.NewInstruction = new Dictionary<string, bool>();
				this.DoLog = DoLog;

				this.CpuProcessor = CpuProcessor;
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
				GenerateCode();
				return new DynarecFunction()
				{
					Delegate = MipsMethodEmiter.CreateDelegate(),
				};
			}


			private void LogInstruction(uint PC, Instruction Instruction)
			{
				if (CpuProcessor.PspConfig.LogInstructionStats)
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

				Labels[EntryPC] = SafeILGenerator.DefineLabel("EntryPoint");

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

						LogInstruction(PC, Instruction);

						// Branch instruction.
						if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.JumpInstruction))
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
							EndOfBranchFound = false;
							goto HandleNewBranch;
						}
					}
				}
			}

			private AstNodeStm _EmitCpuInstructionAT(uint _PC)
			{
				// Skip emit instruction.
				if (SkipPC.Contains(_PC)) return null;

				if (CpuProcessor.PspConfig.TraceJIT)
				{
					SafeILGenerator.LoadArgument<CpuThreadState>(0);
					SafeILGenerator.Push((int)_PC);
					SafeILGenerator.Call((Action<uint>)CpuThreadState.Methods.Trace);
					Console.WriteLine("     PC=0x{0:X}", _PC);
				}

				var Instruction = CpuEmitter.LoadAT(_PC);
//#if OPTIMIZE_LWL_LWR
//				var InstructionDisasm = MipsDisassembler.Disassemble(_PC, Instruction);
//
//				if (InstructionDisasm.InstructionInfo != null && InstructionDisasm.InstructionInfo.Name == "lwl")
//				{
//					// set: RT
//					// get: RT, RS
//					var lwl_rt = Instruction.RT;
//					var lwl_rs = Instruction.RS;
//					var lwl_offset = Instruction.IMM;
//
//					for (int n = 1; n < 16; n++)
//					{
//						var PC2 = (uint)(_PC + n * 4);
//
//						// A label between!
//						if (Labels.ContainsKey(PC2))
//						{
//							//Console.WriteLine("Label!");
//							break;
//						}
//						if (!AnalyzedPC.Contains(PC2))
//						{
//							//Console.WriteLine("Not analyzed!");
//							break;
//						}
//
//						var Instruction2 = CpuEmitter.LoadAT(PC2);
//						var Instruction2Disasm = MipsDisassembler.Disassemble(PC2, Instruction2);
//
//						//Console.WriteLine(Instruction2Disasm);
//
//						// TODO: Check if other instructions modify RT or RS register!!
//
//						if (Instruction2Disasm.InstructionInfo != null && Instruction2Disasm.InstructionInfo.Name == "lwr")
//						{
//							var lwr_rt = Instruction2.RT;
//							var lwr_rs = Instruction2.RS;
//							var lwr_offset = Instruction2.IMM;
//
//							if ((lwl_rt == lwr_rt) && (lwl_rs == lwr_rs) && (lwr_offset == lwl_offset - 3))
//							{
//								//Console.WriteLine("Found it!");
//
//								// FOUND IT!
//								SkipPC.Add(PC2);
//
//								// Emit Unaligned LW 
//								//SafeILGenerator.emi
//								Instruction.RT = lwr_rt;
//								Instruction.RS = lwr_rs;
//								Instruction.IMM = lwr_offset;
//								CpuEmitter.lw();
//								//CpuEmiterInstruction(CpuEmitter.Instruction.Value, CpuEmitter);
//								return null;
//							}
//						}
//					}
//				}
//#endif

				Instruction = CpuEmitter.LoadAT(_PC);
				var Node = CpuEmitterInstruction(CpuEmitter.Instruction, CpuEmitter);
				if (Node == null)
				{
					//var InstructionDisasm = MipsDisassembler.Disassemble(_PC, Instruction);
					throw (new Exception(String.Format("Can't execute instruction {0}", MipsDisassembler.Disassemble(_PC, Instruction))));
				}
				return Node;
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
				if (!CpuProcessor.PspConfig.CountInstructionsAndYield)
				{
					return ast.Statement();
				}

				throw(new NotImplementedException());

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
				//	if (!CpuProcessor.PspConfig.BreakInstructionThreadSwitchingForSpeed)
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

			private AstNodeStm EmitCpuInstruction()
			{
				try
				{
					if (CpuProcessor.NativeBreakpoints.Contains(PC))
					{
						SafeILGenerator.Call((Action)DebugUtils.IsDebuggerPresentDebugBreak);
					}

					var Nodes = ast.Statements();

					// Marks label.
					if (Labels.ContainsKey(PC))
					{
						Nodes.AddStatement(EmitInstructionCountIncrement(false));
						Nodes.AddStatement(ast.Label(AstLabel.CreateFromLabel(Labels[PC])));
						//Labels[PC].Mark();
					}

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

			private void CheckBreakpoint()
			{
				/*
				if (PC >= 0x887F500 && PC <= 0x887F600)
				{
					MipsMethodEmiter.CallMethodWithCpuThreadStateAsFirstArgument(typeof(CpuProcessor), "DebugCurrentThread");
					//Console.WriteLine("Reached Debug!");
					//Console.ReadKey();
				}
				*/
				/*
				if (PC >= 0x881936C && PC <= 0x881936C)
				{
					MipsMethodEmiter.CallMethodWithCpuThreadStateAsFirstArgument(typeof(CpuProcessor), "DebugCurrentThread");
				}
				*/


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
			}

			/// <summary>
			/// PASS 2: Generate code and put labels;
			/// </summary>
			private void GenerateCode()
			{
				InstructionsEmitedSinceLastWaypoint = 0;

				//Debug.WriteLine("PASS2: MinPC:{0:X}, MaxPC:{1:X}", MinPC, MaxPC);

				// Jumps to the entry point.
				SafeILGenerator.BranchAlways(Labels[EntryPC]);

				var Nodes = new AstNodeStmContainer();

				for (PC = MinPC; PC <= MaxPC; )
				{
					uint CurrentInstructionPC = PC;
					Instruction CurrentInstruction = InstructionReader[PC];
					InstructionsProcessed++;

					CheckBreakpoint();

					var BranchInfo = DynarecBranchAnalyzer.GetBranchInfo(CurrentInstruction.Value);

					// Delayed branch instruction.
					if ((BranchInfo & DynarecBranchAnalyzer.JumpFlags.BranchOrJumpInstruction) != 0)
					{
						InstructionsEmitedSinceLastWaypoint += 2;
						Nodes.AddStatement(EmitInstructionCountIncrement(true));

						var BranchAddress = CurrentInstruction.GetBranchAddress(PC);

						if (BranchInfo.HasFlag(DynarecBranchAnalyzer.JumpFlags.JumpInstruction))
						{
							// Marks label.
							if (Labels.ContainsKey(PC))
							{
								Nodes.AddStatement(ast.Label(AstLabel.CreateFromLabel(Labels[PC])));
								//Labels[PC].Mark();
							}

							Nodes.AddStatement(_EmitCpuInstructionAT(PC + 4));
							Nodes.AddStatement(_EmitCpuInstructionAT(PC + 0));
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
									Nodes.AddStatement(CpuEmitter._branch_post(Labels[BranchAddress]));
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

				MipsMethodEmiter.GenerateIL(Nodes);
				ShowInstructionStats();

				//if (BreakPoint) IsDebuggerPresentDebugBreak();
			}

			private void ShowInstructionStats()
			{
				if (CpuProcessor.PspConfig.ShowInstructionStats)
				{
					bool HasNew = false;
					foreach (var Pair in InstructionStats.OrderByDescending(Item => Item.Value))
					{
						if (NewInstruction.ContainsKey(Pair.Key))
						{
							HasNew = true;
						}
					}

					if (!CpuProcessor.PspConfig.ShowInstructionStatsJustNew || HasNew)
					{
						Console.Error.WriteLine("-------------------------- {0:X}-{1:X} ", MinPC, MaxPC);
						foreach (var Pair in InstructionStats.OrderByDescending(Item => Item.Value))
						{
							var IsNew = NewInstruction.ContainsKey(Pair.Key);
							if (!CpuProcessor.PspConfig.ShowInstructionStatsJustNew || IsNew)
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
		}
	}
}
