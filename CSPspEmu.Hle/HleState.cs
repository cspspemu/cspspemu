using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emiter;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core;
using System.Reflection;

namespace CSPspEmu.Hle
{
	public class HleState
	{
		public bool IsRunning;
		public CpuProcessor Processor;
		public PspRtc PspRtc;
		public PspDisplay PspDisplay;
		public PspController PspController;
		public PspConfig PspConfig;

		public MipsEmiter MipsEmiter;

		// Hle Managers
		public HleThreadManager ThreadManager;
		public HleMemoryManager MemoryManager;
		public HleModuleManager ModuleManager;
		public HleCallbackManager CallbackManager;

		public HleState(CpuProcessor Processor, PspConfig PspConfig, PspRtc PspRtc, PspDisplay PspDisplay, PspController PspController, Assembly ModulesAssembly)
		{
			this.IsRunning = true;
			this.Processor = Processor;
			this.PspConfig = PspConfig;
			this.PspRtc = PspRtc;
			this.PspDisplay = PspDisplay;
			this.PspController = PspController;
	
			this.MipsEmiter = new MipsEmiter();

			this.ThreadManager = new HleThreadManager(this.Processor, this.PspRtc);
			this.MemoryManager = new HleMemoryManager(this.Processor.Memory);
			this.ModuleManager = new HleModuleManager(this, ModulesAssembly);
			this.CallbackManager = new HleCallbackManager(this);
		}
	}
}
