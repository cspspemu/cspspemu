//#define ENABLE_RECOMPILE

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Runner;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Drawing;
using CSPspEmu.Core.Gpu.Impl.OpenglEs;
using CSPspEmu.Hle.Modules.iofilemgr;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Modules;

namespace CSPspEmu.AutoTests
{
	public class AutoTestsProgram
	{
		//static TimeSpan TimeoutTime = TimeSpan.FromSeconds(30);
		static TimeSpan TimeoutTime = TimeSpan.FromSeconds(10);

		public class HleOutputHandlerMock : HleOutputHandler
		{
			public String OutputString = "";

			public override void Output(string OutputString)
			{
				this.OutputString += OutputString;
			}
		}

		[Inject]
		CpuConfig CpuConfig;

		[Inject]
		HleConfig HleConfig;

		[Inject]
		PspStoredConfig StoredConfig;

		string FileNameBase;

		public AutoTestsProgram()
		{
			InjectContext.Bootstrap(this);
			CpuConfig.DebugSyscalls = false;
			CpuConfig.ShowInstructionStats = false;
			HleConfig.TraceLastSyscalls = false;
		}

		public void Init()
		{
			foreach (var _FileName in new[] {
					Path.GetDirectoryName(typeof(AutoTestsProgram).Assembly.Location) + @"\CSPspEmu.Hle.Modules.dll",
					Application.ExecutablePath,
				})
			{
				if (File.Exists(_FileName))
				{
					HleConfig.HleModulesDll = Assembly.LoadFile(_FileName);
					break;
				}
			}
		}

		protected string RunExecutableAndGetOutput(bool RunTestsViewOut, string PspAutoTestsFolder, string FileName, out string CapturedOutput, string FileNameBase)
		{
			var OutputString = "";

			IHleIoDriver HostDriver = null;

			InjectContext _InjectContext = null;
			{
				//var Capture = false;
				var Capture = !RunTestsViewOut;
				CapturedOutput = ConsoleUtils.CaptureOutput(() =>
				{
					_InjectContext = PspInjectContext.CreateInjectContext(StoredConfig, Test: true);
					_InjectContext.SetInstanceType<HleOutputHandler, HleOutputHandlerMock>();

					//Console.Error.WriteLine("[1]");

					this.FileNameBase = FileNameBase;

					var Start = DateTime.UtcNow;
					_InjectContext.GetInstance<HleModuleManager>();
					var End = DateTime.UtcNow;
					Console.WriteLine(End - Start);

					//Console.Error.WriteLine("[a]");

					// GPU -> NULL
					//PspEmulatorContext.SetInstanceType<GpuImpl>(typeof(GpuImplOpenglEs));
					_InjectContext.SetInstanceType<GpuImpl>(typeof(GpuImplNull));

					var GpuImpl = _InjectContext.GetInstance<GpuImpl>();
					//GpuImpl.InitSynchronizedOnce();

					//Console.Error.WriteLine("[b]");

					var PspRunner = _InjectContext.GetInstance<PspRunner>();
					PspRunner.StartSynchronized();

					//Console.Error.WriteLine("[c]");

					{
						try
						{
							//PspRunner.CpuComponentThread.SetIso(PspAutoTestsFolder + "/../input/test.cso");
							PspRunner.CpuComponentThread.SetIso(PspAutoTestsFolder + "/../input/cube.cso");
							//Console.Error.WriteLine("[2]");

							var HleIoManager = _InjectContext.GetInstance<HleIoManager>();
							HostDriver = HleIoManager.GetDriver("host:");

							try { HostDriver.IoRemove(null, "/__testoutput.txt"); } catch { }
							try { HostDriver.IoRemove(null, "/__testerror.txt"); } catch { }

							PspRunner.CpuComponentThread._LoadFile(FileName);
							//Console.Error.WriteLine("[3]");
							if (!PspRunner.CpuComponentThread.StoppedEndedEvent.WaitOne(TimeoutTime))
							{
								ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () =>
								{
									Console.Error.WriteLine("Timeout!");
								});
							}
						}
						catch (Exception Exception)
						{
							Console.Error.WriteLine(Exception);
						}
					}

					PspRunner.StopSynchronized();
					GC.Collect();

					using (var test_output = HostDriver.OpenRead("/__testoutput.txt"))
					{
						OutputString = test_output.ReadAllContentsAsString();
					}
				},
				Capture: Capture
				);

				//var HleOutputHandlerMock = (HleOutputHandlerMock)PspEmulatorContext.GetInstance<HleOutputHandler>();
				//OutputString = HleOutputHandlerMock.OutputString;
			}
			_InjectContext.Dispose();

