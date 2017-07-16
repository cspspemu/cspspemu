using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Memory
{
    public interface IPspMemoryInfo
    {
        bool IsAddressValid(uint Address);
    }
}