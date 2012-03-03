using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Table;
using System.Reflection.Emit;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Cpu.Assembler
{
	public class MipsDisassembler
	{
		static public readonly MipsDisassembler Methods = new MipsDisassembler();
		static protected Dictionary<String, InstructionInfo> InstructionDictionary = new Dictionary<String, InstructionInfo>();

		public struct Result
		{
			public Instruction Instruction;
			public InstructionInfo InstructionInfo;

			static public String RegisterIndexToRegisterName(int RegisterIndex)
			{
				return String.Format("r{0}", RegisterIndex);
			}

			static public readonly Dictionary<string, Func<Instruction, string>> Opcodes = new Dictionary<string, Func<Instruction, string>>()
			{
				{ "s", Instruction => RegisterIndexToRegisterName(Instruction.RS) },
				{ "d", Instruction => RegisterIndexToRegisterName(Instruction.RD) },
				{ "t", Instruction => RegisterIndexToRegisterName(Instruction.RT) },
				{ "a", Instruction => Instruction.POS.ToString() },
			};

			public override string ToString()
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
								Parameters += Opcodes[Part](Instruction);
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
		protected Func<uint, MipsDisassembler, Result> ProcessCallback;

		public MipsDisassembler()
		{
		}

		public Result Disassemble(uint Value)
		{
			if (ProcessCallback == null)
			{
				InstructionDictionary = InstructionTable.ALL.ToDictionary(Item => Item.Name);
				ProcessCallback = EmitLookupGenerator.GenerateSwitch<Func<uint, MipsDisassembler, Result>>(InstructionTable.ALL, (SafeILGenerator, InstructionInfo) =>
				{
					SafeILGenerator.LoadArgument<MipsDisassembler>(1);
					SafeILGenerator.LoadArgument<uint>(0);
					if (InstructionInfo == null)
					{
						SafeILGenerator.Push("Unknown");
					}
					else
					{
						SafeILGenerator.Push(InstructionInfo.Name);
					}
					SafeILGenerator.Call((Func<uint, String, Result>)MipsDisassembler.Methods.Handle);
				});
			}

			var Result = ProcessCallback(Value, this);
			return Result;
		}

		public Result Handle(uint Data, String Name)
		{
			return new Result()
			{
				Instruction = Data,
				InstructionInfo = InstructionDictionary[Name],
			};
		}
	}
}
