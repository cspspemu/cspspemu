using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio;
//using CSPspEmu.Core.Audio.Impl.Openal;
using CSPspEmu.Core.Controller;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Utils;
using CSPspEmu.Gui.Winforms;
using CSPspEmu.Runner;
using CSPspEmu.Core.Audio.Impl.WaveOut;
using CSPspEmu.Core.Audio.Impl.Openal;
using CSPspEmu.Hle.Modules;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Gpu.Impl.OpenglEs;
using CSharpUtils;
using CSPspEmu.Hle;
using CSPspEmu.Core.Components.Display;
using CSPspEmu.Hle.Loader;
using System.Reflection;
using System.Globalization;

namespace CSPspEmu
{
    class PspEmulator : IGuiExternalInterface
	{
		[Inject]
		CpuConfig CpuConfig;

		[Inject]
		GpuConfig GpuConfig;

		[Inject]
		GuiConfig GuiConfig;

		[Inject]
		HleConfig HleConfig;

		[Inject]
		DisplayConfig DisplayConfig;

		[Inject]
		PspMemory PspMemory;

		[Inject]
		ElfConfig ElfConfig;

		[Inject]
		GpuImpl GpuImpl;

		[Inject]
		PspDisplay PspDisplay;

		public InjectContext InjectContext
		{
			get
			{
				lock (this)
				{
					return _InjectContext;
				}
			}
		}

		[Inject]
		private InjectContext _InjectContext;

		[Inject]
		PspRunner PspRunner;

		PspStoredConfig StoredConfig;

		/// <summary>
		/// 
		/// </summary>
		public ManualResetEvent ContextInitialized = new ManualResetEvent(false);
		public bool ContextInitializedFlag = false;

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
			StoredConfig = PspStoredConfig.Load();
		}

		/// <summary>
		/// 
		/// </summary>
		public void StartAndLoad(string File, bool TraceSyscalls = false, bool ShowMenus = true, bool TrackCallStack = true, bool? EnableMpeg = null)
		{
			Start(() =>
			{
				CpuConfig.DebugSyscalls = TraceSyscalls;
				CpuConfig.TrackCallStack = TrackCallStack;
				LoadFile(File);
			}, ShowMenus: ShowMenus, AutoLoad: true);
		}

		PspDisplayForm PspDisplayForm;

		/// <summary>
		/// Start.
		/// </summary>
		public void Start(Action CallbackOnInit = null, bool ShowMenus = true, bool AutoLoad = false, bool TrackCallStack = true)
		{
			try
			{
				// Creates a temporal context.
				//PspEmulatorContext = new PspEmulatorContext(PspConfig);

				// Creates a new context.
				CreateNewContextAndRemoveOldOne();

				if (CallbackOnInit != null)
				{
					CallbackOnInit();
				}
				// GUI Thread.
				Thread.CurrentThread.Name = "GuiThread";
				if (Platform.OperatingSystem == Platform.OS.Windows)
				{
					Application.EnableVisualStyles();
				}
				Application.SetCompatibleTextRenderingDefault(false);
				GuiConfig.ShowMenus = ShowMenus;
				GuiConfig.AutoLoad = AutoLoad;
				GuiConfig.DefaultDisplayScale = ShowMenus ? 1 : 2;
				//Thread.CurrentThread.CurrentUICulture = new CultureInfo(GlobalConfig.ThreadCultureName);
				Application.Run(PspDisplayForm = new PspDisplayForm(this));

				ContextInitialized.WaitOne();
				PspRunner.StopSynchronized();
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
			}
			finally
			{
				StoredConfig.Save();
			}

			Environment.Exit(0);
		}

		public void LoadFile(String FileName)
		{
			CreateNewContextAndRemoveOldOne();

			if (File.Exists(FileName + ".cwcheat"))
			{
				ParseCwCheat(File.ReadAllLines(FileName + ".cwcheat"));
			}

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

					InjectContext.GetInstance<PspMemory>().Dispose();
					InjectContext.GetInstance<GpuImpl>().StopSynchronized();
					InjectContext.GetInstance<PspAudioImpl>().StopSynchronized();

					PspRunner = null;
					_InjectContext = null;
					GC.Collect();
				}

				/*
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
				*/
				//

