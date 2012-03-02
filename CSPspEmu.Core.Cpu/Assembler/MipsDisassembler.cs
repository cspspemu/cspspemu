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
		static public readonly MipsDisassembler Methods = new MipsDisassembler();

		protected Action<uint, MipsDisassembler> ProcessCallback;

		public MipsDisassembler()
		{
		}

		public void Process(uint Value)
		{
			if (ProcessCallback == null)
			{
				ProcessCallback = EmitLookupGenerator.GenerateSwitch<Action<uint, MipsDisassembler>>(InstructionTable.ALL, (SafeILGenerator, InstructionInfo) =>
				{
					SafeILGenerator.LoadArgument<MipsDisassembler>(1);
					SafeILGenerator.Push(InstructionInfo.Name);
					SafeILGenerator.Call((Action<String>)MipsDisassembler.Methods.Handle);
				});
			}

			ProcessCallback(Value, this);
		}

		public void Handle(String Name)
		{
		}
	}
}
