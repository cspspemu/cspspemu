using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio;
using CSPspEmu.Core.Audio.Imple.Openal;
using CSPspEmu.Core.Controller;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Core.Memory;
using CSPspEmu.Gui.Winforms;
using CSPspEmu.Runner;

namespace CSPspEmu.Sandbox
{
	unsafe class PspEmulator : IGuiExternalInterface
	{
		private PspConfig PspConfig;
		private PspEmulatorContext PspEmulatorContext;
		protected PspRunner PspRunner;
		PspMemory PspMemory;

		PspMemory IGuiExternalInterface.GetMemory()
		{
			return PspEmulatorContext.GetInstance<PspMemory>();
		}

		public PspDisplay GetDisplay()
		{
			return PspEmulatorContext.GetInstance<PspDisplay>();
		}

		public PspConfig GetConfig()
		{
			return PspConfig;
		}

		public PspController GetController()
		{
			return PspEmulatorContext.GetInstance<PspController>();
		}

		public void PauseResume(Action Action)
		{
			if (Paused)
			{
				Action();
			}
			else
			{
				Pause();
				try
				{
					Action();
				}
				finally
				{
					Resume();
				}
			}
		}

		public bool IsPaused()
		{
			return Paused;
		}

		public bool Paused
		{
			get
			{
				return PspRunner.Paused;
			}
		}

		public void Pause()
		{
			if (!Paused)
			{
				PspRunner.PauseSynchronized();
			}
		}

		public void Resume()
		{
			if (Paused)
			{
				PspRunner.ResumeSynchronized();
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public PspEmulator()
		{
			PspConfig = new PspConfig();
		}

		/// <summary>
		/// Start.
		/// </summary>
		public void Start()
		{
			// Creates a new context.
			CreateNewContextAndRemoveOldOne();

			OnStarted();

			// GUI Thread.
			Thread.CurrentThread.Name = "GuiThread";
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new PspDisplayForm(this));

			if (PspRunner != null)
			{
				PspRunner.StopSynchronized();
			}
		}

		public void LoadFile(String FileName)
		{
			CreateNewContextAndRemoveOldOne();

			PspRunner.CpuThread.ThreadTaskQueue.EnqueueAndWaitCompleted(() =>
			{
				PspRunner.CpuThread._LoadFile(FileName);
			});
		}

		void CreateNewContextAndRemoveOldOne()
		{
			// Stops the current context if it has one already.
			if (PspRunner != null)
			{
				PspRunner.StopSynchronized();

				PspEmulatorContext.GetInstance<PspMemory>().Dispose();
				PspEmulatorContext.GetInstance<GpuImpl>().StopSynchronized();
				PspEmulatorContext.GetInstance<PspAudioImpl>().StopSynchronized();

				PspRunner = null;
				PspEmulatorContext = null;
				GC.Collect();
			}

			PspConfig.HleModulesDll = Assembly.LoadFile(Path.GetDirectoryName(typeof(Program).Assembly.Location) + @"\CSPspEmu.Hle.Modules.dll");

			PspEmulatorContext = new PspEmulatorContext(PspConfig);

			{
				PspEmulatorContext.SetInstanceType<GpuImpl, OpenglGpuImpl>();
				PspEmulatorContext.SetInstanceType<PspAudioImpl, PspAudioOpenalImpl>();

				if (PspConfig.UseFastAndUnsaferMemory)
				{
					PspEmulatorContext.SetInstanceType<PspMemory, FastPspMemory>();
				}
				else
				{
					PspEmulatorContext.SetInstanceType<PspMemory, NormalPspMemory>();
				}
			}

			PspRunner = PspEmulatorContext.GetInstance<PspRunner>();
			PspRunner.StartSynchronized();

			var GpuImpl = PspEmulatorContext.GetInstance<GpuImpl>();
			GpuImpl.InitSynchronizedOnce();
		}

		public void ShowDebugInformation()
		{
			var CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			foreach (var Key in CpuProcessor.GlobalInstructionStats.Keys.OrderBy(Value => Value))
			{
				Console.WriteLine("{0} -> {1}", Key, CpuProcessor.GlobalInstructionStats[Key]);
			}
		}

		protected void OnStarted()
		{
			//PspConfig.DebugSyscalls = true;
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\audio\polyphonic\polyphonic.elf");

			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\fpu.elf");

			LoadFile(@"C:\pspsdk\psp\sdk\samples\gu\ortho\ortho.elf");
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\gu\lines\lines.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\demos_ex\sdl\main.elf");
			//LoadFile(@"C:\projects\csharp\cspspemu\games\cavestory\EBOOT.PBP");


			//LoadFile(@"C:\pspsdk\psp\sdk\samples\gu\cube\cube.elf");
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\gu\text\gufont.elf");

			//LoadFile(@"C:\projects\jpcsp\demos\compilerPerf.pbp");
			//LoadFile(@"C:\juegos\jpcsp2\demos\fputest.elf");
			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\alu.elf");
			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\fpu.elf");
			//LoadFile(@"C:\projects\cspspemu\PspAutoTests\gum.elf");
			//LoadFile(@"C:\juegos\pspemu\demos\controller.pbp");
			//LoadFile(@"C:\juegos\jpcsp-windows-x86\demos\sound.prx");
			//LoadFile(@"C:\juegos\jpcsp-windows-x86\demos\cube.pbp");
			//LoadFile(@"C:\juegos\jpcsp-windows-x86\demos\ortho.pbp");
			//LoadFile(@"C:\projects\pspemu\pspautotests\tests\cpu\cpu\cpu.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\demos\threadstatus.pbp");
			//LoadFile(@"C:\projects\pspemu\pspautotests\tests\io\io\io.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\tests\cpu\fpu\fpu.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\tests\malloc\malloc.elf");

			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\args.elf");
			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\alu.elf");
			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\fpu.elf");
			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\malloc.elf");

			//LoadFile(@"C:\pspsdk\psp\sdk\samples\kernel\sysevent\EBOOT.PBP");
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\kernel\systimer\EBOOT.PBP");
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\kernel\loadmodule\EBOOT.PBP");
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\prx\prx_loader\EBOOT.PBP");
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\prx\prx_loader\EBOOT.PBP");
			//LoadFile(@"../../../TestInput/minifire.elf");
			//PspConfig.ShowInstructionStats = true;

			//LoadFile(@"../../../TestInput/HelloWorld.elf");
			//LoadFile(@"../../../TestInput/HelloWorldPSP.elf");
			//LoadFile(@"../../../TestInput/counter.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\tests\string\string.elf");
			//LoadFile(@"C:\juegos\jpcsp2\demos\cube.pbp");
			//LoadFile(@"C:\juegos\jpcsp2\demos\nehetutorial02.pbp");
			//LoadFile(@"C:\projects\pspemu\pspautotests\demos\mytest.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\demos\cube.pbp");
			//LoadFile(@"C:\projects\pspemu\demos\dumper.elf");
		}
	}
}
