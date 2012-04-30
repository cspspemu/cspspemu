using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Modules.stdio;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	unsafe public partial class IoFileMgrForUser
	{
		public class GuestHleIoDriver : IHleIoDriver
		{
			[Inject]
			HleInterop HleInterop;

			PspIoDrv* PspIoDrv;
			PspIoDrvFuncs* PspIoDrvFuncs { get { return PspIoDrv->funcs; } }

			public GuestHleIoDriver(PspEmulatorContext PspEmulatorContext, PspIoDrv* PspIoDrv)
			{
				PspEmulatorContext.InjectDependencesTo(this);
				this.PspIoDrv = PspIoDrv;
			}

			public int IoInit()
			{
				ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () => { Console.WriteLine("Not Implemented: GuestHleIoDriver.IoInit"); });
				return (int)HleInterop.ExecuteFunctionNow(PspIoDrvFuncs->IoInit);
			}

			public int IoExit()
			{
				ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () => { Console.WriteLine("Not Implemented: GuestHleIoDriver.IoExit"); });
				return (int)HleInterop.ExecuteFunctionNow(PspIoDrvFuncs->IoExit);
			}

			public int IoOpen(HleIoDrvFileArg HleIoDrvFileArg, string FileName, HleIoFlags Flags, SceMode Mode)
			{
				//HleInterop.ExecuteFunctionNow(PspIoDrvFuncs->IoOpen);
				ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () => { Console.WriteLine("Not Implemented: GuestHleIoDriver.IoOpen"); });
				//throw new NotImplementedException();
				return 0;
			}

			public int IoClose(HleIoDrvFileArg HleIoDrvFileArg)
			{
				ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () => { Console.WriteLine("Not Implemented: GuestHleIoDriver.IoClose"); });
				//throw new NotImplementedException();
				return 0;
			}

			public int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength)
			{
				ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () => { Console.WriteLine("Not Implemented: GuestHleIoDriver.IoRead"); });
				//throw new NotImplementedException();
				return 0;
			}

			public int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength)
			{
				ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () => { Console.WriteLine("Not Implemented: GuestHleIoDriver.IoWrite"); });
				//throw new NotImplementedException();
				return 0;
			}

			public long IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, SeekAnchor Whence)
			{
				throw new NotImplementedException();
			}

			public int IoIoctl(HleIoDrvFileArg HleIoDrvFileArg, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
			{
				throw new NotImplementedException();
			}

			public int IoRemove(HleIoDrvFileArg HleIoDrvFileArg, string Name)
			{
				throw new NotImplementedException();
			}

			public int IoMkdir(HleIoDrvFileArg HleIoDrvFileArg, string Name, SceMode Mode)
			{
				throw new NotImplementedException();
			}

			public int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name)
			{
				throw new NotImplementedException();
			}

			public int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name)
			{
				throw new NotImplementedException();
			}

			public int IoDclose(HleIoDrvFileArg HleIoDrvFileArg)
			{
				throw new NotImplementedException();
			}

			public int IoDread(HleIoDrvFileArg HleIoDrvFileArg, HleIoDirent* dir)
			{
				throw new NotImplementedException();
			}

			public int IoGetstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat)
			{
				throw new NotImplementedException();
			}

			public int IoChstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* stat, int bits)
			{
				throw new NotImplementedException();
			}

			public int IoRename(HleIoDrvFileArg HleIoDrvFileArg, string OldFileName, string NewFileName)
			{
				throw new NotImplementedException();
			}

			public int IoChdir(HleIoDrvFileArg HleIoDrvFileArg, string DirectoryName)
			{
				throw new NotImplementedException();
			}

			public int IoMount(HleIoDrvFileArg HleIoDrvFileArg)
			{
				throw new NotImplementedException();
			}

			public int IoUmount(HleIoDrvFileArg HleIoDrvFileArg)
			{
				throw new NotImplementedException();
			}

			public int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
			{
				throw new NotImplementedException();
			}

			public int IoUnk21(HleIoDrvFileArg HleIoDrvFileArg)
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Get the status of a file.
		/// </summary>
		/// <param name="FileName">The path to the file.</param>
		/// <param name="SceIoStat">A pointer to an io_stat_t structure.</param>
		/// <returns>Less than zero on error</returns>
		[HlePspFunction(NID = 0xACE946E8, FirmwareVersion = 150)]
		public int sceIoGetstat(string FileName, SceIoStat* SceIoStat)
		{
			var Info = HleIoManager.ParsePath(FileName);
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
			finally
			{
				_DelayIo(IoDelayType.GetStat);
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
			try
			{
				var Info = HleIoManager.ParsePath(FileName);
				return Info.HleIoDriver.IoChstat(Info.HleIoDrvFileArg, FileName, SceIoStat, Bitmask);
			}
			finally
			{
				_DelayIo(IoDelayType.ChStat);
			}
		}

		/// <summary>
		/// Remove directory entry
		/// </summary>
		/// <param name="FileName">Path to the file to remove</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xF27A9C51, FirmwareVersion = 150)]
		public int sceIoRemove(string FileName)
		{
			try
			{
				var Info = HleIoManager.ParsePath(FileName);
				return Info.HleIoDriver.IoRemove(Info.HleIoDrvFileArg, FileName);
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				return -1;
			}
			finally
			{
				_DelayIo(IoDelayType.Remove);
			}
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
			try
			{
				var Info1 = HleIoManager.ParsePath(OldFileName);
				var Info2 = HleIoManager.ParsePath(NewFileName);
				if (!Info1.Equals(Info2)) throw (new NotImplementedException("Rename from different filesystems"));
				return Info1.HleIoDriver.IoRename(Info1.HleIoDrvFileArg, OldFileName, NewFileName);
			}
			finally
			{
				_DelayIo(IoDelayType.Rename);
			}
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
		[HlePspNotImplemented]
		public int sceIoReopen(string NewFileName, HleIoFlags SceIoFlags, SceMode SceMode, SceUID FileDescriptor)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Reposition read/write file descriptor offset
		/// </summary>
		/// <example>
		///		pos = sceIoLseek(fd, -10, SEEK_END);
		/// </example>
		/// <param name="FileId">Opened file descriptor with which to seek</param>
		/// <param name="Offset">Relative offset from the start position given by whence</param>
		/// <param name="Whence">
		/// Set to SEEK_SET to seek from the start of the file, SEEK_CUR
		/// seek from the current position and SEEK_END to seek from the end.
		/// </param>
		/// <returns>The position in the file after the seek. </returns>
		[HlePspFunction(NID = 0x27EB27B8, FirmwareVersion = 150)]
		public long sceIoLseek(SceUID FileId, long Offset, SeekAnchor Whence)
		{
			try
			{
				var HleIoDrvFileArg = GetFileArgFromHandle(FileId);
				return HleIoDrvFileArg.HleIoDriver.IoLseek(HleIoDrvFileArg, Offset, Whence);
			}
			finally
			{
				_DelayIo(IoDelayType.Seek);
			}
		}

		/// <summary>
		/// Reposition read/write file descriptor offset (32bit mode)
		/// </summary>
		/// <example>
		///		pos = sceIoLseek32(fd, -10, SEEK_END);
		/// </example>
		/// <param name="FileId">Opened file descriptor with which to seek</param>
		/// <param name="Offset">Relative offset from the start position given by whence</param>
		/// <param name="Whence">
		///		Set to SEEK_SET to seek from the start of the file, SEEK_CUR
		///		seek from the current position and SEEK_END to seek from the end.
		/// </param>
		/// <returns>The position in the file after the seek.</returns>
		[HlePspFunction(NID = 0x68963324, FirmwareVersion = 150)]
		public int sceIoLseek32(SceUID FileId, int Offset, SeekAnchor Whence)
		{
			try
			{
				var HleIoDrvFileArg = GetFileArgFromHandle(FileId);
				return (int)HleIoDrvFileArg.HleIoDriver.IoLseek(HleIoDrvFileArg, (long)Offset, Whence);
			}
			finally
			{
				_DelayIo(IoDelayType.Seek);
			}
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
		public int sceIoRead(SceUID FileId, byte* OutputPointer, int OutputSize)
		{
			try
			{
				var HleIoDrvFileArg = GetFileArgFromHandle(FileId);
				var Result = HleIoDrvFileArg.HleIoDriver.IoRead(HleIoDrvFileArg, OutputPointer, OutputSize);
				//for (int n = 0; n < OutputSize; n++) Console.Write("{0:X},", OutputPointer[n]);
				return Result;
			}
			finally
			{
				_DelayIo(IoDelayType.Read, OutputSize);
			}
		}

		/// <summary>
		/// Delete a descriptor
		/// </summary>
		/// <example>
		///		sceIoClose(fd);
		/// </example>
		/// <param name="FileId">File descriptor to close</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0x810C4BC3, FirmwareVersion = 150)]
		public int sceIoClose(SceUID FileId)
		{
			try
			{
				var HleIoDrvFileArg = GetFileArgFromHandle(FileId);
				return HleIoDrvFileArg.HleIoDriver.IoClose(HleIoDrvFileArg);
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				return -1;
			}
			finally
			{
				_DelayIo(IoDelayType.Close);
			}
		}

		public SceUID _sceIoOpen(string FileName, HleIoFlags Flags, SceMode Mode, bool Async)
		{
			try
			{
				var Info = HleIoManager.ParsePath(FileName);
				Console.WriteLine("Opened ({3}) '{0}' with driver '{1}' and local path '{2}' : '{2}'", FileName, Info.HleIoDriver, Info.LocalPath, Async ? "Async" : "NO Async");
				Info.HleIoDrvFileArg.HleIoDriver.IoOpen(Info.HleIoDrvFileArg, Info.LocalPath, Flags, Mode);
				Info.HleIoDrvFileArg.FullFileName = FileName;
				return HleIoManager.HleIoDrvFileArgPool.Create(Info.HleIoDrvFileArg);
			}
			catch (DirectoryNotFoundException)
			{
			}
			catch (FileNotFoundException)
			{
			}
			catch (InvalidOperationException InvalidOperationException)
			{
				Console.Error.WriteLine(InvalidOperationException);
			}
			catch (IOException IOException)
			{
				Console.Error.WriteLine(IOException);
			}
			finally
			{
				_DelayIo(IoDelayType.Open);
			}

			//Console.Error.WriteLine("Didn't find file '{0}'", FileName);
			throw (new SceKernelException(SceKernelErrors.ERROR_ERRNO_FILE_NOT_FOUND));
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
		public SceUID sceIoOpen(string FileName, HleIoFlags Flags, SceMode Mode)
		{
			return _sceIoOpen(FileName, Flags, Mode, Async: false);
		}

		/// <summary>
		/// Write output
		/// </summary>
		/// <example>
		///		bytes_written = sceIoWrite(fd, data, 100);
		/// </example>
		/// <param name="FileId">Opened file descriptor to write to</param>
		/// <param name="InputPointer">Pointer to the data to write</param>
		/// <param name="InputSize">Size of data to write</param>
		/// <returns>The number of bytes written</returns>
		[HlePspFunction(NID = 0x42EC03AC, FirmwareVersion = 150)]
		public int sceIoWrite(SceUID FileId, byte* InputPointer, int InputSize)
		{
			try
			{
				switch ((StdioForUser.StdHandle)FileId)
				{
					case StdioForUser.StdHandle.Out:
					case StdioForUser.StdHandle.Error:
						ConsoleUtils.SaveRestoreConsoleState(() =>
						{
							//Console.BackgroundColor = ConsoleColor.DarkGray;
							if ((StdioForUser.StdHandle)FileId == StdioForUser.StdHandle.Out)
							{
								Console.ForegroundColor = ConsoleColor.Blue;
							}
							else
							{
								Console.ForegroundColor = ConsoleColor.Red;
							}
							//Console.Error.WriteLine("Output: '{0}'", PointerUtils.PtrToString(InputPointer, InputSize, Encoding.UTF8));
							Console.Error.Write("{0}", PointerUtils.PtrToString(InputPointer, InputSize, Encoding.UTF8));
						});
						return 0;
				}

				try
				{
					var HleIoDrvFileArg = GetFileArgFromHandle(FileId);
					return HleIoDrvFileArg.HleIoDriver.IoWrite(HleIoDrvFileArg, InputPointer, InputSize);
				}
				catch (Exception Exception)
				{
					Console.Error.WriteLine(Exception);
					return -1;
				}
			}
			finally
			{
				_DelayIo(IoDelayType.Write, InputSize);
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

	public struct PspIoDrvFuncs
	{
		/// <summary>
		/// int (*IoInit)(PspIoDrvArg* arg);
		/// </summary>
		public PspPointer IoInit;

		/// <summary>
		///int (*IoExit)(PspIoDrvArg* arg); 
		/// </summary>
		public PspPointer IoExit;

		/// <summary>
		///int (*IoOpen)(PspIoDrvFileArg *arg, char *file, int flags, SceMode mode); 
		/// </summary>
		public PspPointer IoOpen;

		/// <summary>
		/// int (*IoClose)(PspIoDrvFileArg *arg); 
		/// </summary>
		public PspPointer IoClose;

		/// <summary>
		/// int (*IoRead)(PspIoDrvFileArg *arg, char *data, int len); 
		/// </summary>
		public PspPointer IoRead;

		/// <summary>
		/// int (*IoWrite)(PspIoDrvFileArg *arg, const char *data, int len); 
		/// </summary>
		public PspPointer IoWrite;

		/// <summary>
		/// SceOff (*IoLseek)(PspIoDrvFileArg *arg, SceOff ofs, int whence); 
		/// </summary>
		public PspPointer IoLseek;

		/// <summary>
		/// int (*IoIoctl)(PspIoDrvFileArg *arg, unsigned int cmd, void *indata, int inlen, void *outdata, int outlen);
		/// </summary>
		public PspPointer IoIoctl;

		/// <summary>
		/// int (*IoRemove)(PspIoDrvFileArg *arg, const char *name); 
		/// </summary>
		public PspPointer IoRemove;

		/// <summary>
		/// int (*IoMkdir)(PspIoDrvFileArg *arg, const char *name, SceMode mode); 
		/// </summary>
		public PspPointer IoMkdir;

		/// <summary>
		/// int (*IoRmdir)(PspIoDrvFileArg *arg, const char *name);
		/// </summary>
		public PspPointer IoRmdir;

		/// <summary>
		/// int (*IoDopen)(PspIoDrvFileArg *arg, const char *dirname); 
		/// </summary>
		public PspPointer IoDopen;

		/// <summary>
		/// int (*IoDclose)(PspIoDrvFileArg *arg);
		/// </summary>
		public PspPointer IoDclose;

		/// <summary>
		/// int (*IoDread)(PspIoDrvFileArg *arg, SceIoDirent *dir);
		/// </summary>
		public PspPointer IoDread;

		/// <summary>
		/// int (*IoGetstat)(PspIoDrvFileArg *arg, const char *file, SceIoStat *stat);
		/// </summary>
		public PspPointer IoGetstat;

		/// <summary>
		/// int (*IoChstat)(PspIoDrvFileArg *arg, const char *file, SceIoStat *stat, int bits);
		/// </summary>
		public PspPointer IoChstat;

		/// <summary>
		/// int (*IoRename)(PspIoDrvFileArg *arg, const char *oldname, const char *newname); 
		/// </summary>
		public PspPointer IoRename;

		/// <summary>
		/// int (*IoChdir)(PspIoDrvFileArg *arg, const char *dir); 
		/// </summary>
		public PspPointer IoChdir;

		/// <summary>
		/// int (*IoMount)(PspIoDrvFileArg *arg); 
		/// </summary>
		public PspPointer IoMount;

		/// <summary>
		/// int (*IoUmount)(PspIoDrvFileArg *arg); 
		/// </summary>
		public PspPointer IoUmount;

		/// <summary>
		/// int (*IoDevctl)(PspIoDrvFileArg *arg, const char *devname, unsigned int cmd, void *indata, int inlen, void *outdata, int outlen); 
		/// </summary>
		public PspPointer IoDevctl;

		/// <summary>
		/// int (*IoUnk21)(PspIoDrvFileArg *arg); 
		/// </summary>
		public PspPointer IoUnk21;
	}
}
