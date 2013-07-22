using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Debugger
{
	unsafe public struct GdbProcessorRegisters
	{
		/// <summary>
		/// General Purpose Registers
		/// 0x00 .. 0x1F
		/// </summary>
		public fixed uint GPR[32];
		
		/// <summary>
		/// 0x20
		/// </summary>
		public uint CP0_STATUS;

		/// <summary>
		/// LOw value for division/multiplication
		/// 0x21
		/// </summary>
		public uint LO;

		/// <summary>
		/// HIGH value for division/multiplication
		/// 0x22
		/// </summary>
		public uint HI;
		
		/// <summary>
		/// 0x23
		/// </summary>
		public uint CP0_BADVADDR;
		
		/// <summary>
		/// Program Counter
		/// 0x24
		/// </summary>
		public uint PC;
		public uint CO0_CAUSE { get { return PC; } }

		/// <summary>
		/// 0x25
		/// </summary>
		public uint __UNK;
		
		/// <summary>
		/// Floating Point Registers
		/// 0x26 .. 0x45
		/// </summary>
		public fixed uint FPR[32];
		
		/// <summary>
		/// 0x46
		/// </summary>
		public uint FCSR;
		
		/// <summary>
		/// 0x47
		/// </summary>
		public uint FIR;
		
		/// <summary>
		/// 0x48
		/// </summary>
		public uint LINUX_RESTART;

		/// <summary>
		/// 0x00 .. 0x48
		/// </summary>
		public uint[] ALL
		{
			get
			{
				return null;
				//PointerUtils
				//[0x49]
			}
		}
	}
}
