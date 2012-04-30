using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emiter;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core;
using System.Reflection;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Audio;
using CSPspEmu.Core.Controller;
using CSPspEmu.Core.Crypto;
using CSPspEmu.Core.Battery;
using CSPspEmu.Hle.Vfs.MemoryStick;
using CSPspEmu.Hle.Vfs.Local;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle
{
	[Obsolete("This class should not exists. Classes needing components, should get them injected.")]
	public class HleState : PspEmulatorComponent
	{
		public bool IsRunning;

		[Inject]
		public CpuProcessor CpuProcessor;

		[Inject]
		public PspMemory PspMemory;

		[Inject]
		public PspConfig PspConfig;

		public MipsEmiter MipsEmiter;

		// Hle Managers
		[Inject]
		public HleModuleManager ModuleManager;

		[Inject]
		public HleIoManager HleIoManager;
		[Inject]
		public HleRegistryManager HleRegistryManager;
		[Inject]
		public HleOutputHandler HleOutputHandler;
		[Inject]
		public HleInterruptManager HleInterruptManager;
		[Inject]
		public HleInterop HleInterop;
		[Inject]
		public Kirk Kirk;

		/*
		public string MemoryStickRootLocalFolder
		{
			get
			{
				//var Mountable = (HleIoDriverMountable)HleIoManager.GetDriver("ms:");
				//var MemoryStick = (HleIoDriverMemoryStick)Mountable.GetMount("/");

				var MemoryStick = (HleIoDriverMemoryStick)HleIoManager.GetDriver("ms:");
				var Mountable = (HleIoDriverMountable)MemoryStick.ParentDriver;
				var Local = (HleIoDriverLocalFileSystem)(Mountable.GetMount("/"));
				return Local.BasePath;
			}
		}
		*/

		public override void InitializeComponent()
		{
			this.IsRunning = true;
			this.PspConfig = PspEmulatorContext.PspConfig;

			this.MipsEmiter = new MipsEmiter();
			this.Kirk = new Kirk();
			this.Kirk.kirk_init();
		}
	}
}
