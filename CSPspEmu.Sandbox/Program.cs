using System;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;
using System.IO;
using CSPspEmu.Hle;
using CSPspEmu.Core.Cpu.Assembler;
using System.Threading;
using System.Windows.Forms;
using CSPspEmu.Gui.Winforms;
using CSPspEmu.Hle.Modules.threadman;
using CSPspEmu.Hle.Modules.utils;
using CSPspEmu.Hle.Modules.display;
using CSPspEmu.Hle.Modules.loadexec;
using CSPspEmu.Hle.Modules.ctrl;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Loader;
using CSharpUtils;
using CSharpUtils.Threading;
using System.Reflection;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Hle.Modules.emulator;
using CSPspEmu.Hle.Vfs.Local;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Vfs.Emulator;
using System.Globalization;
using CSPspEmu.Core.Audio.Imple.Openal;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Controller;
using CSPspEmu.Core.Audio;
using System.Linq;
using CSPspEmu.Runner;

namespace CSPspEmu.Sandbox
{
	unsafe class Program
	{
		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://en.wikipedia.org/wiki/Common_Intermediate_Language"/>
		/// <see cref="http://en.wikipedia.org/wiki/List_of_CIL_instructions"/>
		/// <param name="args"></param>
		[STAThread]
		static void Main(string[] args)
		{
			Console.SetWindowSize(160, 60);
			Console.SetBufferSize(160, 2000);
			var PspEmulator = new PspEmulator();
			PspEmulator.Start();
		}
	}
}
