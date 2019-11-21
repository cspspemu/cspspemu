using CSharpUtils;
using System;
using System.Text;

namespace CSPspEmu.Hle.Vfs
{
    [Flags]
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
        public HleIoDrvFileArg(string DriverName, IHleIoDriver HleIoDriver, int FileSystemNumber = 0,  IDisposable? FileArgument = null)  {
            this.DriverName = DriverName;
            this.HleIoDriver = HleIoDriver;
            this.FileSystemNumber = FileSystemNumber;
            this.FileArgument = FileArgument;
        }

        /// <summary>
        /// 
        /// </summary>
        public string DriverName;

        /// <summary>
        /// Original driver.
        /// </summary>
        public IHleIoDriver HleIoDriver;

        /// <summary>
        /// The file system number, e.g. if a file is opened as host5:/myfile.txt this field will be 5
        /// </summary>
        public int FileSystemNumber;

        /// <summary>
        /// File Name if available.
        /// </summary>
        public string FullFileName;

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
        public object Callback;

        /// <summary>
        /// 
        /// </summary>
        public int CallbackArgument;

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            FileArgument?.Dispose();
        }
    }

    /// <summary>
    /// Access modes for st_mode in SceIoStat (confirm?).
    /// </summary>
    [Flags]
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
    [Flags]
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

        public static ScePspDateTime FromDateTime(DateTime DateTime)
        {
            return new ScePspDateTime()
            {
                Year = (ushort) DateTime.Year,
                Month = (ushort) DateTime.Month,
                Day = (ushort) DateTime.Day,
                Hour = (ushort) DateTime.Hour,
                Minute = (ushort) DateTime.Minute,
                Second = (ushort) DateTime.Second,
                Microsecond = (uint) (DateTime.Millisecond * 1000),
            };
        }

        public DateTime ToDateTime()
        {
            return new DateTime((int) Year, (int) Month, (int) Day, (int) Hour, (int) Minute, (int) Second,
                (int) Microsecond / 1000, DateTimeKind.Utc);
        }

        public long ToUnixTimestamp()
        {
            return (long) (ToDateTime() - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public override string ToString()
        {
            return
                $"ScePspDateTime(Year={Year}, Month={Month}, Day={Day}, Hour={Hour}, Minute={Minute}, Second={Second}, Microsecond={Microsecond})";
        }
    }

    /// <summary>
    /// Structure to hold the status information about a file
    /// </summary>
    public struct SceIoStat
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

        public override string ToString()
        {
            return $"SceIoStat({Mode}, {Size})";
        }
    }

    /// <summary>
    /// Describes a single directory entry
    /// </summary>
    public unsafe struct HleIoDirent : IDisposable
    {
        /// <summary>
        /// File status.
        /// </summary>
        public SceIoStat Stat;

        /// <summary>
        /// File name.
        /// </summary>
        private fixed byte _Name[256];

        public string Name
        {
            set
            {
                fixed (byte* _NamePtr = _Name) PointerUtils.StoreStringOnPtr(value, Encoding.UTF8, _NamePtr);
            }
            get
            {
                fixed (byte* _NamePtr = _Name) return PointerUtils.PtrToStringUtf8(_NamePtr);
            }
        }

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

        public override string ToString()
        {
            return $"HleIoDirent('{Name}', {Stat})";
        }
    }

    public abstract class AbstractHleIoDriver
    {
        protected unsafe Span<T> ReinterpretSpan<T>(Span<byte> Span, int count = 1) where T : unmanaged
        {
            if (Span == null || Span.Length < count * sizeof(T))
                throw new SceKernelException(SceKernelErrors.ERROR_INVALID_ARGUMENT);
            fixed (byte* bp = &Span.GetPinnableReference()) {
                //return new Span<T>(bp, count * sizeof(T));
                return new Span<T>(bp, Span.Length / sizeof(T));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public unsafe interface IHleIoDriver
    {
        int IoInit(); // Initializes the Driver
        int IoExit(); // Deinitializes the driver.
        int IoOpen(HleIoDrvFileArg HleIoDrvFileArg, string FileName, HleIoFlags Flags, SceMode Mode); // Opens a file. If the user opened 'host0:/path/to/file.txt', this function will get 'path/to/file.txt'
        int IoClose(HleIoDrvFileArg HleIoDrvFileArg); // Closes a file.
        int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength); // Reads data from a file.
        int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength); // Writes data to a file.
        long IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, SeekAnchor Whence); // Seeks a file.
        int IoIoctl(HleIoDrvFileArg HleIoDrvFileArg, uint Command, Span<byte> Input, Span<byte> Output); // Sends a command to the driver.
        int IoRemove(HleIoDrvFileArg HleIoDrvFileArg, string Name); // Removes a file.
        int IoMkdir(HleIoDrvFileArg HleIoDrvFileArg, string Name, SceMode Mode); // Creates a file.
        int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name); // Removes a directory.
        int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name); // Opens a directory for listing.
        int IoDclose(HleIoDrvFileArg HleIoDrvFileArg); // Closes a directory for listing.
        int IoDread(HleIoDrvFileArg HleIoDrvFileArg, HleIoDirent* dir); // Reads an entry from the file.
        int IoGetstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat); // Obtains a stat of a file.
        int IoChstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* stat, int bits); // Changes the stat of a file.
        int IoRename(HleIoDrvFileArg HleIoDrvFileArg, string OldFileName, string NewFileName); // Renames a file.
        int IoChdir(HleIoDrvFileArg HleIoDrvFileArg, string DirectoryName); // Changes the current directory.
        int IoMount(HleIoDrvFileArg HleIoDrvFileArg);
        int IoUmount(HleIoDrvFileArg HleIoDrvFileArg);
        int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command, Span<byte> Input,
            Span<byte> Output, ref bool DoDleay);
        int IoUnk21(HleIoDrvFileArg HleIoDrvFileArg);
    }
}