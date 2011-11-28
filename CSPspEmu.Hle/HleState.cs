using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emiter;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core;
using System.Reflection;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Audio;
using CSPspEmu.Core.Controller;

namespace CSPspEmu.Hle
{
	public class HleState : PspEmulatorComponent
	{
		public bool IsRunning;
		public CpuProcessor CpuProcessor;
		public GpuProcessor GpuProcessor;
		public PspRtc PspRtc;
		public PspDisplay PspDisplay;
		public PspAudio PspAudio;
		public PspController PspController;
		public PspConfig PspConfig;

		public MipsEmiter MipsEmiter;

		// Hle Managers
		public HleThreadManager ThreadManager;
		public HleSemaphoreManager SemaphoreManager;
		public HleMemoryManager MemoryManager;
		public HleModuleManager ModuleManager;
		public HleCallbackManager CallbackManager;
		public HleIoManager HleIoManager;
		public HleRegistryManager HleRegistryManager;
		public HleOutputHandler HleOutputHandler;

		public HleState(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
			this.IsRunning = true;
			this.CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			this.GpuProcessor = PspEmulatorContext.GetInstance<GpuProcessor>();
			this.PspAudio = PspEmulatorContext.GetInstance<PspAudio>();
			this.PspConfig = PspEmulatorContext.PspConfig;
			this.PspRtc = PspEmulatorContext.GetInstance<PspRtc>();
			this.PspDisplay = PspEmulatorContext.GetInstance<PspDisplay>();
			this.PspController = PspEmulatorContext.GetInstance<PspController>();

			this.MipsEmiter = new MipsEmiter();

			this.HleOutputHandler = PspEmulatorContext.GetInstance<HleOutputHandler>();

			// @TODO FIX! New Instances!?
			this.ThreadManager = PspEmulatorContext.GetInstance<HleThreadManager>();
			this.SemaphoreManager = PspEmulatorContext.GetInstance<HleSemaphoreManager>();
			this.MemoryManager = new HleMemoryManager(this.CpuProcessor.Memory);
			this.ModuleManager = PspEmulatorContext.GetInstance<HleModuleManager>();
			this.CallbackManager = PspEmulatorContext.GetInstance<HleCallbackManager>();
			this.HleIoManager = PspEmulatorContext.GetInstance<HleIoManager>();
			this.HleRegistryManager = PspEmulatorContext.GetInstance<HleRegistryManager>();
		}
	}
}
