using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
	sealed public class MethodCache
	{
		private Dictionary<uint, Action<CpuThreadState>> Methods = new Dictionary<uint, Action<CpuThreadState>>();

		public void Clear()
		{
			Methods.Clear();
		}

		public Action<CpuThreadState> TryGetMethodAt(uint PC)
		{
			//return null;
			Action<CpuThreadState> Delegate;
			if (Methods.TryGetValue(PC, out Delegate))
			{
				return Delegate;
			}
			return null;
		}

		public void SetMethodAt(uint PC, Action<CpuThreadState> Action)
		{
			Methods[PC] = Action;
		}
	}
}
