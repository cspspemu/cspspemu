using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Memory
{
	public class DefaultMemoryInfo : IPspMemoryInfo
	{
		static public DefaultMemoryInfo Instance = new DefaultMemoryInfo();

		private DefaultMemoryInfo()
		{
		}

		public bool IsAddressValid(uint address)
		{
			return PspMemory.IsAddressValid(address);
		}
	}
}
