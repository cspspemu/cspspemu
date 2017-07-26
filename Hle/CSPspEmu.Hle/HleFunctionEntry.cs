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
            return $"FunctionEntry(NID=0x{NID:X}, Name='{Name}', Description='{Description}', Module='{Module}')";
        }
    }
}