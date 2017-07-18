using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
    public interface IInterruptManager
    {
        void Interrupt(CpuThreadState cpuThreadState);
    }
}