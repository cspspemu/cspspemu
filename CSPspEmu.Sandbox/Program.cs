using System;
using System.IO;
using System.IO.Compression;
using ComponentAce.Compression.Libs.zlib;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Vfs.Iso;

namespace CSPspEmu.Sandbox
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
			Console.SetWindowSize(160, 60);
			Console.SetBufferSize(160, 2000);
#endif

			var PspEmulator = new PspEmulator();
			if (Arguments.Length > 0)
			{
				PspEmulator.StartAndLoad(Arguments[0], TraceSyscalls: false);
			}
			else
			{

#if RELEASE
				PspEmulator.Start();
#else
				//PspEmulator.Start();

				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\TrigWars\EBOOT.PBP", TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\pspautotests\demos\cubevfpu.prx", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\WipeOut Pulse EUR.cso");
				//PspEmulator.StartAndLoad(@"C:\projects\cspspemu\pspautotests\demos\cubevfpu.prx", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Puzzle Bobble.ISO", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Castlevania.cso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Every Extend Extra.cso", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"F:\Isos\psp\homebrew\Aquaria\EBOOT.PBP", TraceSyscalls: true);

				//PspEmulator.StartAndLoad(@"F:\Isos\psp\haruhi.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Final Fantasy Tactics.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Tales of Eternia - Español.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\psp\isos\astonishia.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\psp\isos\Tales of Eternia.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\psp\isos\Koloomn.cso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\psp\isos\Final Fantasy Tactics.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Championship_Manager.cso", TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\DragonBall Z Shin Budokai.cso", TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Harvest Moon.iso", TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Lemmings.cso", TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Luxor.cso");
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Popolocrois.cso");
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Loco Roco.cso");
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\homebrew\Yume for PSP\EBOOT.PBP");
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\homebrew\yume\EBOOT.PBP");


				PspEmulator.StartAndLoad(@"F:\Isos\psp\Breath of Fire 3.cso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Tales of Eternia.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Koloomn.cso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Kameleon.cso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"F:\Isos\psp\Astonishia Story.iso", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\audio\polyphonic\polyphonic.elf", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"C:\juegos\jpcsp_last\demos\compilerPerf.pbp");
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\controller.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\clut.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\lights.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\morph.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\reflection.pbp", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"C:\juegos\pspemu\demos\cube.pbp", TraceSyscalls: false);
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\cavestory\EBOOT.PBP", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\Aquaria\EBOOT.PBP", TraceSyscalls: false);

				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\Astonishia Story.iso");
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\Puzzle Bobble.ISO");
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\Tales of Eternia.iso", TraceSyscalls: true);
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\puzzle_bubble\PSP_GAME\SYSDIR\BOOT.BIN");
				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\astonishia\PSP_GAME\SYSDIR\BOOT.BIN", TraceSyscalls: true);

				//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\abuse\EBOOT.PBP");
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
				//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\audio\polyphonic\polyphonic.elf", TraceSyscalls: true);
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
