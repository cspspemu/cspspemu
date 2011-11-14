using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSPspEmu.Hle.Vfs;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	unsafe public class IoFileMgrForUser : HleModuleHost
	{
		//public enum SceMode { }

		/// <summary>
		/// Open a directory
		/// </summary>
		/// <example>
		///		int dfd;
		///		dfd = sceIoDopen("device:/");
		///		if (dfd >= 0) { Do something with the file descriptor }
		/// </example>
		/// <param name="DirectoryPath">The directory to open for reading.</param>
		/// <returns>If greater or equal to 0 then a valid file descriptor, otherwise a Sony error code.</returns>
		[HlePspFunction(NID = 0xB29DDF9C, FirmwareVersion = 150)]
		public int sceIoDopen(string DirectoryPath)
		{
			var Info = HleState.HleIoManager.ParsePath(DirectoryPath);
			Info.HleIoDrvFileArg.HleIoDriver.IoDopen(Info.HleIoDrvFileArg, Info.LocalPath);
			return HleState.HleIoManager.HleIoDrvFileArgPool.Create(Info.HleIoDrvFileArg);
		}

		/// <summary>
		/// Reads an entry from an opened file descriptor.
		/// </summary>
		/// <param name="FileHandle">Already opened file descriptor (using sceIoDopen)</param>
		/// <param name="dir">Pointer to an io_dirent_t structure to hold the file information</param>
		/// <returns>
		///		Read status
		///		Equal to   0 - No more directory entries left
		///		Great than 0 - More directory entired to go
		///		Less  than 0 - Error
		/// </returns>
		[HlePspFunction(NID = 0xE3EB004C, FirmwareVersion = 150)]
		public int sceIoDread(int FileHandle, HleIoDirent* dir)
		{
			var HleIoDrvFileArg = GetFileArgFromHandle(FileHandle);
			return HleIoDrvFileArg.HleIoDriver.IoDread(HleIoDrvFileArg, dir);
		}

		/// <summary>
		/// Close an opened directory file descriptor
		/// </summary>
		/// <param name="FileHandle">Already opened file descriptor (using sceIoDopen)</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xEB092469, FirmwareVersion = 150)]
		public int sceIoDclose(int FileHandle)
		{
			var HleIoDrvFileArg = GetFileArgFromHandle(FileHandle);
			return HleIoDrvFileArg.HleIoDriver.IoDclose(HleIoDrvFileArg);
		}

		/// <summary>
		/// Change the current directory.
		/// </summary>
		/// <param name="DirectoryPath">The path to change to.</param>
		/// <returns>less than 0 on error.</returns>
		[HlePspFunction(NID = 0x55F4717D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceIoChdir(string DirectoryPath)
		{
			//var Info = HleState.HleIoManager.ParsePath(DirectoryPath);
			//return Info.HleIoDriver.IoChdir(Info.HleIoDrvFileArg, Info.LocalPath);
			return 0;
		}

		/// <summary>
		/// Send a devctl command to a device.
		/// </summary>
		/// <example>
		///		Example: Sending a simple command to a device (not a real devctl)
		///		sceIoDevctl("ms0:", 0x200000, indata, 4, NULL, NULL); 
		/// </example>
		/// <param name="DeviceName">String for the device to send the devctl to (e.g. "ms0:")</param>
		/// <param name="Command">The command to send to the device</param>
		/// <param name="InputPointer">A data block to send to the device, if NULL sends no data</param>
		/// <param name="InputLength">Length of indata, if 0 sends no data</param>
		/// <param name="OutputPointer">A data block to receive the result of a command, if NULL receives no data</param>
		/// <param name="OutputLength">Length of outdata, if 0 receives no data</param>
		/// <returns>0 on success, &lt; 0 on error</returns>
		[HlePspFunction(NID = 0x54F5FB11, FirmwareVersion = 150)]
		public int sceIoDevctl(string DeviceName, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
		{
			var Info = HleState.HleIoManager.ParseDeviceName(DeviceName);
			return Info.HleIoDrvFileArg.HleIoDriver.IoDevctl(Info.HleIoDrvFileArg, DeviceName, Command, InputPointer, InputLength, OutputPointer, OutputLength);
		}

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

		private HleIoDrvFileArg GetFileArgFromHandle(int FileHandle)
		{
			return HleState.HleIoManager.HleIoDrvFileArgPool.Get(FileHandle);
		}
	}
}
