using SafeILGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
	public class NativeSyscallInfo
	{
		public string Name;
		public ILInstanceHolderPoolItem<Action<CpuThreadState>> PoolItem;
	}
}
