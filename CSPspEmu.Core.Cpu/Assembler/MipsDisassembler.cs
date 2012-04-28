using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Table;
using System.Reflection.Emit;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Assembler
{
	public class MipsDisassembler
	{
		static public readonly MipsDisassembler Methods = new MipsDisassembler();
		static protected Dictionary<String, InstructionInfo> InstructionDictionary = new Dictionary<String, InstructionInfo>();

		public struct Result
		{
			public uint InstructionPC;
			public Instruction Instruction;
			public InstructionInfo InstructionInfo;

			static public String RegisterIndexToRegisterName(int RegisterIndex)
			{
				return String.Format("r{0}", RegisterIndex);
			}

			static public readonly Dictionary<string, Func<Result, string>> Opcodes = new Dictionary<string, Func<Result, string>>()
			{
				//return (uint)(PC & ~PspMemory.MemoryMask) | (Instruction.JUMP << 2);

				{ "J", Result => RegisterIndexToRegisterName(Result.Instruction.RS) },
				{ "j", Result => "0x%08X".Sprintf(Result.Instruction.GetJumpAddress(Result.InstructionPC)) },
				{ "s", Result => RegisterIndexToRegisterName(Result.Instruction.RS) },
				{ "d", Result => RegisterIndexToRegisterName(Result.Instruction.RD) },
				{ "t", Result => RegisterIndexToRegisterName(Result.Instruction.RT) },
				{ "a", Result => Result.Instruction.POS.ToString() },
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
		protected Func<uint, MipsDisassembler, Result> ProcessCallback;

		public MipsDisassembler()
		{
		}

		public Result Disassemble(uint PC, Instruction Instruction)
		{
			if (ProcessCallback == null)
			{
				InstructionDictionary = InstructionTable.ALL.ToDictionary(Item => Item.Name);
				ProcessCallback = EmitLookupGenerator.GenerateSwitch<Func<uint, MipsDisassembler, Result>>(InstructionTable.ALL, (SafeILGenerator, InstructionInfo) =>
				{
					//SafeILGenerator.LoadArgument<MipsDisassembler>(1);
					SafeILGenerator.LoadArgument<uint>(0);
					if (InstructionInfo == null)
					{
						SafeILGenerator.Push("Unknown");
					}
					else
					{
						SafeILGenerator.Push(InstructionInfo.Name);
					}
					SafeILGenerator.Call((Func<uint, String, Result>)MipsDisassembler._InternalHandle);
				});
			}

			var Result = ProcessCallback(Instruction, this);
			Result.InstructionPC = PC;
			return Result;
		}

		static public Result _InternalHandle(uint Data, String Name)
		{
			return new Result()
			{
				Instruction = Data,
				InstructionInfo = MipsDisassembler.InstructionDictionary[Name],
			};
		}
	}
}
