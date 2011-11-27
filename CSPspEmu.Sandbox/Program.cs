using System;

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
		static void Main(string[] args)
		{
			var PspEmulator = new PspEmulator();
#if RELEASE
			PspEmulator.Start();
#else
			Console.SetWindowSize(160, 60);
			Console.SetBufferSize(160, 2000);

			//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\audio\wavegen\alsample.elf", TraceSyscalls: false);
			//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\gu\ortho\ortho.elf");
			//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\audio\polyphonic\polyphonic.elf", TraceSyscalls: true);
			PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\audio\polyphonic\polyphonic.elf", TraceSyscalls: false);
			//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\gu\lights\lights.elf");
			//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\gu\morph\morph.elf");
			//PspEmulator.StartAndLoad(@"C:\pspsdk\psp\sdk\samples\gu\skinning\skinning.elf");
			//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\cavestory\EBOOT.PBP", TraceSyscalls: true);
			//PspEmulator.StartAndLoad(@"C:\projects\csharp\cspspemu\games\TrigWars\EBOOT.PBP", TraceSyscalls: true);
#endif
		}
	}
}
