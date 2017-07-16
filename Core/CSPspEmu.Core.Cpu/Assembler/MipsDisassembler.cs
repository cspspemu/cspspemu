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
			public uint InstructionPc;
			public Instruction Instruction;
			public InstructionInfo InstructionInfo;
			public IPspMemoryInfo MemoryInfo;

			public static string GprIndexToRegisterName(int registerIndex)
			{
				return $"r{registerIndex}";
			}

			public static string FprIndexToRegisterName(int registerIndex)
			{
				return $"f{registerIndex}";
			}

			public static readonly Dictionary<string, Func<Result, string>> Opcodes = new Dictionary<string, Func<Result, string>>()
			{
				//return (uint)(PC & ~PspMemory.MemoryMask) | (Instruction.JUMP << 2);
				{ "O", result => String.Format("0x{0:X8}", result.Instruction.GetBranchAddress(result.InstructionPc)) },
				{ "J", result => GprIndexToRegisterName(result.Instruction.RS) },
				{ "j", result => String.Format("0x{0:X8}", result.Instruction.GetJumpAddress(result.MemoryInfo, result.InstructionPc)) },
				
				{ "s", result => GprIndexToRegisterName(result.Instruction.RS) },
				{ "d", result => GprIndexToRegisterName(result.Instruction.RD) },
				{ "t", result => GprIndexToRegisterName(result.Instruction.RT) },

				{ "S", result => FprIndexToRegisterName(result.Instruction.FS) },
				{ "D", result => FprIndexToRegisterName(result.Instruction.FD) },
				{ "T", result => FprIndexToRegisterName(result.Instruction.FT) },

				{ "C", result => String.Format("{0}", result.Instruction.CODE) },
				{ "a", result => result.Instruction.POS.ToString() },
				{ "i", result => result.Instruction.IMM.ToString() },
				{ "I", result => String.Format("0x{0:X4}", result.Instruction.IMMU) },
			};

			public string AssemblyLine
			{
				get
				{
					var parameters = "";
					var encoding = InstructionInfo.AsmEncoding;
					for (var n = 0; n < encoding.Length; n++)
					{
						var Char = encoding[n];
						if (Char == '%')
						{
							var found = false;
							for (var match = 2; match >= 0; match--)
							{
								var part = encoding.Substr(n + 1, match);
								if (Opcodes.ContainsKey(part))
								{
									parameters += Opcodes[part](this);
									n += part.Length;
									found = true;
									break;
								}
							}
							if (found) continue;
							//Parameters += Char;
						}
						parameters += Char;
					}

					return String.Format("{0} {1}", InstructionInfo.Name, parameters);
				}
			}

			public override string ToString()
			{
				return AssemblyLine;
			}
		}
		static protected Func<uint, MipsDisassembler, Result> ProcessCallback;

		protected static InstructionInfo[] InstructionLookup;

		static private AstGenerator ast = AstGenerator.Instance;

		public Result Disassemble(uint pc, Instruction instruction)
		{
			if (ProcessCallback == null)
			{
				var dictionary = new Dictionary<InstructionInfo, int>();

				InstructionLookup = InstructionTable.ALL.ToArray();
				for (var n = 0; n < InstructionLookup.Length; n++) dictionary[InstructionLookup[n]] = n;

				ProcessCallback = EmitLookupGenerator.GenerateSwitch<Func<uint, MipsDisassembler, Result>>("", InstructionTable.ALL, (InstructionInfo) =>
				{
					return ast.Return(ast.CallStatic(
						(Func<uint, int, Result>)MipsDisassembler._InternalHandle,
						ast.Argument<uint>(0),
						(InstructionInfo != null) ? dictionary[InstructionInfo] : -1
					));
				});
			}

			var result = ProcessCallback(instruction, this);
			if (result.InstructionInfo == null)
			{
				Console.Error.WriteLine($"Instruction at 0x{pc:X8} with data 0x{(uint) instruction:X8} didn't generate a value");
				result.InstructionInfo = InstructionTable.Unknown;
			}
			result.InstructionPc = pc;
			return result;
		}

		public static Result _InternalHandle(uint data, int index)
		{
			return new Result
			{
				MemoryInfo = DefaultMemoryInfo.Instance,
				Instruction = data,
				InstructionInfo = (index != -1) ? InstructionLookup[index] : null,
			};
		}
	}
}
