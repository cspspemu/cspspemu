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
		public HleThreadManager ThreadManager;
		public HleMemoryManager MemoryManager;
		public HleModuleManager ModuleManager;
		public HleCallbackManager CallbackManager;

		public HleState(Processor Processor)
		{
			this.MipsEmiter = new MipsEmiter();
			this.Processor = Processor;
			this.PspRtc = new PspRtc();
			this.PspDisplay = new PspDisplay(this.PspRtc);
			this.PspController = new PspController();
			this.ThreadManager = new HleThreadManager(this.Processor, this.PspRtc);
			this.MemoryManager = new HleMemoryManager(this.Processor.Memory);
			this.ModuleManager = new HleModuleManager(this);
			this.CallbackManager = new HleCallbackManager(this);
		}
	}
}
