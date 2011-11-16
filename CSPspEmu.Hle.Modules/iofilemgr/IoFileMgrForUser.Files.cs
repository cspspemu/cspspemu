using System;
using System.Collections.Generic;
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
		/// <param name="file">The path to the file.</param>
		/// <param name="stat">A pointer to an io_stat_t structure.</param>
		/// <returns>Less than zero on error</returns>
		[HlePspFunction(NID = 0xACE946E8, FirmwareVersion = 150)]
		public int sceIoGetstat(string file, SceIoStat* stat)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Change the status of a file.
		/// </summary>
		/// <param name="file">The path to the file.</param>
		/// <param name="stat">A pointer to an io_stat_t structure.</param>
		/// <param name="bits">Bitmask defining which bits to change.</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0xB8A740F4, FirmwareVersion = 150)]
		public int sceIoChstat(string file, SceIoStat *stat, int bits)
		{
			/*
			unimplemented();
			return -1;
			*/
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Remove directory entry
		/// </summary>
		/// <param name="file">Path to the file to remove</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xF27A9C51, FirmwareVersion = 150)]
		public int sceIoRemove(string file)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Change the name of a file
		/// </summary>
		/// <param name="oldname">The old filename</param>
		/// <param name="newname">The new filename</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0x779103A0, FirmwareVersion = 150)]
		public int sceIoRename(string oldname, string newname)
		{
			/*
			logInfo("sceIoRename('%s', '%s')", oldname, newname);
			try {
				fsroot().rename(oldname, newname);
				return 0;
			} catch (Throwable o) {
				logError("%s", o);
				return -1;
			}
			*/
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Reopens an existing file descriptor.
		/// </summary>
		/// <param name="file">The new file to open.</param>
		/// <param name="flags">The open flags.</param>
		/// <param name="mode">The open mode.</param>
		/// <param name="fd">The old filedescriptor to reopen</param>
		/// <returns>Less than 0 on error, otherwise the reopened fd.</returns>
		[HlePspFunction(NID = 0x3C54E908, FirmwareVersion = 150)]
		public int sceIoReopen(string file, SceIoFlags flags, SceMode mode, SceUID fd)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Reposition read/write file descriptor offset
		/// </summary>
		/// <example>
		///		pos = sceIoLseek(fd, -10, SEEK_END);
		/// </example>
		/// <param name="fd">Opened file descriptor with which to seek</param>
		/// <param name="offset">Relative offset from the start position given by whence</param>
		/// <param name="whence">
		/// Set to SEEK_SET to seek from the start of the file, SEEK_CUR
		/// seek from the current position and SEEK_END to seek from the end.
		/// </param>
		/// <returns>The position in the file after the seek. </returns>
		[HlePspFunction(NID = 0x27EB27B8, FirmwareVersion = 150)]
		public SceOff sceIoLseek(SceUID fd, SceOff offset, int whence)
		{
			/*
			logInfo("sceIoLseek(%d, %d, %d)", fd, offset, whence);
			if (fd < 0) return -1;
			FileHandle fileHandle = uniqueIdFactory.get!FileHandle(fd);
			return fsroot().seek(fileHandle, offset, cast(Whence)whence);
			*/
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Reposition read/write file descriptor offset (32bit mode)
		/// </summary>
		/// <example>
		///		pos = sceIoLseek32(fd, -10, SEEK_END);
		/// </example>
		/// <param name="fd">Opened file descriptor with which to seek</param>
		/// <param name="offset">Relative offset from the start position given by whence</param>
		/// <param name="whence">
		///		Set to SEEK_SET to seek from the start of the file, SEEK_CUR
		///		seek from the current position and SEEK_END to seek from the end.
		/// </param>
		/// <returns>The position in the file after the seek.</returns>
		[HlePspFunction(NID = 0x68963324, FirmwareVersion = 150)]
		public int sceIoLseek32(SceUID fd, int offset, int whence)
		{
			/*
			logInfo("sceIoLseek32(%d, %d, %d)", fd, offset, whence);
			return cast(int)sceIoLseek(fd, offset, whence);
			*/
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Read input
		/// </summary>
		/// <example>
		///     bytes_read = sceIoRead(fd, data, 100);
		/// </example>
		/// <param name="fd">Opened file descriptor to read from</param>
		/// <param name="data">Pointer to the buffer where the read data will be placed</param>
		/// <param name="size">Size of the read in bytes</param>
		/// <returns>The number of bytes read</returns>
		[HlePspFunction(NID = 0x6A638D83, FirmwareVersion = 150)]
		public int sceIoRead(SceUID fd, void* data, SceSize size)
		{
			/*
			try {
				FileHandle fileHandle = uniqueIdFactory.get!FileHandle(fd);
				logInfo("sceIoRead(%d, %08X, %d) : %d", fd, cast(uint)data, size, fileHandle.position);
				if (data is null) return -1;
				try {
					int readed = fsroot().read(fileHandle, (cast(ubyte *)data)[0..size]);
					if (readed == 0) return -1;
					return readed;
				} catch (Throwable o) {
					logError("ERROR: sceIoRead: %s", o);
					return -1;
				}
			} catch (UniqueIdNotFoundException) {
				// @TODO: Check this error. 
				return SceKernelErrors.ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
			}
			*/
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Delete a descriptor
		/// </summary>
		/// <example>
		///		sceIoClose(fd);
		/// </example>
		/// <param name="fd">File descriptor to close</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0x810C4BC3, FirmwareVersion = 150)]
		public int sceIoClose(SceUID fd)
		{
			/*
			logInfo("sceIoClose('%d')", fd);
			if (fd < 0) return -1;
			try {
				FileHandle fileHandle = uniqueIdFactory.get!FileHandle(fd);
				fsroot().close(fileHandle);
				uniqueIdFactory.remove!FileHandle(fd);
				return 0;
			} catch (Throwable o) {
				.writefln("sceIoClose(%d) : %s", fd, o);
				return 0;
			}
			*/
			throw(new NotImplementedException());
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
	}
}