				lock (this)
				{

					_InjectContext = PspInjectContext.CreateInjectContext(StoredConfig, Test: false);
					_InjectContext.SetInstanceType<IGuiExternalInterface, PspEmulator>();
					_InjectContext.InjectDependencesTo(this);

					//if (PspDisplayForm != null)
					//{
					//	InjectContext.InjectDependencesTo(PspDisplayForm);
					//}

					PspDisplay.VBlankEventCall += new Action(PspEmulator_VBlankEventCall);
					PspRunner.StartSynchronized();
				}

				//GpuImpl.InitSynchronizedOnce();
			}
			ContextInitializedFlag = true;
			ContextInitialized.Set();
		}

		public void ShowDebugInformation()
		{
			var CpuProcessor = InjectContext.GetInstance<CpuProcessor>();
			Console.WriteLine("-----------------------------------------------------------------");
			Console.WriteLine("ShowDebugInformation:");
			Console.WriteLine("-----------------------------------------------------------------");
			try
			{
				foreach (var Pair in CpuProcessor.GlobalInstructionStats.OrderBy(Pair => Pair.Value))
				{
					Console.WriteLine("{0} -> {1}", Pair.Key, Pair.Value);
				}
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
			}

			/*
			Console.WriteLine("-----------------------------------------------------------------");
			foreach (var Pair in CpuProcessor.GlobalInstructionStats.OrderBy(Pair => Pair.Key)) Console.WriteLine("{0} -> {1}", Pair.Key, Pair.Value);
			*/

			Console.WriteLine("-----------------------------------------------------------------");
			Console.WriteLine("Last called syscalls: ");
			try
			{
				foreach (var CalledCallback in InjectContext.GetInstance<HleModuleManager>().LastCalledCallbacks.ToArray().Reverse())
				{
					Console.WriteLine("  {0}", CalledCallback);
				}
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
			}
			Console.WriteLine("-----------------------------------------------------------------");
			try
			{
				PspRunner.CpuComponentThread.DumpThreads();
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
			}
			Console.WriteLine("-----------------------------------------------------------------");
		}

		public struct CWCheat
		{
			public uint Code;
			public uint Value;

			/*
			 NB: the codes are in the relative format from the start of the user ram area.
				 So the absolute adress is relative adress +0x08800000
				 To convert some cheat from the absolute format you need to
				 subtract 0x08800000  from the adress of the code
			 */

			public void Patch(PspMemory PspMemory)
			{
				try
				{
					_Patch(PspMemory);
				}
				catch (Exception Exception)
				{
					Console.WriteLine(Exception);
				}
			}

			private void _Patch(PspMemory PspMemory)
			{
				byte OpCode = (byte)((this.Code & 0xF0000000) >> 28);
				uint Info = this.Code & 0x0FFFFFFF;
				uint Value = this.Value;
				uint Address = 0x08800000 + Info;
				try
				{
					switch (OpCode)
					{
						// [t]8-bit Constant Write 0x0aaaaaaa 0x000000dd
						case 0x0:
							PspMemory.WriteSafe(Address, (byte)(Value & 0xFF));
							break;
						// [t]16-bit Constant write 0x1aaaaaaa 0x0000dddd
						case 0x1:
							PspMemory.WriteSafe(Address, (ushort)(Value & 0xFFFF));
							break;
						// [t]32-bit Constant write 0x2aaaaaaa 0xdddddddd
						case 0x2:
							PspMemory.WriteSafe(Address, (uint)(Value & 0xFFFFFFFF));
							break;
						// 16-bit XOR - 0x7aaaaaaa 0x0005vvvv
						// 8-bit  XOR - 0x7aaaaaaa 0x000400vv
						// 16-bit AND - 0x7aaaaaaa 0x0003vvvv
						// 8-bit  AND - 0x7aaaaaaa 0x000200vv
						// 16-bit OR  - 0x7aaaaaaa 0x0001vvvv
						// 8-bit  OR  - 0x7aaaaaaa 0x000000vv
						case 0x7:
							{
								uint SubOpCode = (Value >> 16) & 0xFFFF;
								uint SubValue = (Value >> 0) & 0xFFFF;
								switch (SubOpCode)
								{
									// 8-bit  OR  - 0x7aaaaaaa 0x000000vv
									case 0:
										PspMemory.WriteSafe(Address, (byte)(PspMemory.ReadSafe<byte>(Address) | (SubValue & 0xFF)));
										break;
									default:
										Console.Error.WriteLine("Invalid CWCheatOpCode: 0x{0:X} : 0x{1:X}", OpCode, SubOpCode);
										break;
								}
							}
							break;
						default:
							Console.Error.WriteLine("Invalid CWCheatOpCode: 0x{0:X}", OpCode);
							break;
					}
				}
				catch (Exception Exception)
				{
					throw (new Exception(String.Format("At Address: 0x{0:X}", Address), Exception));
				}
			}
		}

		protected List<CWCheat> CWCheats = new List<CWCheat>();
		//public bool UseFastMemory;

		void PspEmulator_VBlankEventCall()
		{
			foreach (var CWCheat in CWCheats)
			{
				if (PspMemory != null)
				{
					CWCheat.Patch(PspMemory);
				}
			}
			//Console.Error.WriteLine("VBlank!");
		}

		/*
		 *16-bit Greater Than : Multiple Skip 	Ennndddd 3aaaaaaa
		 *16-bit Less Than : Multiple Skip 0xEnnndddd 0x2aaaaaaa
		 *16-bit Not Equal : Multiple Skip 0xEnnndddd 0x1aaaaaaa
		 *16-bit Equal : Multiple Skip	0xEnnndddd 0x0aaaaaaa
		 *16-bit greater than - TEST CODE - 0xDaaaaaaa 0x0030dddd
		 *16-bit less than - TEST CODE - 0xDaaaaaaa 0x0020dddd
		 *16-bit not equal - TEST CODE - 0xDaaaaaaa 0x0010dddd
		 *16-bit equal - TEST CODE -   	0xDaaaaaaa 0x0000dddd
		 *code stopper  0xCaaaaaaa 0xvvvvvvvv
		 *Time Command  0xB0000000 0xnnnnnnnn (based on cheat delay)
		 *[pointer command] 32-bit write	0x6aaaaaaa 0xvvvvvvvv 0x0002nnnn 0xiiiiiiii
		 *[pointer command] 16-bit write	0x6aaaaaaa 0x0000vvvv 0x0001nnnn 0xiiiiiiii
		 *[pointer command] 8-bit write	0x6aaaaaaa 0x000000vv 0x0000nnnn 0xiiiiiiii
		 *copy byte	0x5aaaaaaa 0xnnnnnnnn 0xbbbbbbbb 0x00000000
		 *[tp]32-bit Multi-Address Write 	0x4aaaaaaa 0xxxxxyyyy 0xdddddddd 0x00000000
		 *32-bit decrement 0x30500000 0xaaaaaaaa 0xnnnnnnnn 0x00000000
		 *32-bit increment 0x30400000 0xaaaaaaaa 0xnnnnnnnn 0x00000000
		 *16-bit decrement 0x3030nnnn 0xaaaaaaaa
		 *16-bit increment 0x3020nnnn 0xaaaaaaaa
		 *8-bit decrement 0x301000nn 0xaaaaaaaa
		 *8-bit increment 0x300000nn 0xaaaaaaaa
		 *
		*/

		public void AddCwCheat(uint Code, uint Value)
		{
			CWCheats.Add(new CWCheat()
			{
				Code = Code,
				Value = Value,
			});
			//throw new NotImplementedException();
		}

		public void ParseCwCheat(string[] Lines)
		{
			CWCheats.Clear();
			foreach (var LineRaw in Lines)
			{
				var Line = LineRaw.Trim();
				var Parts = Line.Split(' ', '\t');
				if (Parts.Length >= 3)
				{
					if (Parts[0] == "_L")
					{
						var Code = (uint)NumberUtils.ParseIntegerConstant(Parts[1]);
						var Value = (uint)NumberUtils.ParseIntegerConstant(Parts[2]);
						AddCwCheat(Code, Value);
						//Console.WriteLine("{0} {1:X} {2:X}", Line, Code, Value);
					}
				}
			}
			//Console.ReadKey();
		}

		public PluginInfo GetAudioPluginInfo()
		{
			return InjectContext.GetInstance<PspAudioImpl>().PluginInfo;
		}

		public PluginInfo GetGpuPluginInfo()
		{
			return InjectContext.GetInstance<GpuImpl>().PluginInfo;
		}

		public void CaptureGpuFrame()
		{
			InjectContext.GetInstance<GpuProcessor>().CaptureFrame();
		}

		public object GetCpuProcessor()
		{
			return InjectContext.GetInstance<CpuProcessor>();
		}
	}
}
