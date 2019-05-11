using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Core;
using CSPspEmu.AutoTests;
using CSharpUtils;
using CSharpUtils.Getopt;
using CSPspEmu.Hle.Vfs.Iso;
using System.Diagnostics;
using System.Drawing;
using CSharpPlatform.GL;
using CSharpPlatform.GL.Utils;
using CSharpUtils.Drawing;
using CSharpUtils.Drawing.Extensions;
using CSharpUtils.Extensions;
using CSPspEmu.Gui.XBR.Shader;
using CSPspEmu.Hle.Modules.threadman;

namespace CSPspEmu
{
    unsafe class Program
    {
        public static void Main(string[] arguments)
        {
            Console.WriteLine("Program.Main");
            try
            {
                DoMain(arguments);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        public static readonly Logger Logger = Logger.GetLogger("Program");

        private static void Form1_UIThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Console.Error.WriteLine(e.Exception);
            Environment.Exit(-1);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Error.WriteLine(e.ExceptionObject);
            Environment.Exit(-1);
        }

        public static bool IsNet45OrNewer()
        {
            // Class "ReflectionContext" exists from .NET 4.5 onwards.
            return Type.GetType("System.Reflection.ReflectionContext", false) != null;
        }

        static void RunTests(bool runTestsViewOut, string[] arguments, int timeout)
        {
            AutoTestsProgram.Main(runTestsViewOut, arguments.ToArray(), timeout);
            Environment.Exit(0);
        }

        private static GLTexture GlTextureCreateFromBitmap(Bitmap bitmap)
        {
            return GLTexture.Create()
                    .SetFormat(TextureFormat.RGBA)
                    .SetSize(bitmap.Width, bitmap.Height)
                    .SetData(bitmap.GetChannelsDataInterleaved(BitmapChannelList.Rgba))
                ;
        }

        private static void _MainData()
        {
            var context = GlContextFactory.CreateWindowless().MakeCurrent();
            //var BitmapIn = new Bitmap(Image.FromFile(@"C:\temp\1.png"));
            var bitmapIn = new Bitmap(@"C:\temp\in.png");

            Console.WriteLine("{0}", string.Join("\n", Assembly.GetExecutingAssembly().GetManifestResourceNames()));

            var textureOut = new XBRShader().Process(GlTextureCreateFromBitmap(bitmapIn));

            new Bitmap(textureOut.Width, textureOut.Height)
                .SetChannelsDataInterleaved(textureOut.GetDataFromGpu(), BitmapChannelList.Rgba)
                .Save(@"c:\temp\out.png");

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
        /// <param name="arguments"></param>
        [STAThread]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void DoMain(string[] arguments)
        {
            //Console.WriteLine(GL.GetConstantString(GL.GL_TEXTURE_2D));
            //_MainData();
            //_MainData2();

            if (!IsNet45OrNewer())
            {
                //ThreadManForUser.MessageBox.Show(".NET 4.5 required", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            // Add the event handler for handling UI thread exceptions to the event.
            //Application.ThreadException += new ThreadExceptionEventHandler(Form1_UIThreadException);

            //System.AppDomain.CurrentDomain.UnhandledException += 

            // Set the unhandled exception mode to force all Windows Forms errors to go through
            // our handler.
            //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Logger.OnGlobalLog += (logName, level, text, stackFrame) =>
            {
                if (level >= Logger.Level.Info)
                {
                    var method = stackFrame.GetMethod();
                    Console.WriteLine("{0} : {1} : {2}.{3} : {4}", logName, level,
                        (method.DeclaringType != null) ? method.DeclaringType.Name : null, method.Name, text);
                }
            };

#if false
			Console.WriteLine(CSPspEmu.Resources.Translations.GetString("extra", "UnknownGame"));
			Console.ReadKey(); Environment.Exit(0);
#endif

#if RUN_TESTS
			RunTests(Arguments);
#endif

            string fileToLoad = null;
            bool runTestsViewOut = false;
            int runTestsTimeout = 60;

            var getopt = new Getopt(arguments);
            {
                getopt.AddRule(new[] {"/help", "/?", "-h", "--help", "-?"}, () =>
                {
                    Console.WriteLine("Soywiz's Psp Emulator - {0} - r{1} - {2}", PspGlobalConfiguration.CurrentVersion,
                        PspGlobalConfiguration.CurrentVersionNumeric, PspGlobalConfiguration.GitRevision);
                    Console.WriteLine("");
                    Console.WriteLine(" Switches:");
                    Console.WriteLine("   /version                         - Outputs the program version");
                    Console.WriteLine("   /version2                        - Outputs the program numeric version");
                    Console.WriteLine("   /decrypt <EBOOT.BIN>             - Decrypts an EBOOT.BIN");
                    Console.WriteLine("   /gitrevision                     - Outputs the git revision");
                    Console.WriteLine(
                        "   /associate                       - Associates extensions with the program. Requires be launched with administrative rights.");
                    Console.WriteLine("   /viewout /timeout X /tests       - Run integration tests.");
                    Console.WriteLine("   ");
                    Console.WriteLine("   /isolist <pathto.iso|cso|dax>    - Lists the content of an iso.");
                    Console.WriteLine("   /isoextract <in.iso> <outfolder> - Extracts the content of an iso.");
                    Console.WriteLine(
                        "   /isoconvert <in.xxx> <out.yyy>   - Converts a iso/cso/dax file into other format.");
                    Console.WriteLine("");
                    Console.WriteLine(" Examples:");
                    Console.WriteLine("   cspspemu.exe <path_to_psp_executable>");
                    Console.WriteLine("");
                    Environment.Exit(0);
                });
                getopt.AddRule("/version", () =>
                {
                    Console.Write("{0}", PspGlobalConfiguration.CurrentVersion);
                    Environment.Exit(0);
                });
                getopt.AddRule("/version2", () =>
                {
                    Console.Write("{0}", PspGlobalConfiguration.CurrentVersionNumeric);
                    Environment.Exit(0);
                });
                getopt.AddRule("/isoconvert", () =>
                {
                    var isoInPath = getopt.DequeueNext();
                    var isoOutPath = getopt.DequeueNext();

                    if (Path.GetExtension(isoOutPath) != ".iso")
                    {
                        Console.WriteLine("Just support outputing .iso files");
                        Environment.Exit(-1);
                    }

                    var isoInFile = IsoLoader.GetIso(isoInPath);
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    Console.Write("{0} -> {1}...", isoInPath, isoOutPath);
                    isoInFile.Stream.Slice().CopyToFile(isoOutPath);
                    Console.WriteLine("Ok ({0})", stopwatch.Elapsed);
                    Environment.Exit(0);
                });
                getopt.AddRule("/isolist", () =>
                {
                    var isoPath = getopt.DequeueNext();
                    var isoFile = IsoLoader.GetIso(isoPath);
                    var isoFileSystem = new HleIoDriverIso(isoFile);
                    foreach (var fileName in isoFileSystem.ListDirRecursive("/"))
                    {
                        var stat = isoFileSystem.GetStat(fileName);
                        Console.WriteLine("{0} : {1}", fileName, stat.Size);
                    }

                    //Console.Write("{0}", PspGlobalConfiguration.CurrentVersionNumeric);
                    Environment.Exit(0);
                });
                getopt.AddRule("/isoextract", () =>
                {
                    var isoPath = getopt.DequeueNext();
                    var outputPath = getopt.DequeueNext();
                    var isoFile = IsoLoader.GetIso(isoPath);
                    var isoFileSystem = new HleIoDriverIso(isoFile);
                    foreach (var fileName in isoFileSystem.ListDirRecursive("/"))
                    {
                        var stat = isoFileSystem.GetStat(fileName);
                        var outputFileName = outputPath + "/" + fileName;
                        Console.Write("{0} : {1}...", fileName, stat.Size);

                        if (!stat.Attributes.HasFlag(Hle.Vfs.IOFileModes.Directory))
                        {
                            var parentDirectory = Directory.GetParent(outputFileName).FullName;
                            //Console.WriteLine(ParentDirectory);
                            try
                            {
                                Directory.CreateDirectory(parentDirectory);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                            using (var inputStream = isoFileSystem.OpenRead(fileName))
                            {
                                inputStream.CopyToFile(outputFileName);
                            }
                        }

                        Console.WriteLine("Ok");
                    }

                    //Console.Write("{0}", PspGlobalConfiguration.CurrentVersionNumeric);
                    Environment.Exit(0);
                });
                getopt.AddRule("/decrypt", (string encryptedFile) =>
                {
                    try
                    {
                        using (var encryptedStream = File.OpenRead(encryptedFile))
                        {
                            var decryptedFile = $"{encryptedFile}.decrypted";
                            Console.Write("'{0}' -> '{1}'...", encryptedFile, decryptedFile);

                            var encryptedData = encryptedStream.ReadAll();
                            var decryptedData = new EncryptedPrx().Decrypt(encryptedData);
                            File.WriteAllBytes(decryptedFile, decryptedData);
                            Console.WriteLine("Ok");
                            Environment.Exit(0);
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.Error.WriteLine(exception);
                        Environment.Exit(-1);
                    }
                });
                getopt.AddRule("/gitrevision", () =>
                {
                    Console.Write("{0}", PspGlobalConfiguration.GitRevision);
                    Environment.Exit(0);
                });
                //getopt.AddRule("/associate", () =>
                //{
                //    try
                //    {
                //        var classesRoot = Registry.ClassesRoot;
//
                //        classesRoot.CreateSubKey(".pbp")?.SetValue(null, "cspspemu.executable");
                //        classesRoot.CreateSubKey(".elf")?.SetValue(null, "cspspemu.executable");
                //        classesRoot.CreateSubKey(".prx")?.SetValue(null, "cspspemu.executable");
                //        classesRoot.CreateSubKey(".cso")?.SetValue(null, "cspspemu.executable");
                //        classesRoot.CreateSubKey(".dax")?.SetValue(null, "cspspemu.executable");
//
                //        var reg = classesRoot.CreateSubKey("cspspemu.executable");
                //        reg?.SetValue(null, "PSP executable file (.elf, .pbp, .cso, .prx, .dax)");
                //        reg?.SetValue("DefaultIcon", @"""" + ApplicationPaths.ExecutablePath + @""",0");
                //        reg?.CreateSubKey("shell")?.CreateSubKey("open")?.CreateSubKey("command")?.SetValue(null,
                //            @"""" + ApplicationPaths.ExecutablePath + @""" ""%1""");
//
                //        Environment.Exit(0);
                //    }
                //    catch (Exception e)
                //    {
                //        Console.Error.WriteLine(e);
                //        Environment.Exit(-1);
                //    }
                //});
                getopt.AddRule("/viewout", () => { runTestsViewOut = true; });
                getopt.AddRule("/timeout", (int seconds) => { runTestsTimeout = seconds; });
                getopt.AddRule("/tests",
                    () => { RunTests(runTestsViewOut, getopt.DequeueAllNext(), runTestsTimeout); });
                getopt.AddRule(name => { fileToLoad = name; });
            }
            try
            {
                getopt.Process();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                Environment.Exit(-1);
            }

            Logger.Info("Running ... plat:{0} ... int*:{1}", Environment.Is64BitProcess ? "64bit" : "32bit",
                sizeof(int*));
            {
                var monoRuntimeType = Type.GetType("Mono.Runtime");
                if (monoRuntimeType != null)
                {
                    var getDisplayNameMethod =
                        monoRuntimeType.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                    if (getDisplayNameMethod != null)
                        Console.WriteLine("Mono: {0}", getDisplayNameMethod.Invoke(null, null));
                }
            }
            Console.WriteLine("ImageRuntimeVersion: {0}", Assembly.GetExecutingAssembly().ImageRuntimeVersion);

//#if !RELEASE
//            try
//            {
//                Console.OutputEncoding = Encoding.UTF8;
//                Console.SetWindowSize(160, 60);
//                Console.SetBufferSize(160, 2000);
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//            }
//#endif

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

            using (var pspEmulator = new PspEmulator())
            {
                //PspEmulator.UseFastMemory = true;
                var codeBase = Assembly.GetExecutingAssembly().Location;
                var Base = Path.GetDirectoryName(codeBase) + @"\" + Path.GetFileNameWithoutExtension(codeBase);
                foreach (var tryExtension in new[] {"iso", "cso", "elf", "pbp"})
                {
                    var tryIsoFile = Base + "." + tryExtension;

                    //Console.WriteLine(TryIsoFile);
                    //Console.ReadKey();

                    if (File.Exists(tryIsoFile))
                    {
                        Platform.HideConsole();

                        pspEmulator.StartAndLoad(tryIsoFile, TraceSyscalls: false, ShowMenus: false);
                        return;
                    }
                }

                if (fileToLoad != null)
                {
                    pspEmulator.StartAndLoad(fileToLoad, TraceSyscalls: false);
                }
                else
                {
                    //StartWithoutArguments(PspEmulator);
                    pspEmulator.Start();
                }
            }
        }
    }
}