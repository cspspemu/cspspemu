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

		/// <summary>
		/// 
		/// </summary>
		public ManualResetEvent ContextInitialized = new ManualResetEvent(false);
		public bool ContextInitializedFlag = false;


		PspMemory IGuiExternalInterface.GetMemory()
		{
			ContextInitialized.WaitOne();
			return PspEmulatorContext.GetInstance<PspMemory>();
		}

		public PspDisplay GetDisplay()
		{
			ContextInitialized.WaitOne();
			return PspEmulatorContext.GetInstance<PspDisplay>();
		}

		public PspConfig GetConfig()
		{
			return PspConfig;
		}

		public PspController GetController()
		{
			ContextInitialized.WaitOne();
			return PspEmulatorContext.GetInstance<PspController>();
		}

		public bool IsInitialized()
		{
			return ContextInitializedFlag;
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
				if (PspRunner == null) return false;
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
		/// 
		/// </summary>
		public void StartAndLoad(string File, bool TraceSyscalls = false)
		{
			PspConfig.DebugSyscalls = TraceSyscalls;
			Start(() =>
			{
				LoadFile(File);
			});
		}

		/// <summary>
		/// Start.
		/// </summary>
		public void Start(Action CallbackOnInit = null)
		{
			// Creates a temporal context.
			//PspEmulatorContext = new PspEmulatorContext(PspConfig);

			// Creates a new context.
			new Thread(() =>
			{
				CreateNewContextAndRemoveOldOne();

				if (CallbackOnInit != null)
				{
					CallbackOnInit();
				}
			}).Start();

			// GUI Thread.
			Thread.CurrentThread.Name = "GuiThread";
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new PspDisplayForm(this));

			ContextInitialized.WaitOne();
			PspRunner.StopSynchronized();
		}

		public void LoadFile(String FileName)
		{
			CreateNewContextAndRemoveOldOne();

			PspRunner.CpuComponentThread.ThreadTaskQueue.EnqueueAndWaitCompleted(() =>
			{
				PspRunner.CpuComponentThread._LoadFile(FileName);
			});
		}

		void CreateNewContextAndRemoveOldOne()
		{
			ContextInitializedFlag = false;
			ContextInitialized.Reset();
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

				foreach (var FileName in new [] {
					Path.GetDirectoryName(typeof(Program).Assembly.Location) + @"\CSPspEmu.Hle.Modules.dll",
					Application.ExecutablePath,
				})
				{
					if (File.Exists(FileName))
					{
						PspConfig.HleModulesDll = Assembly.LoadFile(FileName);
						break;
					}
				}
				//

				PspEmulatorContext = new PspEmulatorContext(PspConfig);

				{
					PspEmulatorContext.SetInstanceType<GpuImpl, OpenglGpuImpl>();
					PspEmulatorContext.SetInstanceType<PspAudioImpl, PspAudioOpenalImpl>();

#if RELEASE
					PspEmulatorContext.SetInstanceType<PspMemory, FastPspMemory>();
#else
					PspEmulatorContext.SetInstanceType<PspMemory, NormalPspMemory>();
#endif
					/*
					if (PspConfig.UseFastAndUnsaferMemory)
					{
						PspEmulatorContext.SetInstanceType<PspMemory, FastPspMemory>();
					}
					else
					{
						PspEmulatorContext.SetInstanceType<PspMemory, NormalPspMemory>();
					}
					*/
				}

				PspRunner = PspEmulatorContext.GetInstance<PspRunner>();
				PspRunner.StartSynchronized();

				var GpuImpl = PspEmulatorContext.GetInstance<GpuImpl>();
				GpuImpl.InitSynchronizedOnce();
			}
			ContextInitializedFlag = true;
			ContextInitialized.Set();
		}

		public void ShowDebugInformation()
		{
			Console.WriteLine("-----------------------------------------------------------------");
			Console.WriteLine("ShowDebugInformation:");
			var CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			PspRunner.CpuComponentThread.DumpThreads();
			Console.WriteLine("-----------------------------------------------------------------");
			foreach (var Pair in CpuProcessor.GlobalInstructionStats.OrderBy(Pair => Pair.Value))
			{
				Console.WriteLine("{0} -> {1}", Pair.Key, Pair.Value);
			}
			Console.WriteLine("-----------------------------------------------------------------");
			foreach (var Pair in CpuProcessor.GlobalInstructionStats.OrderBy(Pair => Pair.Key))
			{
				Console.WriteLine("{0} -> {1}", Pair.Key, Pair.Value);
			}
			Console.WriteLine("-----------------------------------------------------------------");
		}
	}
}
