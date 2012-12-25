using System;
using System.Collections.Generic;
using System.Linq;
using CSPspEmu.Core.Cpu.Table;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu.Assembler
{
	public class MipsDisassembler
	{
		public static readonly MipsDisassembler Methods = new MipsDisassembler();

		public struct Result
		{
			public uint InstructionPC;
			public Instruction Instruction;
			public InstructionInfo InstructionInfo;
			public IPspMemoryInfo MemoryInfo;

			public static String RegisterIndexToRegisterName(int RegisterIndex)
			{
				return String.Format("r{0}", RegisterIndex);
			}

			public static readonly Dictionary<string, Func<Result, string>> Opcodes = new Dictionary<string, Func<Result, string>>()
			{
				//return (uint)(PC & ~PspMemory.MemoryMask) | (Instruction.JUMP << 2);

				{ "J", Result => RegisterIndexToRegisterName(Result.Instruction.RS) },
				{ "j", Result => String.Format("0x{0:X8}", Result.Instruction.GetJumpAddress(Result.MemoryInfo, Result.InstructionPC)) },
				{ "s", Result => RegisterIndexToRegisterName(Result.Instruction.RS) },
				{ "d", Result => RegisterIndexToRegisterName(Result.Instruction.RD) },
				{ "t", Result => RegisterIndexToRegisterName(Result.Instruction.RT) },
				{ "C", Result => String.Format("{0}", Result.Instruction.CODE) },
				{ "a", Result => Result.Instruction.POS.ToString() },
				{ "i", Result => Result.Instruction.IMM.ToString() },
				{ "I", Result => String.Format("0x{0:X4}", Result.Instruction.IMMU) },
			};

			public string AssemblyLine
			{
				get
				{
					var Parameters = "";
					var Encoding = InstructionInfo.AsmEncoding;
					for (int n = 0; n < Encoding.Length; n++)
					{
						var Char = Encoding[n];
						if (Char == '%')
						{
							bool Found = false;
							for (int Match = 2; Match >= 0; Match--)
							{
								var Part = Encoding.Substr(n + 1, Match);
								if (Opcodes.ContainsKey(Part))
								{
									Parameters += Opcodes[Part](this);
									n += Part.Length;
									Found = true;
									break;
								}
							}
							if (Found) continue;
							//Parameters += Char;
						}
						Parameters += Char;
					}

					return String.Format("{0} {1}", InstructionInfo.Name, Parameters);
				}
			}

			public override string ToString()
			{
				return AssemblyLine;
			}
		}
		protected Func<uint, MipsDisassembler, Result> ProcessCallback;

		public MipsDisassembler()
		{
		}

		protected static InstructionInfo[] InstructionLookup;
		public Result Disassemble(uint PC, Instruction Instruction)
		{
			if (ProcessCallback == null)
			{
				var Dictionary = new Dictionary<InstructionInfo, int>();

				InstructionLookup = InstructionTable.ALL.ToArray();
				for (int n = 0; n < InstructionLookup.Length; n++) Dictionary[InstructionLookup[n]] = n;

				ProcessCallback = EmitLookupGenerator.GenerateSwitch<Func<uint, MipsDisassembler, Result>>(InstructionTable.ALL, (SafeILGenerator, InstructionInfo) =>
				{
					//SafeILGenerator.LoadArgument<MipsDisassembler>(1);
					SafeILGenerator.LoadArgument<uint>(0);
					SafeILGenerator.Push((InstructionInfo != null) ? Dictionary[InstructionInfo] : -1);
					SafeILGenerator.Call((Func<uint, int, Result>)MipsDisassembler._InternalHandle);
				});
			}

			var Result = ProcessCallback(Instruction, this);
			if (Result.InstructionInfo == null)
			{
				Console.Error.WriteLine(String.Format("Instruction at 0x{0:X8} with data 0x{1:X8} didn't generate a value", PC, (uint)Instruction));
				Result.InstructionInfo = InstructionTable.Unknown;
			}
			Result.InstructionPC = PC;
			return Result;
		}

		public static Result _InternalHandle(uint Data, int Index)
		{
			return new Result()
			{
				MemoryInfo = DefaultMemoryInfo.Instance,
				Instruction = Data,
				InstructionInfo = (Index != -1) ? MipsDisassembler.InstructionLookup[Index] : null,
			};
		}
	}
}
