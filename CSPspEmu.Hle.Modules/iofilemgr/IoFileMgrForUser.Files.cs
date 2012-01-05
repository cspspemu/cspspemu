using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Vfs;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	unsafe public partial class IoFileMgrForUser
	{
		public enum SceUID : int { }
		public enum SceSize : int { }
		//public enum SceIoFlags : int { }
		public struct PspIoDrv { }

		/// <summary>
		/// Get the status of a file.
		/// </summary>
		/// <param name="FileName">The path to the file.</param>
		/// <param name="SceIoStat">A pointer to an io_stat_t structure.</param>
		/// <returns>Less than zero on error</returns>
		[HlePspFunction(NID = 0xACE946E8, FirmwareVersion = 150)]
		public int sceIoGetstat(string FileName, SceIoStat* SceIoStat)
		{
			var Info = HleState.HleIoManager.ParsePath(FileName);
			try
			{
				Info.HleIoDriver.IoGetstat(Info.HleIoDrvFileArg, Info.LocalPath, SceIoStat);
				return 0;
			}
			catch (FileNotFoundException FileNotFoundException)
			{
				Console.Error.WriteLine("Can't find file '{0}'", FileName);
				Console.Error.WriteLine(FileNotFoundException);
				return (int)SceKernelErrors.ERROR_ERRNO_FILE_NOT_FOUND;
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				return -1;
			}
		}

		/// <summary>
		/// Change the status of a file.
		/// </summary>
		/// <param name="FileName">The path to the file.</param>
		/// <param name="SceIoStat">A pointer to an io_stat_t structure.</param>
		/// <param name="Bitmask">Bitmask defining which bits to change.</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0xB8A740F4, FirmwareVersion = 150)]
		public int sceIoChstat(string FileName, SceIoStat *SceIoStat, int Bitmask)
		{
			var Info = HleState.HleIoManager.ParsePath(FileName);
			return Info.HleIoDriver.IoChstat(Info.HleIoDrvFileArg, FileName, SceIoStat, Bitmask);
		}

		/// <summary>
		/// Remove directory entry
		/// </summary>
		/// <param name="FileName">Path to the file to remove</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xF27A9C51, FirmwareVersion = 150)]
		public int sceIoRemove(string FileName)
		{
			var Info = HleState.HleIoManager.ParsePath(FileName);
			return Info.HleIoDriver.IoRemove(Info.HleIoDrvFileArg, FileName);
		}

		/// <summary>
		/// Change the name of a file
		/// </summary>
		/// <param name="OldFileName">The old filename</param>
		/// <param name="NewFileName">The new filename</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0x779103A0, FirmwareVersion = 150)]
		public int sceIoRename(string OldFileName, string NewFileName)
		{
			var Info1 = HleState.HleIoManager.ParsePath(OldFileName);
			var Info2 = HleState.HleIoManager.ParsePath(NewFileName);
			if (!Info1.Equals(Info2)) throw(new NotImplementedException("Rename from different filesystems"));
			return Info1.HleIoDriver.IoRename(Info1.HleIoDrvFileArg, OldFileName, NewFileName);
		}

		/// <summary>
		/// Reopens an existing file descriptor.
		/// </summary>
		/// <param name="NewFileName">The new file to open.</param>
		/// <param name="SceIoFlags">The open flags.</param>
		/// <param name="SceMode">The open mode.</param>
		/// <param name="FileDescriptor">The old filedescriptor to reopen</param>
		/// <returns>Less than 0 on error, otherwise the reopened fd.</returns>
		[HlePspFunction(NID = 0x3C54E908, FirmwareVersion = 150)]
		public int sceIoReopen(string NewFileName, HleIoFlags SceIoFlags, SceMode SceMode, SceUID FileDescriptor)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Reposition read/write file descriptor offset
		/// </summary>
		/// <example>
		///		pos = sceIoLseek(fd, -10, SEEK_END);
		/// </example>
		/// <param name="FileDescriptor">Opened file descriptor with which to seek</param>
		/// <param name="Offset">Relative offset from the start position given by whence</param>
		/// <param name="Whence">
		/// Set to SEEK_SET to seek from the start of the file, SEEK_CUR
		/// seek from the current position and SEEK_END to seek from the end.
		/// </param>
		/// <returns>The position in the file after the seek. </returns>
		[HlePspFunction(NID = 0x27EB27B8, FirmwareVersion = 150)]
		public long sceIoLseek(int FileDescriptor, long Offset, SeekAnchor Whence)
		{
			var HleIoDrvFileArg = GetFileArgFromHandle(FileDescriptor);
			return HleIoDrvFileArg.HleIoDriver.IoLseek(HleIoDrvFileArg, Offset, Whence);
		}

		/// <summary>
		/// Reposition read/write file descriptor offset (32bit mode)
		/// </summary>
		/// <example>
		///		pos = sceIoLseek32(fd, -10, SEEK_END);
		/// </example>
		/// <param name="FileDescriptor">Opened file descriptor with which to seek</param>
		/// <param name="Offset">Relative offset from the start position given by whence</param>
		/// <param name="Whence">
		///		Set to SEEK_SET to seek from the start of the file, SEEK_CUR
		///		seek from the current position and SEEK_END to seek from the end.
		/// </param>
		/// <returns>The position in the file after the seek.</returns>
		[HlePspFunction(NID = 0x68963324, FirmwareVersion = 150)]
		public int sceIoLseek32(int FileDescriptor, int Offset, SeekAnchor Whence)
		{
			var HleIoDrvFileArg = GetFileArgFromHandle(FileDescriptor);
			return (int)HleIoDrvFileArg.HleIoDriver.IoLseek(HleIoDrvFileArg, (long)Offset, Whence);
		}

		/// <summary>
		/// Read input
		/// </summary>
		/// <example>
		///     bytes_read = sceIoRead(fd, data, 100);
		/// </example>
		/// <param name="fd">Opened file descriptor to read from</param>
		/// <param name="OutputPointer">Pointer to the buffer where the read data will be placed</param>
		/// <param name="OutputSize">Size of the read in bytes</param>
		/// <returns>The number of bytes read</returns>
		[HlePspFunction(NID = 0x6A638D83, FirmwareVersion = 150)]
		public int sceIoRead(int FileDescriptor, byte* OutputPointer, int OutputSize)
		{
			var HleIoDrvFileArg = GetFileArgFromHandle(FileDescriptor);
			var Result = HleIoDrvFileArg.HleIoDriver.IoRead(HleIoDrvFileArg, OutputPointer, OutputSize);
			//for (int n = 0; n < OutputSize; n++) Console.Write("{0:X},", OutputPointer[n]);
			return Result;
		}

		/// <summary>
		/// Delete a descriptor
		/// </summary>
		/// <example>
		///		sceIoClose(fd);
		/// </example>
		/// <param name="FileDescriptor">File descriptor to close</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0x810C4BC3, FirmwareVersion = 150)]
		public int sceIoClose(int FileDescriptor)
		{
			try
			{
				var HleIoDrvFileArg = GetFileArgFromHandle(FileDescriptor);
				return HleIoDrvFileArg.HleIoDriver.IoClose(HleIoDrvFileArg);
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				return -1;
			}
		}

		/// <summary>
		/// Open or create a file for reading or writing
		/// </summary>
		/// <example>
		///		// Example1: Open a file for reading
		///		if (!(fd = sceIoOpen("device:/path/to/file", PSP_O_RDONLY, 0777)) {
		///			// error
		///		}
		///		
		///		// Example2: Open a file for writing, creating it if it doesnt exist
		///		if (!(fd = sceIoOpen("device:/path/to/file", PSP_O_WRONLY | PSP_O_CREAT, 0777)) {
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
			try
			{
				Console.WriteLine("Opened '{0}' with driver '{1}' and local path '{2}'", FileName, Info.HleIoDriver, Info.LocalPath);
				Info.HleIoDrvFileArg.HleIoDriver.IoOpen(Info.HleIoDrvFileArg, Info.LocalPath, Flags, Mode);
				return HleState.HleIoManager.HleIoDrvFileArgPool.Create(Info.HleIoDrvFileArg);
			}
			catch (DirectoryNotFoundException)
			{
			}
			catch (FileNotFoundException)
			{
			}
			//Console.Error.WriteLine("Didn't find file '{0}'", FileName);
			return unchecked((int)SceKernelErrors.ERROR_ERRNO_FILE_NOT_FOUND);
		}

		/// <summary>
		/// Write output
		/// </summary>
		/// <example>
		///		bytes_written = sceIoWrite(fd, data, 100);
		/// </example>
		/// <param name="FileHandle">Opened file descriptor to write to</param>
		/// <param name="InputPointer">Pointer to the data to write</param>
		/// <param name="InputSize">Size of data to write</param>
		/// <returns>The number of bytes written</returns>
		[HlePspFunction(NID = 0x42EC03AC, FirmwareVersion = 150)]
		public int sceIoWrite(int FileHandle, byte* InputPointer, int InputSize)
		{
			try
			{
				var HleIoDrvFileArg = GetFileArgFromHandle(FileHandle);
				return HleIoDrvFileArg.HleIoDriver.IoWrite(HleIoDrvFileArg, InputPointer, InputSize);
			}
			catch (Exception)
			{
				//Console.Error.WriteLine(Exception);
				return -1;
			}
		}

		/// <summary>
		/// Assigns one IO device to another (I guess)
		/// </summary>
		/// <example>
		/// // Reassign flash0 in read/write mode.
		/// sceIoUnassign("flash0");
		/// sceIoAssign("flash0", "lflash0:0,0", "flashfat0:", IOASSIGN_RDWR, NULL, 0);
		/// </example>
		/// <param name="Device1">The device name to assign.</param>
		/// <param name="Device2">The block device to assign from.</param>
		/// <param name="Device3">The filesystem device to mape the block device to dev1</param>
		/// <param name="mode">Read/Write mode. One of IoAssignPerms.</param>
		/// <param name="unk1">Unknown, set to NULL.</param>
		/// <param name="unk2">Unknown, set to 0.</param>
		/// <returns>
		///		Less than 0 on error.
		/// </returns>
		[HlePspFunction(NID = 0xB2A628C1, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceIoAssign(string Device1, string Device2, string Device3, int mode, void* unk1, long unk2)
		{
			// IoFileMgrForUser.sceIoAssign(Device1:'disc0:', Device2:'umd0:', Device3:'isofs0:', mode:1, unk1:0x00000000, unk2:0x0880001E)
			//throw(new NotImplementedException());
			return 0;
		}
	}
}
