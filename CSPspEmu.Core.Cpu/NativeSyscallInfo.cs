using SafeILGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
	public class NativeSyscallInfo
	{
		public string Name { get { return String.Format("{0}.{1} (0x{2:X8})", ModuleImportName, FunctionEntryName, NID); } }
		public ILInstanceHolderPoolItem<Action<CpuThreadState>> PoolItem;
		public uint NID;
		public string FunctionEntryName;
		public string ModuleImportName;
	}
}
