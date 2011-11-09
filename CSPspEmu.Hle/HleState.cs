using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emiter;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core;

namespace CSPspEmu.Hle
{
	public class HleState
	{
		public bool IsRunning;
		public Processor Processor;
		public PspRtc PspRtc;
		public PspDisplay PspDisplay;
		public PspController PspController;

		public MipsEmiter MipsEmiter;

		// Hle Managers
		public HleThreadManager ThreadManager;
		public HleMemoryManager MemoryManager;
		public HleModuleManager ModuleManager;
		public HleCallbackManager CallbackManager;

		public HleState(Processor Processor, PspRtc PspRtc, PspDisplay PspDisplay, PspController PspController)
		{
			this.IsRunning = true;
			this.Processor = Processor;
			this.PspRtc = PspRtc;
			this.PspDisplay = PspDisplay;
			this.PspController = PspController;
	
			this.MipsEmiter = new MipsEmiter();

			this.ThreadManager = new HleThreadManager(this.Processor, this.PspRtc);
			this.MemoryManager = new HleMemoryManager(this.Processor.Memory);
			this.ModuleManager = new HleModuleManager(this);
			this.CallbackManager = new HleCallbackManager(this);
		}
	}
}
