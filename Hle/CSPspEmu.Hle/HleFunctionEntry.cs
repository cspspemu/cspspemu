using CSPspEmu.Core.Cpu;
using System;

namespace CSPspEmu.Hle
{
    public struct HleFunctionEntry
    {
        public uint NID;
        public string Name;
        public string Description;
        public HleModuleHost Module;
        public string ModuleName;
        public Action<CpuThreadState> Delegate;

        public override string ToString()
        {
            return string.Format("FunctionEntry(NID=0x{0:X}, Name='{1}', Description='{2}', Module='{3}')", NID, Name,
                Description, Module);
        }
    }
}