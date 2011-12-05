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
		static public Func<uint, Object, String> GetInstructionName = EmitLookupGenerator.GenerateSwitchDelegateReturn<Object, String>(
			InstructionTable.ALL,
			(ILGenerator, InstructionInfo) =>
			{
				ILGenerator.Emit(OpCodes.Ldstr, (InstructionInfo != null) ? InstructionInfo.Name : "unknown");
			}
		);

		public const ushort NativeCallSyscallCode = 0x1234;

		static public Action<CpuThreadState> CreateDelegateForPC(CpuProcessor CpuProcessor, Stream MemoryStream, uint EntryPC)
		{
			DateTime Start, End;
			int InstructionsProcessed = 0;
			Start = DateTime.Now;
			try
			{
				return _CreateDelegateForPC(CpuProcessor, MemoryStream, EntryPC, out InstructionsProcessed);
			}
			finally
			{
				End = DateTime.Now;
				//Console.WriteLine("Generated 0x{0:X} {1} ({2})", EntryPC, End - Start, InstructionsProcessed);
			}
		}

		static private Action<CpuThreadState> _CreateDelegateForPC(CpuProcessor CpuProcessor, Stream MemoryStream, uint EntryPC, out int InstructionsProcessed)
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
			var MipsMethodEmiter = new MipsMethodEmiter(MipsEmiter, CpuProcessor);
			var ILGenerator = MipsMethodEmiter.ILGenerator;
			var CpuEmiter = new CpuEmiter(MipsMethodEmiter, InstructionReader, MemoryStream, CpuProcessor);

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
			//Debug.WriteLine("PASS1: (PC={0:X}, EndPC={1:X})", PC, EndPC);

			var GlobalInstructionStats = CpuProcessor.GlobalInstructionStats;
			var InstructionStats = new Dictionary<string, uint>();
			var NewInstruction = new Dictionary<string, bool>();

			int MaxNumberOfInstructions = 8 * 1024;
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
					if (CpuProcessor.PspConfig.ShowInstructionStats)
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
					ILGenerator.Emit(OpCodes.Ldarg_0);
					ILGenerator.Emit(OpCodes.Ldc_I4, _PC);
					ILGenerator.Emit(OpCodes.Call, typeof(CpuThreadState).GetMethod("Trace"));
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
					if (!CpuProcessor.PspConfig.BreakInstructionThreadSwitchingForSpeed)
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
				}
			};

			Action EmitCpuInstruction = () =>
			{
				if (CpuProcessor.NativeBreakpoints.Contains(PC))
				{
					ILGenerator.Emit(OpCodes.Call, typeof(DebugUtils).GetMethod("IsDebuggerPresentDebugBreak"));
				}

				// Marks label.
				if (Labels.ContainsKey(PC))
				{
					EmitInstructionCountIncrement(false);
					ILGenerator.MarkLabel(Labels[PC]);
				}

				_EmitCpuInstructionAT(PC);
				PC += 4;
				InstructionsEmitedSinceLastWaypoint++;
			};

			//Debug.WriteLine("PASS2: MinPC:{0:X}, MaxPC:{1:X}", MinPC, MaxPC);

			// Jumps to the entry point.
			ILGenerator.Emit(OpCodes.Br, Labels[EntryPC]);

			for (PC = MinPC; PC <= MaxPC; )
			{
				uint CurrentInstructionPC = PC;
				Instruction CurrentInstruction = InstructionReader[PC];
				InstructionsProcessed++;

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
				Console.Error.WriteLine("-------------------------- {0:X}-{1:X} ", MinPC, MaxPC);
				foreach (var Pair in InstructionStats.OrderByDescending(Item => Item.Value))
				{
					Console.Error.Write("{0} : {1}", Pair.Key, Pair.Value);
					if (NewInstruction.ContainsKey(Pair.Key))
					{
						Console.Error.Write(" <-- NEW!");
					}
					Console.Error.WriteLine("");
				}
			}

			//if (BreakPoint) IsDebuggerPresentDebugBreak();
			Action<CpuThreadState> Delegate = MipsMethodEmiter.CreateDelegate();

			return Delegate;
		}
	}
}
