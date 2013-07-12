using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.VFpu
{
	public enum VfpuControlRegistersEnum
	{
		/// <summary>
		/// Source prefix stack
		/// </summary>
		VFPU_PFXS = 128,

		/// <summary>
		/// Target prefix stack
		/// </summary>
		VFPU_PFXT = 129,

		/// <summary>
		/// Destination prefix stack
		/// </summary>
		VFPU_PFXD = 130,

		/// <summary>
		/// Condition information
		/// </summary>
		VFPU_CC = 131,

		/// <summary>
		/// VFPU internal information 4
		/// </summary>
		VFPU_INF4 = 132,

		/// <summary>
		/// Not used (reserved)
		/// </summary>
		VFPU_RSV5 = 133,

		/// <summary>
		/// Not used (reserved)
		/// </summary>
		VFPU_RSV6 = 134,

		/// <summary>
		/// VFPU revision information
		/// </summary>
		VFPU_REV = 135,

		/// <summary>
		/// Pseudorandom number generator information 0
		/// </summary>
		VFPU_RCX0 = 136,

		/// <summary>
		/// Pseudorandom number generator information 1
		/// </summary>
		VFPU_RCX1 = 137,

		/// <summary>
		/// Pseudorandom number generator information 2
		/// </summary>
		VFPU_RCX2 = 138,

		/// <summary>
		/// Pseudorandom number generator information 3
		/// </summary>
		VFPU_RCX3 = 139,

		/// <summary>
		/// Pseudorandom number generator information 4
		/// </summary>
		VFPU_RCX4 = 140,

		/// <summary>
		/// Pseudorandom number generator information 5
		/// </summary>
		VFPU_RCX5 = 141,

		/// <summary>
		/// Pseudorandom number generator information 6
		/// </summary>
		VFPU_RCX6 = 142,

		/// <summary>
		/// Pseudorandom number generator information 7
		/// </summary>
		VFPU_RCX7 = 143,
	}
}
