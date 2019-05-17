using System;

namespace CSPspEmu.Core.Cpu.Table
{
    [AttributeUsage(AttributeTargets.All)]
    public class InstructionName : Attribute
    {
        public readonly string Name;

        public InstructionName(string name)
        {
            Name = name;
        }

        public override string ToString() => $"{Name}";
    }
}