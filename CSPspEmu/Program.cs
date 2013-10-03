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
using CSPspEmu.Hle.Vfs.Iso;
using System.Diagnostics;
using CSharpPlatform.GL;
using CSharpPlatform.GL.Utils;
using CSPspEmu.Core.Gpu;
using System.Runtime.InteropServices;
using CSharpPlatform.GL.Impl;
using System.Windows;
using System.Drawing;
using CSPspEmu.Gui.SMAA;
using CSPspEmu.Gui.XBR.Shader;

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

		static void RunTests(bool RunTestsViewOut, string[] Arguments, int Timeout)
		{
			AutoTestsProgram.Main(RunTestsViewOut, Arguments.ToArray(), Timeout);
			Environment.Exit(0);
		}

		static private GLTexture GLTextureCreateFromBitmap(Bitmap Bitmap)
		{
			return GLTexture.Create()
				.SetFormat(TextureFormat.RGBA)
				.SetSize(Bitmap.Width, Bitmap.Height)
				.SetData(Bitmap.GetChannelsDataInterleaved(BitmapChannelList.RGBA))
			;
		}

		static private void _MainData()
		{
			var Context = GLContextFactory.CreateWindowless().MakeCurrent();
			//var BitmapIn = new Bitmap(Image.FromFile(@"C:\temp\1.png"));
			var BitmapIn = new Bitmap(Image.FromFile(@"C:\temp\in.png"));

			Console.WriteLine("{0}", String.Join("\n", Assembly.GetExecutingAssembly().GetManifestResourceNames()));

			var TextureOut = new XBRShader().Process(GLTextureCreateFromBitmap(BitmapIn));

			new Bitmap(TextureOut.Width, TextureOut.Height).SetChannelsDataInterleaved(TextureOut.GetDataFromGpu(), BitmapChannelList.RGBA).Save(@"c:\temp\out.png");

			//var Smaa = new Smaa();
			//var TextureIn = GLTextureCreateFromBitmap(BitmapIn);
			//var TextureOut = Smaa.Process(TextureIn, null);
			//
			//new Bitmap(TextureOut.Width, TextureOut.Height).SetChannelsDataInterleaved(TextureOut.GetDataFromGpu(), BitmapChannelList.RGBA).Save(@"c:\temp\test.out.png");
			//
			//File.WriteAllBytes(@"c:\temp\test.out.bin", TextureOut.GetDataFromGpu());

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
		unsafe static void Main(string[] Arguments)
		{
			//Console.WriteLine(GL.GetConstantString(GL.GL_TEXTURE_2D));
			//_MainData();
			//_MainData2();
			
			if (!IsNet45OrNewer())
			{
				MessageBox.Show(".NET 4.5 required", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
				return;
			}

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
			int RunTestsTimeout = 60;

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
			{
				var MonoRuntimeType = Type.GetType("Mono.Runtime");
				if (MonoRuntimeType != null)
				{
					var GetDisplayNameMethod = MonoRuntimeType.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
					if (GetDisplayNameMethod != null) Console.WriteLine("Mono: {0}", GetDisplayNameMethod.Invoke(null, null));
				}
			}
			Console.WriteLine("ImageRuntimeVersion: {0}", Assembly.GetExecutingAssembly().ImageRuntimeVersion);

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

			/*
			foreach (var NI in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (NI.SupportsMulticast && NI.OperationalStatus == OperationalStatus.Up)
				{
					var IPProperties = NI.GetIPProperties();
					Console.WriteLine("[A]:{0}", NI.ToStringDefault());
					foreach (var Item in IPProperties.DhcpServerAddresses)
					{
						Console.WriteLine("[B]:{0},{1}", Item.ToString(), Item.IsIPv6Multicast);
					}
					foreach (var Item in IPProperties.AnycastAddresses)
					{
						Console.WriteLine("[D]:{0}", Item.Address.ToString());
					}
					foreach (var Item in IPProperties.MulticastAddresses)
					{
						Console.WriteLine("[E]:{0}", Item.Address.ToString());
					}
					foreach (var Item in IPProperties.UnicastAddresses)
					{
						Console.WriteLine("[F]:{0}", Item.Address.ToString());
					}
					Console.WriteLine("[G]:{0}", NI.GetPhysicalAddress());
				}
				else
				{
					Console.WriteLine("-");
				}
			}
			*/

			using (var PspEmulator = new PspEmulator())
			{
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
}
