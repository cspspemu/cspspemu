//#define ENABLE_RECOMPILE

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Hle;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Runner;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Drawing;
using CSharpUtils.Drawing;
using CSharpUtils.Extensions;
using CSPspEmu.Compat;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Gpu.Impl.Null;
using CSPspEmu.Utils;

namespace CSPspEmu.AutoTests
{
    public class AutoTestsProgram
    {
        static TimeSpan _timeoutTime = 0.Milliseconds();

        public class HleOutputHandlerMock : HleOutputHandler
        {
            public string OutputString = "";

            public override void Output(string outputString)
            {
                this.OutputString += outputString;
            }
        }

        public HleConfig HleConfig;
        public PspStoredConfig StoredConfig;

        public AutoTestsProgram(HleConfig hleConfig, PspStoredConfig storedConfig)
        {
            HleConfig = hleConfig;
            StoredConfig = storedConfig;
        }

        public void Init()
        {
            foreach (var fileName in new[]
            {
                Path.GetDirectoryName(typeof(AutoTestsProgram).Assembly.Location) + @"\V",
                Application.ExecutablePath,
            })
            {
                if (File.Exists(fileName))
                {
                    HleConfig.HleModulesDll = Assembly.LoadFile(fileName);
                    break;
                }
            }
        }

        protected string RunExecutableAndGetOutput(bool runTestsViewOut, string pspAutoTestsFolder, string fileName,
            out string capturedOutput, string fileNameBase)
        {
            var outputString = "";

            IHleIoDriver hostDriver = null;

            //Console.WriteLine(FileNameBase);

            InjectContext injectContext = null;
            {
                //var Capture = false;
                var capture = !runTestsViewOut;
                capturedOutput = ConsoleUtils.CaptureOutput(() =>
                    {
                        injectContext = PspInjectContext.CreateInjectContext(StoredConfig, test: true);
                        injectContext.SetInstanceType<HleOutputHandler, HleOutputHandlerMock>();

                        var cpuConfig = injectContext.GetInstance<CpuConfig>();
                        var hleConfig = injectContext.GetInstance<HleConfig>();
                        cpuConfig.DebugSyscalls = false;
                        cpuConfig.ShowInstructionStats = false;
                        hleConfig.TraceLastSyscalls = false;
                        hleConfig.DebugSyscalls = false;

                        //Console.Error.WriteLine("[1]");

                        var start = DateTime.UtcNow;
                        injectContext.GetInstance<HleModuleManager>();
                        var end = DateTime.UtcNow;
                        Console.WriteLine(end - start);

                        //Console.Error.WriteLine("[a]");

                        // GPU -> NULL
                        //PspEmulatorContext.SetInstanceType<GpuImpl>(typeof(GpuImplOpenglEs));
                        injectContext.SetInstanceType<GpuImpl>(typeof(GpuImplNull));

                        var gpuImpl = injectContext.GetInstance<GpuImpl>();
                        //GpuImpl.InitSynchronizedOnce();

                        //Console.Error.WriteLine("[b]");

                        var pspRunner = injectContext.GetInstance<PspRunner>();
                        pspRunner.StartSynchronized();

                        //Console.Error.WriteLine("[c]");

                        {
                            try
                            {
                                //PspRunner.CpuComponentThread.SetIso(PspAutoTestsFolder + "/../input/test.cso");
                                pspRunner.CpuComponentThread.SetIso(pspAutoTestsFolder + "/../input/cube.cso");
                                //Console.Error.WriteLine("[2]");

                                var hleIoManager = injectContext.GetInstance<HleIoManager>();
                                hostDriver = hleIoManager.GetDriver("host:");

                                try
                                {
                                    hostDriver.IoRemove(null, "/__testoutput.txt");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                                try
                                {
                                    hostDriver.IoRemove(null, "/__testerror.txt");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }

                                injectContext.GetInstance<PspHleRunningConfig>().FileNameBase = fileNameBase;
                                pspRunner.CpuComponentThread._LoadFile(fileName);
                                //Console.Error.WriteLine("[3]");
                                if (!pspRunner.CpuComponentThread.StoppedEndedEvent.WaitOne(_timeoutTime))
                                {
                                    ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red,
                                        () => { Console.Error.WriteLine("Timeout!"); });
                                }
                            }
                            catch (Exception e)
                            {
                                Console.Error.WriteLine(e);
                            }
                        }

                        pspRunner.StopSynchronized();
                        GC.Collect();

                        using var testOutput = hostDriver.OpenRead("/__testoutput.txt");
                        outputString = testOutput.ReadAllContentsAsString();
                    },
                    capture: capture
                );

                //var HleOutputHandlerMock = (HleOutputHandlerMock)PspEmulatorContext.GetInstance<HleOutputHandler>();
                //OutputString = HleOutputHandlerMock.OutputString;
            }
            injectContext.Dispose();

            return outputString;
        }

