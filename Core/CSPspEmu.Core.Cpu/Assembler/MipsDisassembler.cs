using System;
using System.Collections.Generic;
using System.Linq;
using CSPspEmu.Core.Cpu.Table;
using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast;

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

			public static String GprIndexToRegisterName(int RegisterIndex)
			{
				return String.Format("r{0}", RegisterIndex);
			}

			public static String FprIndexToRegisterName(int RegisterIndex)
			{
				return String.Format("f{0}", RegisterIndex);
			}

			public static readonly Dictionary<string, Func<Result, string>> Opcodes = new Dictionary<string, Func<Result, string>>()
			{
				//return (uint)(PC & ~PspMemory.MemoryMask) | (Instruction.JUMP << 2);
				{ "O", Result => String.Format("0x{0:X8}", Result.Instruction.GetBranchAddress(Result.InstructionPC)) },
				{ "J", Result => GprIndexToRegisterName(Result.Instruction.RS) },
				{ "j", Result => String.Format("0x{0:X8}", Result.Instruction.GetJumpAddress(Result.MemoryInfo, Result.InstructionPC)) },
				
				{ "s", Result => GprIndexToRegisterName(Result.Instruction.RS) },
				{ "d", Result => GprIndexToRegisterName(Result.Instruction.RD) },
				{ "t", Result => GprIndexToRegisterName(Result.Instruction.RT) },

				{ "S", Result => FprIndexToRegisterName(Result.Instruction.FS) },
				{ "D", Result => FprIndexToRegisterName(Result.Instruction.FD) },
				{ "T", Result => FprIndexToRegisterName(Result.Instruction.FT) },

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
		static protected Func<uint, MipsDisassembler, Result> ProcessCallback;

		public MipsDisassembler()
		{
		}

		protected static InstructionInfo[] InstructionLookup;

		static private AstGenerator ast = AstGenerator.Instance;

		public Result Disassemble(uint PC, Instruction Instruction)
		{
			if (ProcessCallback == null)
			{
				var Dictionary = new Dictionary<InstructionInfo, int>();

				InstructionLookup = InstructionTable.ALL.ToArray();
				for (int n = 0; n < InstructionLookup.Length; n++) Dictionary[InstructionLookup[n]] = n;

				ProcessCallback = EmitLookupGenerator.GenerateSwitch<Func<uint, MipsDisassembler, Result>>("", InstructionTable.ALL, (InstructionInfo) =>
				{
					return ast.Return(ast.CallStatic(
						(Func<uint, int, Result>)MipsDisassembler._InternalHandle,
						ast.Argument<uint>(0),
						(InstructionInfo != null) ? Dictionary[InstructionInfo] : -1
					));
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
