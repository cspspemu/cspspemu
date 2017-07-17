using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Memory
{
    public class DefaultMemoryInfo : IPspMemoryInfo
    {
        public static DefaultMemoryInfo Instance = new DefaultMemoryInfo();

        private DefaultMemoryInfo()
        {
        }

        public bool IsAddressValid(uint Address)
        {
            return PspMemory.IsAddressValid(Address);
        }
    }
}