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
		public enum SceIoFlags : int { }
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
			return Info.HleIoDriver.IoGetstat(Info.HleIoDrvFileArg, FileName, SceIoStat);
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
		public int sceIoReopen(string NewFileName, SceIoFlags SceIoFlags, SceMode SceMode, SceUID FileDescriptor)
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
			return HleIoDrvFileArg.HleIoDriver.IoRead(HleIoDrvFileArg, OutputPointer, OutputSize);
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
			var HleIoDrvFileArg = GetFileArgFromHandle(FileDescriptor);
			return HleIoDrvFileArg.HleIoDriver.IoClose(HleIoDrvFileArg);
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
			try
			{
				Info.HleIoDrvFileArg.HleIoDriver.IoOpen(Info.HleIoDrvFileArg, Info.LocalPath, Flags, Mode);
				return HleState.HleIoManager.HleIoDrvFileArgPool.Create(Info.HleIoDrvFileArg);
			}
			catch (FileNotFoundException)
			{
				return unchecked((int)SceKernelErrors.ERROR_ERRNO_FILE_NOT_FOUND);
			}
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
			var HleIoDrvFileArg = GetFileArgFromHandle(FileHandle);
			return HleIoDrvFileArg.HleIoDriver.IoWrite(HleIoDrvFileArg, InputPointer, InputSize);
		}
	}
}
