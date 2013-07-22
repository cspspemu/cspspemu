using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Hle
{
	public unsafe struct SceKernelSysClock
	{
		//ulong Value;
		public uint Low;
		public uint High;

		public long MicroSeconds
		{
			get
			{
				fixed (uint* LowPtr = &Low)
				{
					return *(long*)LowPtr;
				}
			}
			set
			{
				fixed (uint* LowPtr = &Low)
				{
					*(long*)LowPtr = value;
				}
			}
		}
	}
}
