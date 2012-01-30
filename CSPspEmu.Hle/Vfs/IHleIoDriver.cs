using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Vfs
{
	public enum HleIoFlags : uint
	{
		Read = 0x0001,
		Write = 0x0002,
		ReadWrite = Read | Write,
		NoBlock = 0x0004,
		_InternalDirOpen = 0x0008, // Internal use for dopen
		Append = 0x0100,
		Create = 0x0200,
		Truncate = 0x0400,
		Excl = 0x0800,
		Unknown1 = 0x4000, // something async?
		NoWait = 0x8000,
		Unknown2 = 0xf0000, // seen on Wipeout Pure and Infected
		Unknown3 = 0x2000000, // seen on Puzzle Guzzle, Hammerin' Hero
	}

	public class HleIoDrvFileArg : IDisposable
	{
		/// <summary>
		/// Original driver.
		/// </summary>
		public IHleIoDriver HleIoDriver;

		/// <summary>
		/// The file system number, e.g. if a file is opened as host5:/myfile.txt this field will be 5
		/// </summary>
		public int FileSystemNumber;

		/// <summary>
		/// Pointer to a user defined argument, this is preserved on a per file basis
		/// </summary>
		public IDisposable FileArgument;

		/// <summary>
		/// Last result produced by an async call.
		/// </summary>
		public long AsyncLastResult;

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			if (FileArgument != null)
			{
				FileArgument.Dispose();
			}
		}
	}

	/// <summary>
	/// Access modes for st_mode in SceIoStat (confirm?).
	/// </summary>
	public enum SceMode : uint
	{
		/// <summary>
		/// Format bits mask
		/// </summary>
		FormatMask = 0xF000,

		/// <summary>
		/// Symbolic link
		/// </summary>
		SymbolicLink = 0x4000,

		/// <summary>
		/// Directory
		/// </summary>
		Directory = 0x1000,
		
		/// <summary>
		/// Regular file
		/// </summary>
		File = 0x2000,

		/// <summary>
		/// Set UID
		/// </summary>
		Uid = 0x0800,
		
		/// <summary>
		/// Set GID
		/// </summary>
		Gid = 0x0400,
		
		/// <summary>
		/// Sticky
		/// </summary>
		Sticky = 0x0200,

		All = UserMask | GroupMask | OtherMask,

		/// <summary>
		/// User access rights mask
		/// </summary>
		UserMask = 0x01C0,
		
		/// <summary>
		/// Read user permission
		/// </summary>
		UserCanRead = 0x0100,
		
		/// <summary>
		/// Write user permission
		/// </summary>
		UserCanWrite = 0x0080,
		
		/// <summary>
		/// Execute user permission
		/// </summary>
		UserCanExecute = 0x0040,

		/// <summary>
		/// Group access rights mask
		/// </summary>
		GroupMask = 0x0038,
		
		/// <summary>
		/// Group read permission
		/// </summary>
		GroupCanRead = 0x0020,
		
		/// <summary>
		/// Group write permission
		/// </summary>
		GroupCanWrite = 0x0010,
		
		/// <summary>
		/// Group execute permission
		/// </summary>
		GroupCanExecute = 0x0008,

		/// <summary>
		/// Others access rights mask
		/// </summary>
		OtherMask = 0x0007,
		
		/// <summary>
		/// Others read permission
		/// </summary>
		OtherCanRead = 0x0004,
		
		/// <summary>
		/// Others write permission
		/// </summary>
		OtherCanWrite = 0x0002,
		
		/// <summary>
		/// Others execute permission
		/// </summary>
		OtherCanExecute = 0x0001,
	}

	/// <summary>
	/// File modes, used for the st_attr parameter in SceIoStat (confirm?).
	/// </summary>
	public enum IOFileModes : uint
	{
		/// <summary>
		/// Format mask
		/// </summary>
		FormatMask = 0x0038,

		/// <summary>
		/// Symbolic link
		/// </summary>
		SymbolicLink = 0x0008,
		
		/// <summary>
		/// Directory
		/// </summary>
		Directory = 0x0010,
		
		/// <summary>
		/// Regular file
		/// </summary>
		File = 0x0020,

		/// <summary>
		/// Hidden read permission
		/// </summary>
		CanRead = 0x0004,
		
		/// <summary>
		/// Hidden write permission
		/// </summary>
		CanWrite = 0x0002,
		
		/// <summary>
		/// Hidden execute permission
		/// </summary>
		CanExecute = 0x0001,
	}

	public enum SceOff : long
	{
	}

	/// <summary>
	/// Date and time.
	/// </summary>
	public struct ScePspDateTime
	{
		public ushort Year;
		public ushort Month;
		public ushort Day;
		public ushort Hour;
		public ushort Minute;
		public ushort Second;
		public uint Microsecond;

		static public ScePspDateTime FromDateTime(DateTime DateTime)
		{
			return new ScePspDateTime()
			{
				Year = (ushort)DateTime.Year,
				Month = (ushort)DateTime.Month,
				Day = (ushort)DateTime.Day,
				Hour = (ushort)DateTime.Hour,
				Minute = (ushort)DateTime.Minute,
				Second = (ushort)DateTime.Second,
				Microsecond = (uint)(DateTime.Millisecond * 1000),
			};
		}

		public DateTime ToDateTime()
		{
			return new DateTime((int)Year, (int)Month, (int)Day, (int)Hour, (int)Minute, (int)Second, (int)Microsecond / 1000, DateTimeKind.Utc);
		}
	}

	/// <summary>
	/// Structure to hold the status information about a file
	/// </summary>
	unsafe public struct SceIoStat
	{
		/// <summary>
		/// 
		/// </summary>
		public SceMode Mode;

		/// <summary>
		/// 
		/// </summary>
		public IOFileModes Attributes;
		
		/// <summary>
		/// Size of the file in bytes.
		/// </summary>
		public long Size;
		
		/// <summary>
		/// Creation time.
		/// </summary>
		public ScePspDateTime TimeCreation;
		
		/// <summary>
		/// Access time.
		/// </summary>
		public ScePspDateTime TimeLastAccess;
		
		/// <summary>
		/// Modification time.
		/// </summary>
		public ScePspDateTime TimeLastModification;
		
		/// <summary>
		/// Device-specific data.
		/// 
		/// For example.
		/// The UMD driver stores
		/// the sector of a file in the first index.
		/// </summary>
		public uint DeviceDependentData0;
		public uint DeviceDependentData1;
		public uint DeviceDependentData2;
		public uint DeviceDependentData3;
		public uint DeviceDependentData4;
		public uint DeviceDependentData5;
	}

	/// <summary>
	/// Describes a single directory entry
	/// </summary>
	unsafe public struct HleIoDirent : IDisposable
	{
		/// <summary>
		/// File status.
		/// </summary>
		public SceIoStat Stat;

		/// <summary>
		/// File name.
		/// </summary>
		public fixed byte Name[256];
		
		/// <summary>
		/// Device-specific data.
		/// </summary>
		public uint PrivateData;

		/// <summary>
		/// 
		/// </summary>
		public uint Dummy;

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
		}
	}

	/// <summary>
	/// 
	/// </summary>
	unsafe public interface IHleIoDriver
	{
		/// <summary>
		/// Initializes the Driver
		/// </summary>
		/// <param name="PspIoDrvArg"></param>
		/// <returns></returns>
		int IoInit();

		/// <summary>
		/// Deinitializes the driver.
		/// </summary>
		/// <param name="PspIoDrvArg"></param>
		/// <returns></returns>
		int IoExit(); 

		/// <summary>
		/// Opens a file.
		/// If the user opened 'host0:/path/to/file.txt', this function will get 'path/to/file.txt'
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="FileName"></param>
		/// <param name="Flags"></param>
		/// <param name="Mode"></param>
		/// <returns></returns>
		int IoOpen(HleIoDrvFileArg HleIoDrvFileArg, string FileName, HleIoFlags Flags, SceMode Mode);

		/// <summary>
		/// Closes a file.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <returns></returns>
		int IoClose(HleIoDrvFileArg HleIoDrvFileArg);

		/// <summary>
		/// Reads data from a file.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="OutputPointer"></param>
		/// <param name="OutputLength"></param>
		/// <returns></returns>
		int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength);

		/// <summary>
		/// Writes data to a file.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="InputPointer"></param>
		/// <param name="InputLength"></param>
		/// <returns></returns>
		int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength);

		/// <summary>
		/// Seeks a file.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="Offset"></param>
		/// <param name="Whence"></param>
		/// <returns></returns>
		long IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, SeekAnchor Whence);

		/// <summary>
		/// Sends a command to the driver.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="Command"></param>
		/// <param name="InputPointer"></param>
		/// <param name="InputLength"></param>
		/// <param name="OutputPointer"></param>
		/// <param name="OutputLength"></param>
		/// <returns></returns>
		int IoIoctl(HleIoDrvFileArg HleIoDrvFileArg, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength);

		/// <summary>
		/// Removes a file.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="Name"></param>
		/// <returns></returns>
		int IoRemove(HleIoDrvFileArg HleIoDrvFileArg, string Name);

		/// <summary>
		/// Creates a file.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="Name"></param>
		/// <param name="Mode"></param>
		/// <returns></returns>
		int IoMkdir(HleIoDrvFileArg HleIoDrvFileArg, string Name, SceMode Mode);

		/// <summary>
		/// Removes a directory.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="Name"></param>
		/// <returns></returns>
		int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name);

		/// <summary>
		/// Opens a directory for listing.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="Name"></param>
		/// <returns></returns>
		int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name);

		/// <summary>
		/// Closes a directory for listing.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <returns></returns>
		int IoDclose(HleIoDrvFileArg HleIoDrvFileArg);

		/// <summary>
		/// Reads an entry from the file.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="dir"></param>
		/// <returns></returns>
		int IoDread(HleIoDrvFileArg HleIoDrvFileArg, HleIoDirent* dir);

		/// <summary>
		/// Obtains a stat of a file.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="FileName"></param>
		/// <param name="Stat"></param>
		/// <returns></returns>
		int IoGetstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat);

		/// <summary>
		/// Changes the stat of a file.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="FileName"></param>
		/// <param name="stat"></param>
		/// <param name="bits"></param>
		/// <returns></returns>
		int IoChstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* stat, int bits);

		/// <summary>
		/// Renames a file.
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="OldFileName"></param>
		/// <param name="NewFileName"></param>
		/// <returns></returns>
		int IoRename(HleIoDrvFileArg HleIoDrvFileArg, string OldFileName, string NewFileName);

		/// <summary>
		/// Changes the current directory.
		/// </summary>
		/// <remarks>@TODO Check if it is device based or global.</remarks>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="DirectoryName"></param>
		/// <returns></returns>
		int IoChdir(HleIoDrvFileArg HleIoDrvFileArg, string DirectoryName);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <returns></returns>
		int IoMount(HleIoDrvFileArg HleIoDrvFileArg);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <returns></returns>
		int IoUmount(HleIoDrvFileArg HleIoDrvFileArg);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <param name="DeviceName"></param>
		/// <param name="Command"></param>
		/// <param name="InputPointer"></param>
		/// <param name="InputLength"></param>
		/// <param name="OutputPointer"></param>
		/// <param name="OutputLength"></param>
		/// <returns></returns>
		int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="HleIoDrvFileArg"></param>
		/// <returns></returns>
		int IoUnk21(HleIoDrvFileArg HleIoDrvFileArg); 
	}
}