        protected void RunFile(bool runTestsViewOut, string pspAutoTestsFolder, string fileNameExecutable,
            string fileNameExpected, string fileNameBase)
        {
            ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.DarkCyan,
                () => { Console.Write("{0}...", fileNameExecutable); });
            var expectedOutput = File.ReadAllText(fileNameExpected, Encoding.ASCII);
            var realOutput = "";
            var capturedOutput = "";

            // Execute.
            {
                realOutput = RunExecutableAndGetOutput(runTestsViewOut, pspAutoTestsFolder, fileNameExecutable,
                    out capturedOutput, fileNameBase);
            }

            var expectedOutputLines = expectedOutput.Trim().Replace("\r\n", "\n").Split('\n');
            var realOutputLines = realOutput.Trim().Replace("\r\n", "\n").Split('\n');
            var result = Diff.DiffTextProcessed(expectedOutputLines, realOutputLines);

            File.WriteAllText(
                Path.ChangeExtension(fileNameExpected, ".lastoutput"),
                realOutput
            );

            File.WriteAllText(
                Path.ChangeExtension(fileNameExpected, ".lastdebug"),
                capturedOutput
            );

            var hadAnError = false;
            for (var n = 0; n < 10; n++)
            {
                var imageReferenceFile = $"{fileNameBase}.reference.{n}.png";
                var imageOutputFile = $"{fileNameBase}.lastoutput.{n}.png";
                if (File.Exists(imageReferenceFile))
                {
                    if (File.Exists(imageOutputFile))
                    {
                        var referenceBitmap = new Bitmap(imageReferenceFile);
                        var outputBitmap = new Bitmap(imageOutputFile);
                        if (referenceBitmap.Size == outputBitmap.Size)
                        {
                            var compareResult = BitmapUtils.CompareBitmaps(referenceBitmap, outputBitmap, 0.01);

                            if (compareResult.Equal)
                            {
                                Console.Error.WriteLine(
                                    "Files '{0}:{1}' and '{2}:{3}' have different contents {4}/{5} different pixels {6}%",
                                    imageReferenceFile, referenceBitmap.Size, imageOutputFile, outputBitmap.Size,
                                    compareResult.DifferentPixelCount, compareResult.TotalPixelCount,
                                    compareResult.PixelTotalDifferencePercentage
                                );
                                hadAnError = true;
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine(
                                "Files '{0}:{1}' and '{2}:{3}' have different sizes",
                                imageReferenceFile, referenceBitmap.Size, imageOutputFile, outputBitmap.Size
                            );
                            hadAnError = true;
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine(
                            "File '{0}' exists, but not exists '{1}'",
                            imageReferenceFile, imageOutputFile
                        );
                        hadAnError = true;
                    }
                }
            }

            if (result.Items.Any(item => item.Action != Diff.ProcessedItem.ActionEnum.Keep)) hadAnError = true;

            if (!hadAnError)
            {
                ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Green, () => { Console.WriteLine("Ok"); });
            }
            else
            {
                ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () => { Console.WriteLine("Error"); });
                result.Print(avoidKeep: true);
            }

            File.WriteAllText(
                Path.ChangeExtension(fileNameExpected, ".lastdiff"),
                result.ToString()
            );
        }

