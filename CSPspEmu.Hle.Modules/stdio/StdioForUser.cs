using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.stdio
{
	public class StdioForUser : HleModuleHost
	{
		public enum StdHandle : int
		{
			In  = -1,
			Out = -2,
			Error = -3
		}

		public enum SceMode : uint
		{
		}

		/// <summary>
		/// Function to get the current standard in file no
		/// </summary>
		/// <returns>The stdin fileno</returns>
		[HlePspFunction(NID = 0x172D316E, FirmwareVersion = 150)]
		public StdHandle sceKernelStdin()
		{
			return StdHandle.In;
		}

		/// <summary>
		/// Function to get the current standard out file no
		/// </summary>
		/// <returns>The stdout fileno</returns>
		[HlePspFunction(NID = 0xA6BAB2E9, FirmwareVersion = 150)]
		public StdHandle sceKernelStdout()
		{
			return StdHandle.Out;
		}

		/// <summary>
		/// Function to get the current standard err file no
		/// </summary>
		/// <returns>The stderr fileno</returns>
		[HlePspFunction(NID = 0xF78BA90A, FirmwareVersion = 150)]
		public StdHandle sceKernelStderr()
		{
			return StdHandle.Error;
		}

		/// <summary>
		/// Function reopen the stdout file handle to a new file
		/// </summary>
		/// <param name="file">The file to open.</param>
		/// <param name="flags">The open flags </param>
		/// <param name="mode">The file mode</param>
		/// <returns>&lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x98220F3E, FirmwareVersion = 150)]
		public int sceKernelStdoutReopen(string file, int flags, SceMode mode)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Function reopen the stderr file handle to a new file
		/// </summary>
		/// <param name="file">The file to open.</param>
		/// <param name="flags">The open flags </param>
		/// <param name="mode">The file mode</param>
		/// <returns>&lt; 0 on error.</returns>
		[HlePspFunction(NID = 0xFB5380C5, FirmwareVersion = 150)]
		public int sceKernelStderrReopen(string file, int flags, SceMode mode)
		{
			throw (new NotImplementedException());
		}
	}
}
