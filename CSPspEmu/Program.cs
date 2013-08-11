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



		//static private void _MainData()
		//{
		//	var Form = new Form();
		//	//Form.CreateControl();
		//	Form.Controls.Add(new GLControl());
		//	Form.Show();
		//
		//	Application.Run(Form);
		//
		//	Console.ReadKey();
		//
		//	var IdentityMatrix = default(GLMatrix4);
		//	IdentityMatrix.LoadIdentity();
		//
		//	var Context = OpenglContextFactory.CreateWindowless();
		//
		//	//var RenderTarget = GLRenderTarget.Default;
		//	var RenderTarget = GLRenderTarget.Create(128, 128);
		//	RenderTarget.Bind();
		//
		//	GL.glClearColor(0.5f, 0, 1, 1);
		//	GL.glClear(GL.GL_COLOR_BUFFER_BIT);
		//
		//	//foreach (var Name in typeof(GpuConfig).Assembly.GetManifestResourceNames())
		//	//{
		//	//	Console.WriteLine(Name);
		//	//}
		//	//Console.ReadKey();
		//
		//	var Shader = new GLShader(
		//		typeof(GpuConfig).Assembly.GetManifestResourceStream("CSPspEmu.Core.Gpu.Impl.Opengl.shader.vert").ReadAllContentsAsString(),
		//		typeof(GpuConfig).Assembly.GetManifestResourceStream("CSPspEmu.Core.Gpu.Impl.Opengl.shader.frag").ReadAllContentsAsString()
		//	);
		//
		//	var vertexPosition = Shader.GetAttribute("vertexPosition");
		//	var UniformMatrixWorld = Shader.GetUniform("matrixWorld");
		//	var UniformMatrixView = Shader.GetUniform("matrixView");
		//	var UniformMatrixProjection = Shader.GetUniform("matrixProjection");
		//
		//	var Vertices = new float[]
		//	{
		//		0f, 0f, 0f, 0f,
		//		1f, 0f, 0f, 0f,
		//		1f, 1f, 0f, 0f,
		//		0f, 1f, 0f, 0f,
		//	};
		//
		//	Shader.Use();
		//	Console.WriteLine(vertexPosition);
		//
		//	UniformMatrixWorld.Set(IdentityMatrix);
		//	UniformMatrixView.Set(IdentityMatrix);
		//	UniformMatrixProjection.Set(IdentityMatrix);
		//
		//	var Buffer = new GLBuffer();
		//
		//	fixed (float* VerticesPtr = Vertices)
		//	{
		//		//vPosition.SetPointer(VerticesPtr);
		//		vertexPosition.SetData<float>(Buffer.SetData(Vertices.Length * sizeof(float), VerticesPtr));
		//		GL.glDrawArrays(GL.GL_TRIANGLE_STRIP, 0, 4);
		//	}
		//
		//
		//	var Pixels = RenderTarget.ReadPixels();
		//	Console.WriteLine("{0:X2}{1:X2}{2:X2}{3:X2}", Pixels[0], Pixels[1], Pixels[2], Pixels[3]);
		//	File.WriteAllBytes(@"c:\temp\out.bin", Pixels);
		//
		//	//Console.WriteLine("{0}", Marshal.PtrToStringAnsi(new IntPtr(GL.glGetString(GL.GL_VERSION))));
		//	//Console.WriteLine(Context);
		//	Console.ReadKey();
		//	Environment.Exit(0);
		//}

		//static private void _MainData2()
		//{
		//	var Context = OpenglContextFactory.CreateWindowless();
		//	Context.MakeCurrent();
		//
		//	//var RenderTarget = GLRenderTarget.Default;
		//	var RenderTarget = GLRenderTarget.Create(1024, 1024);
		//
		//	var Shader = new GLShader(
		//		@"
		//			attribute vec4 vertexPosition;
		//			attribute vec2 vertexTexture;
		//			varying vec2 v_texCoord;
		//			void main() {
		//				gl_Position = vertexPosition;
		//				v_texCoord = vertexTexture;
		//			}
		//		",
		//		@"
		//			uniform sampler2D texture1;
		//			varying vec2 v_texCoord;
		//			void main() {
		//				gl_FragColor = texture2D(texture1, v_texCoord);
		//			}
		//		"
		//	);
		//
		//	var VertexBuffer = GLBuffer.Create().SetData(new float[] {
		//		-1, -1,   0, 0,
		//		+1, -1,   1, 0,
		//		-1, +1,   0, 1,
		//		+1, +1,   1, 1,
		//	});
		//
		//	GL.glClearColor(0, 0, 1, 1);
		//	GL.glClear(GL.GL_COLOR_BUFFER_BIT);
		//
		//	var Image1 = new Bitmap(Image.FromFile(@"C:\temp\1.jpg"));
		//	var Image1Data = Image1.GetChannelsDataInterleaved(BitmapChannelList.RGBA);
		//
		//	var TestTexture = GLTexture.Create().SetFormat(TextureFormat.RGBA).SetSize(Image1.Width, Image1.Height).SetData(Image1Data);
		//	//var TestTexture = GLTexture.Create().SetFormat(TextureFormat.RGBA).SetSize(2, 2).SetData(new uint[] { 0xFFFFFFFF, 0xFF00FFFF, 0xFFF00FFF, 0xFFFF00FF }).Upload();
		//	//var TestTexture = GLTexture.Create().SetFormat(TextureFormat.RGBA).SetSize(2, 1).SetData(new uint[] { 0xFF0000FF, 0xFFFF00FF }).Upload();
		//
		//	Shader.Draw(GLGeometry.GL_TRIANGLE_STRIP, 0, 4, () =>
		//	{
		//		Shader.GetUniform("texture1").Set(GLTextureUnit.CreateAtIndex(0).SetTexture(TestTexture));
		//		Shader.GetAttribute("vertexPosition").SetData<float>(VertexBuffer, 2, 0 * sizeof(float), 4 * sizeof(float));
		//		Shader.GetAttribute("vertexTexture").SetData<float>(VertexBuffer, 2, 2 * sizeof(float), 4 * sizeof(float));
		//	});
		//
		//	File.WriteAllBytes(@"c:\temp\out.bin", RenderTarget.ReadPixels());
		//
		//	new Bitmap(RenderTarget.Width, RenderTarget.Height)
		//		.SetChannelsDataInterleaved(RenderTarget.ReadPixels(), BitmapChannel.Red, BitmapChannel.Green, BitmapChannel.Blue, BitmapChannel.Alpha)
		//		.Save(@"c:\temp\out.png")
		//	;
		//
		//	Console.ReadKey();
		//	Environment.Exit(0);
		//}

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
			//_MainData();
			//_MainData2();
			
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
