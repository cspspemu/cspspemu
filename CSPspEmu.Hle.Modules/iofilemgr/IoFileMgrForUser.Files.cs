using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Vfs;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	unsafe public partial class IoFileMgrForUser
	{
		/// <summary>
		/// Open or create a file for reading or writing
		/// </summary>
		/// <example>
		///		// Example1: Open a file for reading
		///		if (!(fd = sceIoOpen("device:/path/to/file", O_RDONLY, 0777)) {
		///			// error
		///		}
		///		
		///		// Example2: Open a file for writing, creating it if it doesnt exist
		///		if (!(fd = sceIoOpen("device:/path/to/file", O_WRONLY|O_CREAT, 0777)) {
		///			// error
		///		}
		/// </example>
		/// <param name="FileName">Pointer to a string holding the name of the file to open</param>
		/// <param name="Flags">Libc styled flags that are or'ed together</param>
		/// <param name="Mode">File access mode.</param>
		/// <returns>A non-negative integer is a valid fd, anything else an error</returns>
		[HlePspFunction(NID = 0x109F50BC, FirmwareVersion = 150)]
		public int sceIoOpen(string FileName, HleIoFlags Flags, SceMode Mode)
		{
			var Info = HleState.HleIoManager.ParsePath(FileName);
			Info.HleIoDrvFileArg.HleIoDriver.IoOpen(Info.HleIoDrvFileArg, Info.LocalPath, Flags, Mode);
			return HleState.HleIoManager.HleIoDrvFileArgPool.Create(Info.HleIoDrvFileArg);
		}

		/// <summary>
		/// Write output
		/// </summary>
		/// <example>
		///		bytes_written = sceIoWrite(fd, data, 100);
		/// </example>
		/// <param name="FileHandle">Opened file descriptor to write to</param>
		/// <param name="DataPtr">Pointer to the data to write</param>
		/// <param name="DataSize">Size of data to write</param>
		/// <returns>The number of bytes written</returns>
		[HlePspFunction(NID = 0x42EC03AC, FirmwareVersion = 150)]
		public int sceIoWrite(int FileHandle, byte* DataPtr, int DataSize)
		{
			var HleIoDrvFileArg = GetFileArgFromHandle(FileHandle);
			return HleIoDrvFileArg.HleIoDriver.IoWrite(HleIoDrvFileArg, DataPtr, DataSize);
		}
	}
}
