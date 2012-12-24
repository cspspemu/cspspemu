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
using Mono.Simd;

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

		static void RunTests(bool RunTestsViewOut, string[] Arguments)
		{
			AutoTestsProgram.Main(RunTestsViewOut, Arguments.ToArray());
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
					Console.WriteLine("{0} : {1} : {2}.{3} : {4}", LogName, Level, Method.DeclaringType.Name, Method.Name, Text);
				}
			};

			Logger.Info("Running ... plat:{0} ... int*:{1} ... simd:{2}", Environment.Is64BitProcess ? "64bit" : "32bit", sizeof(int*), SimdRuntime.AccelMode);
#if false
			Console.WriteLine(CSPspEmu.Resources.Translations.GetString("extra", "UnknownGame"));
			Console.ReadKey(); Environment.Exit(0);
#endif

#if RUN_TESTS
			RunTests(Arguments);
#endif

			string FileToLoad = null;
			bool RunTestsViewOut = false;

			var Getopt = new Getopt(Arguments);
			{
				Getopt.AddRule(new[] { "/help", "/?", "-h", "--help", "-?" }, () =>
				{
					Console.WriteLine("Soywiz's Psp Emulator - {0} - r{1} - {2}", PspGlobalConfiguration.CurrentVersion, PspGlobalConfiguration.CurrentVersionNumeric, PspGlobalConfiguration.GitRevision);
					Console.WriteLine("");
					Console.WriteLine(" Switches:");
					Console.WriteLine("   /version             - Outputs the program version");
					Console.WriteLine("   /version2            - Outputs the program numeric version");
					Console.WriteLine("   /decrypt <EBOOT.BIN> - Decrypts an EBOOT.BIN");
					Console.WriteLine("   /gitrevision         - Outputs the git revision");
					Console.WriteLine("   /installat3          - Installs the WavDest filter. Requires be launched with administrative rights.");
					Console.WriteLine("   /associate           - Associates extensions with the program. Requires be launched with administrative rights.");
					Console.WriteLine("   /viewout /tests      - Run integration tests.");
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
				Getopt.AddRule("/decrypt", (string EncryptedFile) =>
				{
					try
					{
						using (var EncryptedStream = File.OpenRead(EncryptedFile))
						{
							/*
							var Format = new FormatDetector().DetectSubType(EncryptedStream);

							switch (Format)
							{
								case FormatDetector.SubType.Cso:
								case FormatDetector.SubType.Dax:
								case FormatDetector.SubType.Iso:

									break;
							}
							*/

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
				Getopt.AddRule("/installat3", () =>
				{
					var OutFile = Environment.SystemDirectory + @"\WavDest.dll";
					File.WriteAllBytes(OutFile, Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.References.WavDest.dll").ReadAll());
					ProcessUtils.ExecuteCommand("regsvr32", String.Format(@"/s ""{0}"" ", OutFile));
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
				Getopt.AddRule("/tests", () =>
				{
					RunTests(RunTestsViewOut, Getopt.DequeueAllNext());
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
			//new PspAudioOpenalImpl().__TestAudio();
			//new PspAudioWaveOutImpl().__TestAudio();
			//return;

			/*
			var CsoName = "../../../TestInput/test.cso";
			var Cso = new Cso(File.OpenRead(CsoName));
			var Iso = new IsoFile();
			Console.WriteLine("[1]");
			Iso.SetStream(new CsoProxyStream(Cso), CsoName);
			Console.WriteLine("[2]");
			foreach (var Node in Iso.Root.Descendency())
			{
				Console.WriteLine(Node);
			}
			Console.ReadKey();
			return;
			*/

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
