using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio;
using CSPspEmu.Core.Audio.Imple.Openal;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Runner;
using CSharpUtils.Extensions;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace CSPspEmu.AutoTests
{
	public class Program
	{
		static PspConfig PspConfig;

		public class HleOutputHandlerMock : HleOutputHandler
		{
			public String OutputString = "";

			public override void Output(string OutputString)
			{
				this.OutputString += OutputString;
			}
		}

		static public void Init()
		{
			PspConfig = new PspConfig();
			foreach (var _FileName in new[] {
					Path.GetDirectoryName(typeof(Program).Assembly.Location) + @"\CSPspEmu.Hle.Modules.dll",
					Application.ExecutablePath,
				})
			{
				if (File.Exists(_FileName))
				{
					PspConfig.HleModulesDll = Assembly.LoadFile(_FileName);
					break;
				}
			}
		}

		static protected string RunExecutableAndGetOutput(string FileName)
		{
			var OutputString = "";

			GC.Collect();

			var PspEmulatorContext = new PspEmulatorContext(PspConfig);

			ConsoleUtils.CaptureOutput(() =>
			{				
				PspEmulatorContext.SetInstanceType<PspMemory, NormalPspMemory>();
				PspEmulatorContext.SetInstanceType<GpuImpl, GpuImplMock>();
				PspEmulatorContext.SetInstanceType<PspAudioImpl, AudioImplMock>();
				PspEmulatorContext.SetInstanceType<HleOutputHandler, HleOutputHandlerMock>();

				var Start = DateTime.Now;
				PspEmulatorContext.GetInstance<HleModuleManager>();
				var End = DateTime.Now;
				Console.WriteLine(End - Start);

				var GpuImpl = PspEmulatorContext.GetInstance<GpuImpl>();
				GpuImpl.InitSynchronizedOnce();

				var PspRunner = PspEmulatorContext.GetInstance<PspRunner>();
				PspRunner.StartSynchronized();
				{
					try
					{
						PspRunner.CpuComponentThread._LoadFile(FileName);
						if (!PspRunner.CpuComponentThread.StoppedEndedEvent.WaitOne(TimeSpan.FromSeconds(5)))
						{
							Console.Error.WriteLine("Timeout!");
						}
					}
					catch (Exception Exception)
					{
						Console.Error.WriteLine(Exception);
					}
				}

				PspRunner.StopSynchronized();
			}
			);

			var HleOutputHandlerMock = (HleOutputHandlerMock)PspEmulatorContext.GetInstance<HleOutputHandler>();
			OutputString = HleOutputHandlerMock.OutputString;

			return OutputString;
		}

		static protected void RunFile(string FileNameExecutable, string FileNameExpected)
		{
			Console.Write("{0}...", FileNameExecutable);
			var ExpectedOutput = File.ReadAllText(FileNameExpected, Encoding.ASCII);
			var RealOutput = RunExecutableAndGetOutput(FileNameExecutable);

			var ExpectedOutputLines = ExpectedOutput.Trim().Split('\n');
			var RealOutputLines = RealOutput.Trim().Split('\n');
			var Result = Diff.DiffTextProcessed(ExpectedOutputLines, RealOutputLines);

			if (Result.Items.All(Item => Item.Action == Diff.ProcessedItem.ActionEnum.Keep))
			{
				Console.WriteLine("Ok");
			}
			else
			{
				Console.WriteLine("Error");
				Result.Print();
			}
		}

		static protected string ExecuteBat(string ExecutableFileName, string Arguments, double TimeoutSeconds = -1)
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

		static protected void Run(string PspAutoTestsFolder, string WildCardFilter)
		{
			foreach (var FileNameExpected in Directory.GetFiles(PspAutoTestsFolder, "*.expected", SearchOption.AllDirectories))
			{
				var FileNameBaseBase = Path.GetFileNameWithoutExtension(FileNameExpected);
				var FileNameBase = Path.GetDirectoryName(FileNameExpected) + @"\" + FileNameBaseBase;
				var FileNameExecutable = FileNameBase + ".elf";
				var FileNameSourceCode = FileNameBase + ".c";

				if (!new Regex(Wildcard.WildcardToRegex(WildCardFilter)).IsMatch(FileNameBaseBase))
				{
					continue;
				}

				bool Recompile = false;
				//bool Recompile = true;

				if (File.GetLastWriteTime(FileNameExecutable) != File.GetLastWriteTime(FileNameSourceCode))
				{
					Recompile = true;
				}

				if (Recompile)
				{
					//PspAutoTestsFolder + @"\make.bat"
					// FileNameBase
					File.Delete(FileNameExecutable);
					var Output = ExecuteBat(PspAutoTestsFolder + @"\make_silent.bat", FileNameBase);
					if (Output != "")
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

				if (File.Exists(FileNameExecutable))
				{
					RunFile(FileNameExecutable, FileNameExpected);
				}
				else
				{
					Console.WriteLine("Can't find executable for '{0}' '{1}'", FileNameExpected, FileNameExecutable);
				}
			}
		}

		static public void Main(String[] Arguments)
		{
			var BasePath = Path.GetDirectoryName(Application.ExecutablePath);
			String PspAutoTestsFolder = "";

			foreach (var TryName in new[] { "pspautotests/tests", "../../../pspautotests/tests" })
			{
				if (Directory.Exists(BasePath + "/" + TryName))
				{
					PspAutoTestsFolder = new DirectoryInfo(BasePath + "/" + TryName).FullName;
					break;
				}
			}

			if (PspAutoTestsFolder == "")
			{
				Console.Error.WriteLine("Can't find 'pspautotests/tests' folder.");
			}

			Console.WriteLine(PspAutoTestsFolder);

			var WildCardFilter = "*";

			if (Arguments.Length > 0)
			{
				WildCardFilter = "*" + Arguments[0] + "*";
			}

			//Console.WriteLine(String.Join(" ", Arguments));
			if (Debugger.IsAttached)
			{
				Console.SetWindowSize(160, 60);
				Console.SetBufferSize(160, 2000);

				//WildCardFilter = "cpu";
			}
			Init();
			Run(PspAutoTestsFolder, WildCardFilter);
			if (Debugger.IsAttached)
			{
				Console.ReadKey();
			}
		}
	}
}
