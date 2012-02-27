using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ComponentAce.Compression.Libs.zlib;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Vfs.Iso;
using CSPspEmu.Core.Audio.Impl.WaveOut;
using CSPspEmu.Core.Audio.Impl.Openal;
using Microsoft.Win32;
using CSPspEmu.Core;

namespace CSPspEmu.Sandbox
{
	unsafe class Program
	{
		private const Int32 SW_HIDE = 0;

		[DllImport("Kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		private static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);

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
			if (Arguments.Length > 0)
			{
				if (Arguments[0] == "/associate")
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
					IntPtr hwnd = GetConsoleWindow();
					ShowWindow(hwnd, SW_HIDE);

					PspEmulator.StartAndLoad(TryIsoFile, TraceSyscalls: false, ShowMenus: false);
					return;
				}
			}

			if (Arguments.Length > 0)
			{
				PspEmulator.StartAndLoad(Arguments[0], TraceSyscalls: false);
			}
			else
			{
#if RELEASE
				PspEmulator.Start();
#else
				//PspEmulator.UseFastMemory = true;
				PspEmulator.Start();
				//PspEmulator.Start();

				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\abuse\EBOOT.PBP", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Medievil.cso", TraceSyscalls: true);

				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Puzzler Collection.iso");

				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Exit.iso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Namco Museum Battle Collection.iso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Breath of Fire 3.cso", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Jak & Daxter - The Lost Frontier.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Sims2.cso");

				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Astonishia Story.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Tales of Phantasia.cso", EnableMpeg: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Skate Park City.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Outrun 2006.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\haruhi.iso", TraceSyscalls: false, EnableMpeg: true);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Castlevania.cso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Jeanne d'Arc.iso", TraceSyscalls: true, EnableMpeg: true);
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\TestInput\minifire.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\gu\text\gufont.elf");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\DragonBall Z Shin Budokai.cso", TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\PSPTris\EBOOT.PBP");
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\TrigWars\EBOOT.PBP", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Valkyrie Profile.cso", EnableMpeg: false, TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\valkyria profile\BOOT.BIN", EnableMpeg: false, TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Downstream Panic.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Clannad.iso", TraceSyscalls: false, EnableMpeg: true);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\haruhi.iso", TraceSyscalls: false, EnableMpeg: true);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Luxor.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Vallhala Knights.iso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Tales of Eternia - Español.iso", TraceSyscalls: false, EnableMpeg: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\kaiten\EBOOT.PBP", TraceSyscalls: false, EnableMpeg: false);

			// bltzal
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Kameleon.cso", TraceSyscalls: false);

			// API: Not Implemented
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Skate Park City.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Patapon.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\scummvm-1.4.0\EBOOT.PBP");

			// Memory: Invalid Address
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Puyo.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Prince.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Downstream Panic.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Call of Duty - Roads of Victory.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\haruhi.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\DragonBall Z Shin Budokai.cso", TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Loco Roco.cso", TraceSyscalls: true);
				
			// VFPU
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Castlevania.cso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Outrun 2006.cso");

			// Loader: Encrypted
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Armored Core - Silent Line Portable.iso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Brandish The Dark Revenant.cso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Final Fantasy 20th Anniversary.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Valkya_Chronics_2_USA.iso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Tekken Dark Resurrection.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Ultimate_Ghosts_N_Goblins.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Sonic Rivals.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Puzzle Quest.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Princess Frontier Portable.iso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Metal.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Daxter.cso");

			// Loader: External unencrypted modules
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Metal Gear Ac!d.cso");

			// Loader: Relocation
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Metal Gear Solid Portable Ops.iso", EnableMpeg: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Every Extend Extra.cso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Luxor.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Tales of the World.iso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Sega.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Puzzler Collection.iso");

			// Black. Loop
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Ape Academy.cso", TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Valkyrie Profile.cso", EnableMpeg: false, TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Need for Speed Most Wanted.cso", EnableMpeg: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Naruto Nultimate Portable.cso", EnableMpeg: false, TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Final Fantasy Tactics.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Harvest Moon.iso", TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Championship_Manager.cso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Koloomn.cso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Final Fantasy Crisis Core.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Legend of Heroes 3.cso", TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Street Fighter Alpha 3 MAX.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Jeanne d'Arc.iso", TraceSyscalls: true, EnableMpeg: true);

			// Starting
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Vallhala Knights.iso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\GripShift.cso", EnableMpeg: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\WipeOut Pulse EUR.cso");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\MACH.cso", EnableMpeg: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Lemmings.cso", EnableMpeg: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Tales of Phantasia.cso", EnableMpeg: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Tales of Eternia - Español.iso", TraceSyscalls: false, EnableMpeg: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Tales of Eternia.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Darkstalkers Chronicle the Chaos Tower.cso", EnableMpeg: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Astonishia Story.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Breath of Fire 3.cso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Popolocrois.cso", EnableMpeg: true);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\PSPTris\EBOOT.PBP");
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\TrigWars\EBOOT.PBP", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\Puzzle Bobble.ISO", TraceSyscalls: false);

			// Unknown
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\Aquaria\EBOOT.PBP", TraceSyscalls: true);

				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\NesterP\EBOOT.PBP", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\scummvm-0.13.0-psp\EBOOT.PBP");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\PicoDrive\EBOOT.PBP");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\uMSX\EBOOT.PBP");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\Daedalus\EBOOT.PBP");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\Battlegrounds3\EBOOT.PBP");
				
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\EmuMaster V3.1 (Unofficial)\EBOOT.PBP", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\goldminer\EBOOT.PBP");
				//PspEmulator.StartAndLoad(@"C:\juegos\jpcsp_last\demos\nehetutorial07.pbp");
				//PspEmulator.StartAndLoad(@"C:\juegos\jpcsp_last\demos\nehetutorial08.pbp");
				//PspEmulator.StartAndLoad(@"C:\juegos\jpcsp_last\demos\nehetutorial09.pbp");
				//PspEmulator.StartAndLoad(@"C:\juegos\jpcsp_last\demos\nehetutorial10.pbp", TrackCallStack : false);

				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\cavestory\EBOOT.PBP", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\pspautotests\tests\video\pmf\pmf.elf", TraceSyscalls: false);
				//PspEmulator.AddCwCheat(0x70D69F04, 0x00000007);
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\Yume for PSP\EBOOT.PBP");
				//PspEmulator.StartAndLoad(@"E:\Isos\psp\homebrew\yume\EBOOT.PBP");


				//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\audio\polyphonic\polyphonic.elf", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"C:\juegos\jpcsp_last\demos\compilerPerf.pbp");
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\controller.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\clut.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\lights.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\morph.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\reflection.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\rtctest.pbp", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\cube.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\cavestory\EBOOT.PBP", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\Aquaria\EBOOT.PBP", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\Astonishia Story.iso");
				//PspEmulator.StartAndLoad(@"C:\juegos\jpcsp_last\demos\mstick.pbp");
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\Puzzle Bobble.ISO");
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\Tales of Eternia.iso", TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\puzzle_bubble\PSP_GAME\SYSDIR\BOOT.BIN");
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\astonishia\PSP_GAME\SYSDIR\BOOT.BIN", TraceSyscalls: true);

				//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\gu\ortho\ortho.elf");


				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\nesterJ\1.5\NesterJ\EBOOT.PBP");
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\Divi-Dead\dividead_psp.elf");
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\Divi-Dead\EBOOT.PBP");

				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\REminiscence\EBOOT.PBP", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\threads\semaphores\semaphores.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\threads\threads\threads.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\threads\threads\wakeup.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\dmac\dmactest.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\hle\check_not_used_uids.elf", TraceSyscalls: false);



				//PspEmulator.Start();

				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\clut.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\lights.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\morph.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\reflection.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\morph.pbp", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\kaiten\UraKaitenPatissierPSP\kaiten.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\Divi-Dead\dividead.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\intr\intr.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\io\cwd\cwd.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\io\io\io.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\malloc\malloc.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\misc\testgp.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\power\power.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\rtc\rtc.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\string\string.elf");
				//PspEmulator.StartAndLoad(@"C:\projects\pspemu\pspautotests\tests\sysmem\sysmem.elf");

				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\compilerPerf.pbp");
				//PspEmulator.StartAndLoad(@"C:\juegos\jpcsp_last\demos\compilerPerf.pbp");

				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\TestInput\minifire.elf", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\gu\text\gufont.elf");

				//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\audio\wavegen\alsample.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\gu\cube\cube.elf");
				//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\audio\polyphonic\polyphonic.elf", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\gu\lights\lights.elf");
				//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\gu\morph\morph.elf");
				//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\gu\skinning\skinning.elf");
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\cavestory\EBOOT.PBP", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\reminiscence\EBOOT.PBP", TraceSyscalls: true);

				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\PspAutoTests\string.elf");
#endif
			}
		}
	}
}
