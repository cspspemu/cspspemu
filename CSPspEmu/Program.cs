using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Core;
using CSPspEmu.AutoTests;
using CSharpUtils;
using CSharpUtils.Getopt;
using CSPspEmu.Hle;
using CSPspEmu.Hle.Vfs.Iso;
using System.Diagnostics;

namespace CSPspEmu
{
	unsafe class Program
	{
		static Logger Logger = Logger.GetLogger("Program");

		private static void Form1_UIThreadException(object sender, ThreadExceptionEventArgs e)
		{
			Console.Error.WriteLine(e.Exception);
			Application.Exit();
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Console.Error.WriteLine(e.ExceptionObject);
			Application.Exit();
		}

		public static bool IsNet45OrNewer()
		{
			// Class "ReflectionContext" exists from .NET 4.5 onwards.
			return Type.GetType("System.Reflection.ReflectionContext", false) != null;
		}

		static void RunTests(bool RunTestsViewOut, string[] Arguments, int Timeout = 10)
		{
			AutoTestsProgram.Main(RunTestsViewOut, Arguments.ToArray(), Timeout);
			Environment.Exit(0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://en.wikipedia.org/wiki/Common_Intermediate_Language"/>
		/// <see cref="http://en.wikipedia.org/wiki/List_of_CIL_instructions"/>
		/// <see cref="http://www.microsoft.com/downloads/details.aspx?FamilyID=22914587-b4ad-4eae-87cf-b14ae6a939b0&displaylang=en" />
		/// <param name="Arguments"></param>
		[STAThread]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
		static void Main(string[] Arguments)
		{
			//Thread.Sleep(1000);
			//var Cso = new Cso(new MemoryStream(File.ReadAllBytes(@"F:\isos\psp\luxor.cso")));
			//var Cso = new Cso(File.OpenRead(@"F:\isos\psp\Puyo.cso"));
			//var Stopwatch1 = new Stopwatch();
			//Stopwatch1.Start();
			////var IsoBytes = File.ReadAllBytes("../../../TestInput/cube.iso");
			////Assert.AreEqual(ExpectedNumberOfBlocks, Cso.NumberOfBlocks);
			////Assert.AreEqual(ExpectedBlockSize, Cso.BlockSize);
			//for (uint Block = 0; Block < Cso.NumberOfBlocks; Block++)
			//{
			//	var DecompressedBlockData = Cso.ReadBlockDecompressed(Block);
			//	//CollectionAssert.AreEqual(
			//	//	IsoBytes.Skip((int)(ExpectedBlockSize * Block)).Take(ExpectedBlockSize).ToArray(),
			//	//	DecompressedBlockData.ToArray()
			//	//);
			//}
			//Console.WriteLine(Stopwatch1.Elapsed);
			//Console.ReadKey();
			//return;
			//Environment.Exit(0);
			//Arguments = new[] { "/isoconvert", @"f:\isos\psp\Princess Crown.cso", @"c:\isos\psp\pricess.iso" };
			//Arguments = new[] { "/isoextract", @"f:\isos\psp\Puyo.cso", @"c:\isos\psp\puyo" };

			//MiniPlayer.Play(
			//	File.OpenRead(@"F:\isos\psp2\temp\1C-99-F2-16-B6-41-D9-27-8D-41-80-6A-AB-D1-EB-77-29-61-17-0F.oma"),
			//	File.OpenWrite(@"F:\isos\psp2\temp\1C-99-F2-16-B6-41-D9-27-8D-41-80-6A-AB-D1-EB-77-29-61-17-0F.raw")
			//); return;

			if (!IsNet45OrNewer())
			{
				MessageBox.Show(".NET 4.5 required", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
				return;
			}
#if false
			var Test = new byte[4 * 1024 * 1024];
			Console.WriteLine(Logger.Measure(() =>
			{
				for (int n = 0; n < 32; n++) PointerUtils.Memset(Test, 0x7F, Test.Length);
			}));

			Console.WriteLine(Logger.Measure(() =>
			{
				for (int n = 0; n < 32; n++) PointerUtils.MemsetSlow(Test, 0x7F, Test.Length);
			}));
#endif
#if false
			Console.WriteLine("SIMD Test: {0}", Logger.Measure(() =>
			{
				for (int n = 0; n < 2000000; n++)
				{
					var Test = default(Vector4f);
					Test += new Vector4f(1, 1, 1, 1);
					Test += new Vector4f(1, 1, 1, 1);
					Test += new Vector4f(1, 1, 1, 1);
					Test += new Vector4f(1, 1, 1, 1);
					Test += new Vector4f(1, 1, 1, 1);
					Test += new Vector4f(1, 1, 1, 1);
				}
			}));
			//SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Add");
#endif

			// Add the event handler for handling UI thread exceptions to the event.
			Application.ThreadException += new ThreadExceptionEventHandler(Form1_UIThreadException);

			// Set the unhandled exception mode to force all Windows Forms errors to go through
			// our handler.
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			// Add the event handler for handling non-UI thread exceptions to the event. 
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			Logger.OnGlobalLog += (LogName, Level, Text, StackFrame) =>
			{
				if (Level >= Logger.Level.Info)
				{
					var Method = StackFrame.GetMethod();
					Console.WriteLine("{0} : {1} : {2}.{3} : {4}", LogName, Level, (Method.DeclaringType != null) ? Method.DeclaringType.Name : null, Method.Name, Text);
				}
			};

#if false
			Console.WriteLine(CSPspEmu.Resources.Translations.GetString("extra", "UnknownGame"));
			Console.ReadKey(); Environment.Exit(0);
#endif

#if RUN_TESTS
			RunTests(Arguments);
#endif

			string FileToLoad = null;
			bool RunTestsViewOut = false;
			int RunTestsTimeout = 30;

			var Getopt = new Getopt(Arguments);
			{
				Getopt.AddRule(new[] { "/help", "/?", "-h", "--help", "-?" }, () =>
				{
					Console.WriteLine("Soywiz's Psp Emulator - {0} - r{1} - {2}", PspGlobalConfiguration.CurrentVersion, PspGlobalConfiguration.CurrentVersionNumeric, PspGlobalConfiguration.GitRevision);
					Console.WriteLine("");
					Console.WriteLine(" Switches:");
					Console.WriteLine("   /version                         - Outputs the program version");
					Console.WriteLine("   /version2                        - Outputs the program numeric version");
					Console.WriteLine("   /decrypt <EBOOT.BIN>             - Decrypts an EBOOT.BIN");
					Console.WriteLine("   /gitrevision                     - Outputs the git revision");
					Console.WriteLine("   /associate                       - Associates extensions with the program. Requires be launched with administrative rights.");
					Console.WriteLine("   /viewout /timeout X /tests       - Run integration tests.");
					Console.WriteLine("   ");
					Console.WriteLine("   /isolist <pathto.iso|cso|dax>    - Lists the content of an iso.");
					Console.WriteLine("   /isoextract <in.iso> <outfolder> - Extracts the content of an iso.");
					Console.WriteLine("   /isoconvert <in.xxx> <out.yyy>   - Converts a iso/cso/dax file into other format.");
					Console.WriteLine("");
					Console.WriteLine(" Examples:");
					Console.WriteLine("   cspspemu.exe <path_to_psp_executable>");
					Console.WriteLine("");
					Environment.Exit(0);
				});
				Getopt.AddRule("/version", () =>
				{
					Console.Write("{0}", PspGlobalConfiguration.CurrentVersion);
					Environment.Exit(0);
				});
				Getopt.AddRule("/version2", () =>
				{
					Console.Write("{0}", PspGlobalConfiguration.CurrentVersionNumeric);
					Environment.Exit(0);
				});
				Getopt.AddRule("/isoconvert", () =>
				{
					var IsoInPath = Getopt.DequeueNext();
					var IsoOutPath = Getopt.DequeueNext();
					
					if (Path.GetExtension(IsoOutPath) != ".iso")
					{
						Console.WriteLine("Just support outputing .iso files");
						Environment.Exit(-1);
					}

					var IsoInFile = IsoLoader.GetIso(IsoInPath);
					var Stopwatch = new Stopwatch();
					Stopwatch.Start();
					Console.Write("{0} -> {1}...", IsoInPath, IsoOutPath);
					IsoInFile.Stream.Slice().CopyToFile(IsoOutPath);
					Console.WriteLine("Ok ({0})", Stopwatch.Elapsed);
					Environment.Exit(0);
				});
				Getopt.AddRule("/isolist", () =>
				{
					var IsoPath = Getopt.DequeueNext();
					var IsoFile = IsoLoader.GetIso(IsoPath);
					var IsoFileSystem = new HleIoDriverIso(IsoFile);
					foreach (var FileName in IsoFileSystem.ListDirRecursive("/"))
					{
						var Stat = IsoFileSystem.GetStat(FileName);
						Console.WriteLine("{0} : {1}", FileName, Stat.Size);
					}
					//Console.Write("{0}", PspGlobalConfiguration.CurrentVersionNumeric);
					Environment.Exit(0);
				});
				Getopt.AddRule("/isoextract", () =>
				{
					var IsoPath = Getopt.DequeueNext();
					var OutputPath = Getopt.DequeueNext();
					var IsoFile = IsoLoader.GetIso(IsoPath);
					var IsoFileSystem = new HleIoDriverIso(IsoFile);
					foreach (var FileName in IsoFileSystem.ListDirRecursive("/"))
					{
						var Stat = IsoFileSystem.GetStat(FileName);
						var OutputFileName = OutputPath + "/" + FileName;
						Console.Write("{0} : {1}...", FileName, Stat.Size);

						if (!Stat.Attributes.HasFlag(Hle.Vfs.IOFileModes.Directory))
						{
							var ParentDirectory = Directory.GetParent(OutputFileName).FullName;
							//Console.WriteLine(ParentDirectory);
							try { Directory.CreateDirectory(ParentDirectory); }
							catch
							{
							}
							using (var InputStream = IsoFileSystem.OpenRead(FileName))
							{
								InputStream.CopyToFile(OutputFileName);
							}
						}
						Console.WriteLine("Ok");
					}
					//Console.Write("{0}", PspGlobalConfiguration.CurrentVersionNumeric);
					Environment.Exit(0);
				});
				Getopt.AddRule("/decrypt", (string EncryptedFile) =>
				{
					try
					{
						using (var EncryptedStream = File.OpenRead(EncryptedFile))
						{
							var DecryptedFile = String.Format("{0}.decrypted", EncryptedFile);
							Console.Write("'{0}' -> '{1}'...", EncryptedFile, DecryptedFile);

							var EncryptedData = EncryptedStream.ReadAll();
							var DecryptedData = new EncryptedPrx().Decrypt(EncryptedData);
							File.WriteAllBytes(DecryptedFile, DecryptedData);
							Console.WriteLine("Ok");
							Environment.Exit(0);
						}
					}
					catch (Exception Exception)
					{
						Console.Error.WriteLine(Exception);
						Environment.Exit(-1);
					}
				});
				Getopt.AddRule("/gitrevision", () =>
				{
					Console.Write("{0}", PspGlobalConfiguration.GitRevision);
					Environment.Exit(0);
				});
				Getopt.AddRule("/associate", () =>
				{
					try
					{
						Registry.ClassesRoot.CreateSubKey(".pbp").SetValue(null, "cspspemu.executable");
						Registry.ClassesRoot.CreateSubKey(".elf").SetValue(null, "cspspemu.executable");
						Registry.ClassesRoot.CreateSubKey(".prx").SetValue(null, "cspspemu.executable");
						Registry.ClassesRoot.CreateSubKey(".cso").SetValue(null, "cspspemu.executable");
						Registry.ClassesRoot.CreateSubKey(".dax").SetValue(null, "cspspemu.executable");

						var Reg = Registry.ClassesRoot.CreateSubKey("cspspemu.executable");
						Reg.SetValue(null, "PSP executable file (.elf, .pbp, .cso, .prx, .dax)");
						Reg.SetValue("DefaultIcon", @"""" + ApplicationPaths.ExecutablePath + @""",0");
						Reg.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue(null, @"""" + ApplicationPaths.ExecutablePath + @""" ""%1""");

						Environment.Exit(0);
					}
					catch (Exception Exception)
					{
						Console.Error.WriteLine(Exception);
						Environment.Exit(-1);
					}
				});
				Getopt.AddRule("/viewout", () =>
				{
					RunTestsViewOut = true;
				});
				Getopt.AddRule("/timeout", (int seconds) =>
				{
					RunTestsTimeout = seconds;
				});
				Getopt.AddRule("/tests", () =>
				{
					RunTests(RunTestsViewOut, Getopt.DequeueAllNext(), RunTestsTimeout);
				});
				Getopt.AddRule((Name) =>
				{
				    FileToLoad = Name;
				});
			}
			try
			{
				Getopt.Process();
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				Environment.Exit(-1);
			}

			Logger.Info("Running ... plat:{0} ... int*:{1}", Environment.Is64BitProcess ? "64bit" : "32bit", sizeof(int*));

#if !RELEASE
			try
			{
				Console.OutputEncoding = Encoding.UTF8;
				Console.SetWindowSize(160, 60);
				Console.SetBufferSize(160, 2000);
			}
			catch
			{
			}
#endif
			var PspEmulator = new PspEmulator();
			//PspEmulator.UseFastMemory = true;
			var CodeBase = Assembly.GetExecutingAssembly().Location;
			var Base = Path.GetDirectoryName(CodeBase) + @"\" + Path.GetFileNameWithoutExtension(CodeBase);
			foreach (var TryExtension in new[] { "iso", "cso", "elf", "pbp" })
			{
				var TryIsoFile = Base + "." + TryExtension;

				//Console.WriteLine(TryIsoFile);
				//Console.ReadKey();

				if (File.Exists(TryIsoFile))
				{
					Platform.HideConsole();

					PspEmulator.StartAndLoad(TryIsoFile, TraceSyscalls: false, ShowMenus: false);
					return;
				}
			}

			if (FileToLoad != null)
			{
				PspEmulator.StartAndLoad(FileToLoad, TraceSyscalls: false);
			}
			else
			{
				//StartWithoutArguments(PspEmulator);
				PspEmulator.Start();
			}
		}
	}
}
