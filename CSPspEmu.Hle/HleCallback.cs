using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle
{
	sealed public class HleCallback
	{
		public string Name { get; private set; }
		public uint Function { get; private set; }
		public object[] Arguments { get; private set; }

		private HleCallback()
		{
		}

		static public HleCallback Create(string Name, uint Function, params object[] Arguments)
		{
			return new HleCallback() { Name = Name, Function = Function, Arguments = Arguments };
		}

		public override string ToString()
		{
			return String.Format("HleCallback(Name='{0}', Function=0x{1:X})", Name, Function);
		}

		public void SetArgumentsToCpuThreadState(CpuThreadState CpuThreadState)
		{
			int GprIndex = 4;
			int FprIndex = 0;
			Action<int> GprAlign = (int Alignment) =>
			{
				GprIndex = (int)MathUtils.NextAligned((uint)GprIndex, Alignment);
			};
			foreach (var Argument in Arguments)
			{
				var ArgumentType = Argument.GetType();
				if (ArgumentType == typeof(uint))
				{
					GprAlign(1);
					CpuThreadState.GPR[GprIndex++] = (int)(uint)Argument;
				}
				else if (ArgumentType == typeof(int))
				{
					GprAlign(1);
					CpuThreadState.GPR[GprIndex++] = (int)Argument;
				}
				else
				{
					throw(new NotImplementedException(String.Format("Can't handle type '{0}'", ArgumentType)));
				}
			}

			CpuThreadState.PC = Function;
			//Console.Error.WriteLine(CpuThreadState);
			//CpuThreadState.DumpRegisters(Console.Error);
		}
	}
}
