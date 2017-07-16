using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.Table
{
    public enum AddressType
    {
        None = 0,
        T16 = 1,
        T26 = 2,
        Reg = 3,
    }
}