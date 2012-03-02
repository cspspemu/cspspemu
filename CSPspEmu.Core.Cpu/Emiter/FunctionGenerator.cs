using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSPspEmu.Core.Cpu.Emiter;
using System.Reflection.Emit;
using CSPspEmu.Core.Cpu.Table;
using CSPspEmu.Core.Memory;
using CSharpUtils.Extensions;
using Codegen;

namespace CSPspEmu.Core.Cpu.Emiter
{
	/// <summary>
	/// 
	/// </summary>
	/// <see cref="http://msdn.microsoft.com/en-us/library/ms973852.aspx"/>
	/// <see cref="http://stackoverflow.com/questions/8263146/ilgenerator-how-to-use-unmanaged-pointers-i-get-a-verificationexception"/>
	/// <see cref="http://srstrong.blogspot.com/2008/09/unsafe-code-without-unsafe-keyword.html"/>
	/*
	DynamicMethod M = new DynamicMethod(
		"HiThere",
		ReturnType, Args,
		Assembly.GetExecutingAssembly().ManifestModule
	);
	*/
	public class FunctionGenerator
	{
		static public MipsEmiter MipsEmiter = new MipsEmiter();
		static public Action<uint, CpuEmiter> CpuEmiterInstruction = EmitLookupGenerator.GenerateSwitchDelegate<CpuEmiter>(InstructionTable.ALL);
		static public Func<uint, CpuBranchAnalyzer.Flags> GetBranchInfo = EmitLookupGenerator.GenerateInfoDelegate<CpuBranchAnalyzer, CpuBranchAnalyzer.Flags>(
			EmitLookupGenerator.GenerateSwitchDelegateReturn<CpuBranchAnalyzer, CpuBranchAnalyzer.Flags>(
				InstructionTable.ALL, ThrowOnUnexistent: false
			),
			new CpuBranchAnalyzer()
		);
		static public Func<uint, Object, String> GetInstructionName = EmitLookupGenerator.GenerateSwitch<Func<uint, Object, String>>(
			InstructionTable.ALL,
			(SafeILGenerator, InstructionInfo) =>
			{
				SafeILGenerator.Push((string)((InstructionInfo != null) ? InstructionInfo.Name : "unknown"));
			}
		);

		public const ushort NativeCallSyscallCode = 0x1234;

		static public uint NativeCallSyscallOpCode
		{
			get
			{
				return (uint)(0x0000000C | (FunctionGenerator.NativeCallSyscallCode << 6));
			}
		}

		static public PspMethodStruct CreateDelegateForPC(CpuProcessor CpuProcessor, Stream MemoryStream, uint EntryPC)
		{
			DateTime Start, End;
			int InstructionsProcessed = 0;
			Start = DateTime.UtcNow;
			try
			{
				return _CreateDelegateForPC(CpuProcessor, MemoryStream, EntryPC, out InstructionsProcessed);
			}
			finally
			{
				End = DateTime.UtcNow;
				//Console.WriteLine("Generated 0x{0:X} {1} ({2})", EntryPC, End - Start, InstructionsProcessed);
			}
		}

