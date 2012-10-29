using System;
using System.Collections.Generic;
using System.Linq;

namespace CSPspEmu.Core.Cpu
{
	public sealed class MethodCacheSlow
	{
		private Dictionary<uint, Action<CpuThreadState>> Methods = new Dictionary<uint, Action<CpuThreadState>>();

		public void ClearRange(uint Low, uint High)
		{
			foreach (var Key in Methods.Where(Pair => (Pair.Key >= Low && Pair.Key < High)).Select(Pair => Pair.Key))
			{
				Methods.Remove(Key);
			}
		}

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
			//Methods2[(PC - PspMemory.MainOffset) / 4] = Action;
			Methods[PC] = Action;
		}
	}
}