			return OutputString;
		}

		protected void RunFile(bool RunTestsViewOut, string PspAutoTestsFolder, string FileNameExecutable, string FileNameExpected, string FileNameBase)
		{
			ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.DarkCyan, () =>
			{
				Console.Write("{0}...", FileNameExecutable);
			});
			var ExpectedOutput = File.ReadAllText(FileNameExpected, Encoding.ASCII);
			var RealOutput = "";
			string CapturedOutput = "";

			// Execute.
			{
				RealOutput = RunExecutableAndGetOutput(RunTestsViewOut, PspAutoTestsFolder, FileNameExecutable, out CapturedOutput, FileNameBase);
			}

			var ExpectedOutputLines = ExpectedOutput.Trim().Replace("\r\n", "\n").Split('\n');
			var RealOutputLines = RealOutput.Trim().Replace("\r\n", "\n").Split('\n');
			var Result = Diff.DiffTextProcessed(ExpectedOutputLines, RealOutputLines);

			File.WriteAllText(
				Path.ChangeExtension(FileNameExpected, ".lastoutput"),
				RealOutput
			);

			File.WriteAllText(
				Path.ChangeExtension(FileNameExpected, ".lastdebug"),
				CapturedOutput
			);

			bool HadAnError = false;
			for (int n = 0; n < 10; n++)
			{
				var ImageReferenceFile = String.Format("{0}.reference.{1}.png", FileNameBase, n);
				var ImageOutputFile = String.Format("{0}.lastoutput.{1}.png", FileNameBase, n);
				if (File.Exists(ImageReferenceFile))
				{
					if (File.Exists(ImageOutputFile))
					{
						var ReferenceBitmap = new Bitmap(ImageReferenceFile);
						var OutputBitmap = new Bitmap(ImageOutputFile);
						if (ReferenceBitmap.Size == OutputBitmap.Size)
						{
							var CompareResult = BitmapUtils.CompareBitmaps(ReferenceBitmap, OutputBitmap, 0.01);

							if (CompareResult.Equal)
							{
								Console.Error.WriteLine(
									"Files '{0}:{1}' and '{2}:{3}' have different contents {4}/{5} different pixels {6}%",
									ImageReferenceFile, ReferenceBitmap.Size, ImageOutputFile, OutputBitmap.Size,
									CompareResult.DifferentPixelCount, CompareResult.TotalPixelCount, CompareResult.PixelTotalDifferencePercentage
								);
								HadAnError |= true;
							}
						}
						else
						{
							Console.Error.WriteLine(
								"Files '{0}:{1}' and '{2}:{3}' have different sizes",
								ImageReferenceFile, ReferenceBitmap.Size, ImageOutputFile, OutputBitmap.Size
							);
							HadAnError |= true;
						}
					}
					else
					{
						Console.Error.WriteLine(
							"File '{0}' exists, but not exists '{1}'",
							ImageReferenceFile, ImageOutputFile
						);
						HadAnError |= true;
					}
				}
			}

			if (!Result.Items.All(Item => Item.Action == Diff.ProcessedItem.ActionEnum.Keep)) HadAnError |= true;

