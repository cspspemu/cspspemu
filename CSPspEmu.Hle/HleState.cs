using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emiter;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle
{
	public class HleState
	{
		public MipsEmiter MipsEmiter;
		public Processor Processor;
		public PspRtc PspRtc;
		public PspDisplay PspDisplay;
		public PspController PspController;
		public HleThreadManager HleThreadManager;
		public HleMemoryManager HleMemoryManager;
		public HleModuleManager HleModuleManager;

		public HleState(Processor Processor)
		{
			this.MipsEmiter = new MipsEmiter();
			this.Processor = Processor;
			this.PspRtc = new PspRtc();
			this.PspDisplay = new PspDisplay(this.PspRtc);
			this.PspController = new PspController();
			this.HleThreadManager = new HleThreadManager(this.Processor, this.PspRtc);
			this.HleMemoryManager = new HleMemoryManager(this.Processor.Memory);
			this.HleModuleManager = new HleModuleManager(this);
		}
	}
}
