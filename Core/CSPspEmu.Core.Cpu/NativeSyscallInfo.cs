using SafeILGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
    public class NativeSyscallInfo
    {
        public string Name => $"{ModuleImportName}.{FunctionEntryName} (0x{Nid:X8})";

        public ILInstanceHolderPoolItem<Action<CpuThreadState>> PoolItem;
        public uint Nid;
        public string FunctionEntryName;
        public string ModuleImportName;
    }
}