		static private PspMethodStruct _CreateDelegateForPC(CpuProcessor CpuProcessor, Stream MemoryStream, uint EntryPC, out int InstructionsProcessed)
		{
			InstructionsProcessed = 0;

			if (EntryPC == 0)
			{
				if (MemoryStream is PspMemoryStream)
				{
					throw (new InvalidOperationException("EntryPC can't be NULL"));
				}
			}

			if (CpuProcessor.PspConfig.TraceJIT)
			{
				Console.WriteLine("Emiting EntryPC=0x{0:X}", EntryPC);
			}

			MemoryStream.Position = EntryPC;
			if ((MemoryStream.Length >= 8) && new BinaryReader(MemoryStream).ReadUInt64() == 0x0000000003E00008)
			{
				Console.WriteLine("NullSub detected at 0x{0:X}!", EntryPC);
			}

			var InstructionReader = new InstructionReader(MemoryStream);
			var MipsMethodEmiter = new MipsMethodEmiter(MipsEmiter, CpuProcessor, EntryPC);
			var SafeILGenerator = MipsMethodEmiter.SafeILGenerator;
			var CpuEmiter = new CpuEmiter(MipsMethodEmiter, InstructionReader, MemoryStream, CpuProcessor);

			uint PC;
			uint EndPC = (uint)MemoryStream.Length;
			uint MinPC = uint.MaxValue, MaxPC = uint.MinValue;

			var Labels = new SortedDictionary<uint, SafeLabel>();
			var BranchesToAnalyze = new Queue<uint>();
			var AnalyzedPC = new HashSet<uint>();

			Labels[EntryPC] = SafeILGenerator.DefineLabel("EntryPoint");

			BranchesToAnalyze.Enqueue(EntryPC);

			// PASS1: Analyze and find labels.
			PC = EntryPC;
			//Debug.WriteLine("PASS1: (PC={0:X}, EndPC={1:X})", PC, EndPC);

			var GlobalInstructionStats = CpuProcessor.GlobalInstructionStats;
			var InstructionStats = MipsMethodEmiter.InstructionStats;
			var NewInstruction = new Dictionary<string, bool>();

			//int MaxNumberOfInstructions = 8 * 1024;
			int MaxNumberOfInstructions = 64 * 1024;
			//int MaxNumberOfInstructions = 60;

			while (BranchesToAnalyze.Count > 0)
			{
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

					CpuEmiter.LoadAT(PC);

					var BranchInfo = GetBranchInfo(CpuEmiter.Instruction.Value);
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
						Labels[BranchAddress] = SafeILGenerator.DefineLabel("" + BranchAddress);
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
					else if ((BranchInfo & CpuBranchAnalyzer.Flags.SyscallInstruction) != 0)
					{
						// On this special Syscall
						if (CpuEmiter.Instruction.CODE == FunctionGenerator.NativeCallSyscallCode)
						{
							//PC += 4;
							break;
						}
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
				if (CpuProcessor.PspConfig.TraceJIT)
				{
					SafeILGenerator.LoadArgument<CpuThreadState>(0);
					SafeILGenerator.Push((int)_PC);
					SafeILGenerator.Call(typeof(CpuThreadState).GetMethod("Trace"));
					Console.WriteLine("     PC=0x{0:X}", _PC);
				}

				CpuEmiter.LoadAT(_PC);
				CpuEmiterInstruction(CpuEmiter.Instruction.Value, CpuEmiter);
			};

			uint InstructionsEmitedSinceLastWaypoint = 0;

			Action StorePC = () =>
			{
				MipsMethodEmiter.SavePC(PC);
			};

			Action<bool> EmitInstructionCountIncrement = (bool CheckForYield) =>
			{
				if (!CpuProcessor.PspConfig.CountInstructionsAndYield)
				{
					return;
				}

				//Console.WriteLine("EmiteInstructionCountIncrement: {0},{1}", InstructionsEmitedSinceLastWaypoint, CheckForYield);
				if (InstructionsEmitedSinceLastWaypoint > 0)
				{
					MipsMethodEmiter.SaveStepInstructionCount(() =>
					{
						MipsMethodEmiter.LoadStepInstructionCount();
						SafeILGenerator.Push((int)InstructionsEmitedSinceLastWaypoint);
						SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned);
					});
					//ILGenerator.Emit(OpCodes.Ldc_I4, 100);
					//ILGenerator.EmitCall(OpCodes.Call, typeof(Console).GetMethod("WriteLine"), new Type[] { typeof(int) });
					InstructionsEmitedSinceLastWaypoint = 0;
				}

				if (CheckForYield)
				{
					if (!CpuProcessor.PspConfig.BreakInstructionThreadSwitchingForSpeed)
					{
						var NoYieldLabel = SafeILGenerator.DefineLabel("NoYieldLabel");
						MipsMethodEmiter.LoadStepInstructionCount();
						SafeILGenerator.Push((int)0);
						SafeILGenerator.BranchBinaryComparison(SafeBinaryComparison.GreaterThanSigned, NoYieldLabel);
						//ILGenerator.Emit(OpCodes.Ldc_I4, 1000000);
						//ILGenerator.Emit(OpCodes.Blt, NoYieldLabel);
						MipsMethodEmiter.SaveStepInstructionCount(() =>
						{
							SafeILGenerator.Push((int)0);
						});
						StorePC();
						SafeILGenerator.LoadArgument0CpuThreadState();
						SafeILGenerator.Call((Action)CpuThreadState.Methods.Yield);
						//ILGenerator.Emit(OpCodes.Call, typeof(GreenThread).GetMethod("Yield"));
						NoYieldLabel.Mark();
					}
				}
			};

			Action EmitCpuInstruction = () =>
			{
				if (CpuProcessor.NativeBreakpoints.Contains(PC))
				{
					SafeILGenerator.Call((Action)DebugUtils.IsDebuggerPresentDebugBreak);
				}

				// Marks label.
				if (Labels.ContainsKey(PC))
				{
					EmitInstructionCountIncrement(false);
					Labels[PC].Mark();
				}

				_EmitCpuInstructionAT(PC);
				PC += 4;
				InstructionsEmitedSinceLastWaypoint++;
			};

			//Debug.WriteLine("PASS2: MinPC:{0:X}, MaxPC:{1:X}", MinPC, MaxPC);

			// Jumps to the entry point.
			SafeILGenerator.BranchAlways(Labels[EntryPC]);

			for (PC = MinPC; PC <= MaxPC; )
			{
				uint CurrentInstructionPC = PC;
				Instruction CurrentInstruction = InstructionReader[PC];
				InstructionsProcessed++;

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

				var BranchInfo = GetBranchInfo(CurrentInstruction.Value);

				// Delayed branch instruction.
				if ((BranchInfo & CpuBranchAnalyzer.Flags.BranchOrJumpInstruction) != 0)
				{
					InstructionsEmitedSinceLastWaypoint += 2;
					EmitInstructionCountIncrement(true);

					var BranchAddress = CurrentInstruction.GetBranchAddress(PC);

					if ((BranchInfo & CpuBranchAnalyzer.Flags.JumpInstruction) != 0)
					{
						// Marks label.
						if (Labels.ContainsKey(PC))
						{
							Labels[PC].Mark();
						}

						_EmitCpuInstructionAT(PC + 4);
						_EmitCpuInstructionAT(PC + 0);
						PC += 8;
					}
					else
					{
						// Branch instruction.
						EmitCpuInstruction();

						//if ((BranchInfo & CpuBranchAnalyzer.Flags.Likely) != 0)
						if (BranchInfo.HasFlag(CpuBranchAnalyzer.Flags.Likely))
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
							if (Labels.ContainsKey(BranchAddress))
							{
								CpuEmiter._branch_post(Labels[BranchAddress]);
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
					if ((BranchInfo & CpuBranchAnalyzer.Flags.SyscallInstruction) != 0)
					{
						StorePC();
					}
					EmitCpuInstruction();
					if ((BranchInfo & CpuBranchAnalyzer.Flags.SyscallInstruction) != 0)
					{
						// On this special Syscall
						if (CurrentInstruction.CODE == FunctionGenerator.NativeCallSyscallCode)
						{
							//PC += 4;
							break;
						}
					}
				}
			}


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

			//if (BreakPoint) IsDebuggerPresentDebugBreak();
			Action<CpuThreadState> Delegate = MipsMethodEmiter.CreateDelegate();

			return new PspMethodStruct()
			{
				Delegate = Delegate,
				MipsMethodEmiter = MipsMethodEmiter,
			};
		}
	}
}
