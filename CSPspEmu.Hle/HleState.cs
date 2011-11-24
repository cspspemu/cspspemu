﻿using System;
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
	public class HleState
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
		public HleMemoryManager MemoryManager;
		public HleModuleManager ModuleManager;
		public HleCallbackManager CallbackManager;
		public HleIoManager HleIoManager;

		public HleState(CpuProcessor CpuProcessor, GpuProcessor GpuProcessor, PspAudio PspAudio, PspConfig PspConfig, PspRtc PspRtc, PspDisplay PspDisplay, PspController PspController, Assembly ModulesAssembly)
		{
			this.IsRunning = true;
			this.CpuProcessor = CpuProcessor;
			this.GpuProcessor = GpuProcessor;
			this.PspAudio = PspAudio;
			this.PspConfig = PspConfig;
			this.PspRtc = PspRtc;
			this.PspDisplay = PspDisplay;
			this.PspController = PspController;
	
			this.MipsEmiter = new MipsEmiter();

			this.ThreadManager = new HleThreadManager(this.CpuProcessor, this.PspRtc);
			this.MemoryManager = new HleMemoryManager(this.CpuProcessor.Memory);
			this.ModuleManager = new HleModuleManager(this, ModulesAssembly);
			this.CallbackManager = new HleCallbackManager(this);
			this.HleIoManager = new HleIoManager();
		}
	}
}
