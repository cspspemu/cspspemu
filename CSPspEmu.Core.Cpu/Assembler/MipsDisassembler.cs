using System;
using System.Collections.Generic;
using System.Linq;
using CSPspEmu.Core.Cpu.Table;

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

			public static String RegisterIndexToRegisterName(int RegisterIndex)
			{
				return String.Format("r{0}", RegisterIndex);
			}

			public static readonly Dictionary<string, Func<Result, string>> Opcodes = new Dictionary<string, Func<Result, string>>()
			{
				//return (uint)(PC & ~PspMemory.MemoryMask) | (Instruction.JUMP << 2);

				{ "J", Result => RegisterIndexToRegisterName(Result.Instruction.RS) },
				{ "j", Result => String.Format("0x{0:X8}", Result.Instruction.GetJumpAddress(Result.InstructionPC)) },
				{ "s", Result => RegisterIndexToRegisterName(Result.Instruction.RS) },
				{ "d", Result => RegisterIndexToRegisterName(Result.Instruction.RD) },
				{ "t", Result => RegisterIndexToRegisterName(Result.Instruction.RT) },
				{ "a", Result => Result.Instruction.POS.ToString() },
				{ "i", Result => Result.Instruction.IMM.ToString() },
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
			Result.InstructionPC = PC;
			return Result;
		}

		public static Result _InternalHandle(uint Data, int Index)
		{
			return new Result()
			{
				Instruction = Data,
				InstructionInfo = (Index != -1) ? MipsDisassembler.InstructionLookup[Index] : null,
			};
		}
	}
}
