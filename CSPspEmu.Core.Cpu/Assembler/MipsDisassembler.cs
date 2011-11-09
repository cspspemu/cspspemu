using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Table;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Assembler
{
	public class MipsDisassembler
	{
		protected Action<uint, MipsDisassembler> ProcessCallback;

		public MipsDisassembler()
		{
			ProcessCallback = EmitLookupGenerator.GenerateSwitchDelegate<MipsDisassembler>(InstructionTable.ALL, (ILGenerator ILGenerator, InstructionInfo InstructionInfo) =>
			{
				ILGenerator.Emit(OpCodes.Ldstr, InstructionInfo.Name);
				ILGenerator.Emit(OpCodes.Call, typeof(MipsAssembler).GetMethod("Handle"));
			});
		}

		public void Process(uint Value)
		{
			ProcessCallback(Value, this);
		}

		public void Handle(String Name)
		{
		}
	}
}