        protected string ExecuteBat(string executableFileName, string arguments, double timeoutSeconds = -1)
        {
            var process = new Process(); // Declare New Process
            //proc.StartInfo.FileName = fileName;
            //proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //proc.StartInfo.CreateNoWindow = true;

            process.StartInfo.FileName = executableFileName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;

            process.Start();


            if (timeoutSeconds < 0)
            {
                process.WaitForExit();
            }
            else
            {
                process.WaitForExit((int) TimeSpan.FromSeconds(timeoutSeconds).TotalMilliseconds);
            }

            var errorMessage = process.StandardError.ReadToEnd();
            process.WaitForExit();

            var outputMessage = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            process.Start();
            process.WaitForExit();

            return errorMessage + outputMessage;
        }

        protected void Run(bool runTestsViewOut, string pspAutoTestsFolder, string wildCardFilter)
        {
            foreach (var fileNameExpected in Directory.GetFiles(pspAutoTestsFolder, "*.expected",
                SearchOption.AllDirectories))
            {
                var fileNameBaseBase = Path.GetFileNameWithoutExtension(fileNameExpected);
                var fileNameBase = Path.GetDirectoryName(fileNameExpected) + @"\" + fileNameBaseBase;
                var fileNameExecutable = fileNameBase + ".prx";
                var fileNameSourceCode = fileNameBase + ".c";

                var matchName = fileNameBase.Substr(pspAutoTestsFolder.Length).Replace("\\", "/");

                //Console.WriteLine(MatchName + " ~ " + Wildcard.WildcardToRegex(WildCardFilter));
                if (!new Regex(Wildcard.WildcardToRegex(wildCardFilter)).IsMatch(matchName))
                {
                    continue;
                }

                var recompile = false;
                //bool Recompile = true;

                if (File.GetLastWriteTime(fileNameExecutable) != File.GetLastWriteTime(fileNameSourceCode))
                {
                    recompile = true;
                }

#if ENABLE_RECOMPILE
				if (recompile)
				{
					//PspAutoTestsFolder + @"\make.bat"
					// FileNameBase
					try { File.Delete(FileNameExecutable); } catch (Exception e) { Console.WriteLine(e); }
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

                if (File.Exists(fileNameExecutable))
                {
                    RunFile(runTestsViewOut, pspAutoTestsFolder, fileNameExecutable, fileNameExpected, fileNameBase);
                }
                else
                {
                    Console.WriteLine("Can't find executable for '{0}' '{1}'", fileNameExpected, fileNameExecutable);
                }
            }
        }

        private void InternalMain(bool runTestsViewOut, string[] arguments)
        {
            var basePath = Path.GetDirectoryName(Application.ExecutablePath);
            var pspAutoTestsFolder = "";

            foreach (var tryName in new[] {"pspautotests/tests", "../../../pspautotests/tests"})
            {
                if (Directory.Exists(basePath + "/" + tryName))
                {
                    pspAutoTestsFolder = new DirectoryInfo(basePath + "/" + tryName).FullName;
                    break;
                }
            }

            if (string.IsNullOrEmpty(pspAutoTestsFolder))
            {
                Console.Error.WriteLine("Can't find 'pspautotests/tests' folder.");
                Console.ReadKey();
            }

            Console.WriteLine(pspAutoTestsFolder);

            var wildCardFilter = "*";

            if (arguments.Length > 0)
            {
                wildCardFilter = arguments[0];
            }

            //Console.WriteLine(String.Join(" ", Arguments));
            if (Debugger.IsAttached)
            {
                try
                {
                    Console.SetWindowSize(160, 60);
                    Console.SetBufferSize(160, 2000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (wildCardFilter.Length > 0)
            {
                wildCardFilter = "*" + wildCardFilter + "*";
            }

            Init();
            Run(runTestsViewOut, pspAutoTestsFolder, wildCardFilter);
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Done");
                Console.ReadKey();
            }
        }

        public static void Main(bool runTestsViewOut, string[] arguments, int timeout)
        {
            _timeoutTime = timeout.Seconds();
            new InjectContext().GetInstance<AutoTestsProgram>().InternalMain(runTestsViewOut, arguments);
        }
    }
}