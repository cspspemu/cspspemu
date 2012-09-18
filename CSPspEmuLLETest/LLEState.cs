using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmuLLETest
{
	public class LLEState
	{
		public LlePspCpu Cpu;
		public LlePspCpu Me;
		public LleGPIO GPIO;
		public LleNAND NAND;
		public LleKirk LleKirk;
		public DebugPspMemory Memory;
	}
}