			if (!HadAnError)
			{
				ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Green, () =>
				{
					Console.WriteLine("Ok");
				});
			}
			else
			{
				ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () =>
				{
					Console.WriteLine("Error");
				});
				Result.Print();
			}
		}

		protected string ExecuteBat(string ExecutableFileName, string Arguments, double TimeoutSeconds = -1)
		{
			var Process = new System.Diagnostics.Process(); // Declare New Process
			//proc.StartInfo.FileName = fileName;
			//proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			//proc.StartInfo.CreateNoWindow = true;

			Process.StartInfo.FileName = ExecutableFileName;
			Process.StartInfo.Arguments = Arguments;
			Process.StartInfo.RedirectStandardError = true;
			Process.StartInfo.RedirectStandardOutput = true;
			Process.StartInfo.UseShellExecute = false;

			Process.Start();


			if (TimeoutSeconds < 0)
			{
				Process.WaitForExit();
			}
			else
			{
				Process.WaitForExit((int)TimeSpan.FromSeconds(TimeoutSeconds).TotalMilliseconds);
			}

			var ErrorMessage = Process.StandardError.ReadToEnd();
			Process.WaitForExit();

			var OutputMessage = Process.StandardOutput.ReadToEnd();
			Process.WaitForExit();

			Process.Start();
			Process.WaitForExit();

			return ErrorMessage + OutputMessage;
		}

		protected void Run(bool RunTestsViewOut, string PspAutoTestsFolder, string WildCardFilter)
		{
			foreach (var FileNameExpected in Directory.GetFiles(PspAutoTestsFolder, "*.expected", SearchOption.AllDirectories))
			{
				var FileNameBaseBase = Path.GetFileNameWithoutExtension(FileNameExpected);
				var FileNameBase = Path.GetDirectoryName(FileNameExpected) + @"\" + FileNameBaseBase;
				var FileNameExecutable = FileNameBase + ".prx";
				var FileNameSourceCode = FileNameBase + ".c";

				var MatchName = FileNameBase.Substr(PspAutoTestsFolder.Length).Replace("\\", "/");

				//Console.WriteLine(MatchName + " ~ " + Wildcard.WildcardToRegex(WildCardFilter));
				if (!new Regex(Wildcard.WildcardToRegex(WildCardFilter)).IsMatch(MatchName))
				{
					continue;
				}

				bool Recompile = false;
				//bool Recompile = true;

				if (File.GetLastWriteTime(FileNameExecutable) != File.GetLastWriteTime(FileNameSourceCode))
				{
					Recompile = true;
				}

#if ENABLE_RECOMPILE
				if (Recompile)
				{
					//PspAutoTestsFolder + @"\make.bat"
					// FileNameBase
					try { File.Delete(FileNameExecutable); } catch { }
					var Output = ExecuteBat(PspAutoTestsFolder + @"\build.bat", FileNameBase);
					if (!string.IsNullOrEmpty((Output)))
					{
						Console.Write("Compiling {0}...", FileNameBase);
						Console.WriteLine("Result:");
						Console.WriteLine("{0}", Output);
					}
					try
					{
						File.SetLastWriteTime(FileNameExecutable, File.GetLastWriteTime(FileNameSourceCode));
					}
					catch
					{
					}
				}
#endif

				if (File.Exists(FileNameExecutable))
				{
					RunFile(RunTestsViewOut, PspAutoTestsFolder, FileNameExecutable, FileNameExpected, FileNameBase);
				}
				else
				{
					Console.WriteLine("Can't find executable for '{0}' '{1}'", FileNameExpected, FileNameExecutable);
				}
			}
		}

		private void InternalMain(bool RunTestsViewOut, String[] Arguments)
		{
var BasePath = Path.GetDirectoryName(Application.ExecutablePath);
			string PspAutoTestsFolder = "";

			foreach (var TryName in new[] { "pspautotests/tests", "../../../pspautotests/tests" })
			{
				if (Directory.Exists(BasePath + "/" + TryName))
				{
					PspAutoTestsFolder = new DirectoryInfo(BasePath + "/" + TryName).FullName;
					break;
				}
			}

			if (string.IsNullOrEmpty(PspAutoTestsFolder))
			{
				Console.Error.WriteLine("Can't find 'pspautotests/tests' folder.");
				Console.ReadKey();
			}

			Console.WriteLine(PspAutoTestsFolder);

			var WildCardFilter = "*";

			if (Arguments.Length > 0)
			{
				WildCardFilter =  Arguments[0];
			}

			//Console.WriteLine(String.Join(" ", Arguments));
			if (Debugger.IsAttached)
			{
				try
				{
					Console.SetWindowSize(160, 60);
					Console.SetBufferSize(160, 2000);
				}
				catch
				{
				}
			}

			if (WildCardFilter.Length > 0)
			{
				WildCardFilter = "*" + WildCardFilter + "*";
			}

			Init();
			Run(RunTestsViewOut, PspAutoTestsFolder, WildCardFilter);
			if (Debugger.IsAttached)
			{
				Console.WriteLine("Done");
				Console.ReadKey();
			}
		}

		public static void Main(bool RunTestsViewOut, String[] Arguments)
		{
			new AutoTestsProgram().InternalMain(RunTestsViewOut, Arguments);
		}			
	}
}
