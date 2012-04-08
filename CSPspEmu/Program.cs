using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ComponentAce.Compression.Libs.zlib;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Vfs.Iso;
using CSPspEmu.Core.Audio.Impl.WaveOut;
using CSPspEmu.Core.Audio.Impl.Openal;
using Microsoft.Win32;
using CSPspEmu.Core;
using CSPspEmu.Resources;
using CSPspEmu.AutoTests;
using System.Diagnostics;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSharpUtils.Getopt;
using Codegen;
using CSPspEmu.Gui.Winforms;

namespace CSPspEmu
{
	unsafe class Program
	{
		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://en.wikipedia.org/wiki/Common_Intermediate_Language"/>
		/// <see cref="http://en.wikipedia.org/wiki/List_of_CIL_instructions"/>
		/// <see cref="http://www.microsoft.com/downloads/details.aspx?FamilyID=22914587-b4ad-4eae-87cf-b14ae6a939b0&displaylang=en" />
		/// <param name="args"></param>
		[STAThread]
		static void Main(string[] Arguments)
		{
#if false
			Console.WriteLine(CSPspEmu.Resources.Translations.GetString("extra", "UnknownGame"));
			Console.ReadKey(); Environment.Exit(0);
#endif

#if RUN_TESTS
			TestsAutoProgram.Main(Arguments.Skip(0).ToArray());
			Environment.Exit(0);
#endif
			//AppDomain.UnHandledException
			/*
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				try
				{
					Console.Error.WriteLine(e.ExceptionObject);
				}
				catch
				{
				}
				Console.ReadKey();
				Environment.Exit(-1);
			};
			*/

			//Application.EnableVisualStyles(); Application.Run(new GameListForm()); Application.Exit();

			string FileToLoad = null;

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
					Console.WriteLine("   /tests               - Run integration tests.");
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
					File.WriteAllBytes(OutFile, Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.WavDest.dll").ReadAll());
					Process.Start(new ProcessStartInfo("regsvr32", String.Format(@"/s ""{0}"" ", OutFile))).WaitForExit();
					Environment.Exit(0);
				});
				Getopt.AddRule("/associate", () =>
				{
					try
					{
						Registry.ClassesRoot.CreateSubKey(".elf").SetValue(null, "cspspemu.executable");
						Registry.ClassesRoot.CreateSubKey(".pbp").SetValue(null, "cspspemu.executable");
						Registry.ClassesRoot.CreateSubKey(".cso").SetValue(null, "cspspemu.executable");
						Registry.ClassesRoot.CreateSubKey(".prx").SetValue(null, "cspspemu.executable");
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
				Getopt.AddRule("/tests", () =>
				{
					TestsAutoProgram.Main(Arguments.Skip(1).ToArray());
					Environment.Exit(0);
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
