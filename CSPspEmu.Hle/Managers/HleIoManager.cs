using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Core;
using System.IO;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Managers
{
	public struct ParsePathInfo
	{
		/// <summary>
		/// 
		/// </summary>
		public HleIoDrvFileArg HleIoDrvFileArg;

		/// <summary>
		/// 
		/// </summary>
		public string LocalPath;

		/// <summary>
		/// 
		/// </summary>
		public IHleIoDriver HleIoDriver
		{
			get
			{
				return HleIoDrvFileArg.HleIoDriver;
			}
		}
	}

	unsafe public class HleIoWrapper
	{
		HleIoManager HleIoManager;

		unsafe public class FileHandle : Stream
		{
			HleIoWrapper HleIoWrapper;
			HleIoDrvFileArg HleIoDrvFileArg;

			public HleIoManager HleIoManager
			{
				get
				{
					return HleIoWrapper.HleIoManager;
				}
			}
			public IHleIoDriver HleIoDriver
			{
				get
				{
					return HleIoDrvFileArg.HleIoDriver;
				}
			}

			internal FileHandle(HleIoWrapper HleIoWrapper, HleIoDrvFileArg HleIoDrvFileArg)
			{
				this.HleIoWrapper = HleIoWrapper;
				this.HleIoDrvFileArg = HleIoDrvFileArg;
			}

			public override bool CanRead
			{
				get { return true; }
			}

			public override bool CanSeek
			{
				get { return true; }
			}

			public override bool CanWrite
			{
				get { return true; }
			}

			public override void Flush()
			{
			}

			public override long Length
			{
				get
				{
					var Previous = HleIoDriver.IoLseek(HleIoDrvFileArg, 0, SeekAnchor.Cursor);
					var Length = HleIoDriver.IoLseek(HleIoDrvFileArg, 0, SeekAnchor.End);
					HleIoDriver.IoLseek(HleIoDrvFileArg, Previous, SeekAnchor.Set);
					return Length;
				}
			}

			public override long Position
			{
				get
				{
					return HleIoDriver.IoLseek(HleIoDrvFileArg, 0, SeekAnchor.Cursor);
				}
				set
				{
					HleIoDriver.IoLseek(HleIoDrvFileArg, value, SeekAnchor.Set);
				}
			}

			public override void SetLength(long value)
			{
				throw new NotImplementedException();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				fixed (byte* FixedBuffer = &buffer[offset])
				{
					return HleIoDriver.IoRead(HleIoDrvFileArg, FixedBuffer, count);
				}
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				return HleIoDriver.IoLseek(HleIoDrvFileArg, offset, (SeekAnchor)origin);
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				fixed (byte* FixedBuffer = &buffer[offset])
				{
					HleIoDriver.IoWrite(HleIoDrvFileArg, FixedBuffer, count);
				}
			}

			public override void Close()
			{
				HleIoDriver.IoClose(HleIoDrvFileArg);
				base.Close();
			}
		}


		public HleIoWrapper(HleIoManager HleIoManager)
		{
			this.HleIoManager = HleIoManager;
		}

		public void Mkdir(string Path, SceMode SceMode)
		{
			var PathInfo = HleIoManager.ParsePath(Path);
			PathInfo.HleIoDriver.IoMkdir(PathInfo.HleIoDrvFileArg, PathInfo.LocalPath, SceMode);
		}

		public FileHandle Open(string FileName, HleIoFlags Flags, SceMode Mode)
		{
			var PathInfo = HleIoManager.ParsePath(FileName);
			PathInfo.HleIoDrvFileArg.HleIoDriver.IoOpen(PathInfo.HleIoDrvFileArg, PathInfo.LocalPath, Flags, Mode);
			return new FileHandle(this, PathInfo.HleIoDrvFileArg);
		}

		public byte[] ReadBytes(string FileName)
		{
			using (var File = Open(FileName, HleIoFlags.Read, SceMode.File))
			{
				return File.ReadAll();
			}
		}

		public void WriteBytes(string FileName, byte[] Data)
		{
			using (var File = Open(FileName, HleIoFlags.Create | HleIoFlags.Write | HleIoFlags.Truncate, SceMode.All))
			{
				File.WriteBytes(Data);
			}
		}
	}

	unsafe public class HleIoManager : PspEmulatorComponent
	{
		protected Dictionary<string, IHleIoDriver> Drivers = new Dictionary<string, IHleIoDriver>();
		public HleIoWrapper HleIoWrapper;

		public HleUidPoolSpecial<HleIoDrvFileArg, SceUID> HleIoDrvFileArgPool = new HleUidPoolSpecial<HleIoDrvFileArg, SceUID>();

		public override void InitializeComponent()
		{
			HleIoWrapper = new HleIoWrapper(this);
		}

		/// <summary>
		/// Returns a tuple of Driver/Index/path.
		/// </summary>
		/// <param name="FullPath"></param>
		/// <returns></returns>
		public ParsePathInfo ParsePath(string FullPath)
		{
			//Console.Error.WriteLine("FullPath: {0}", FullPath);
			if (FullPath.IndexOf(':') == -1)
			{
				FullPath = CurrentDirectoryPath + "/" + FullPath;
			}
			//Console.Error.WriteLine("FullPath: {0}", FullPath);
			var Match = new Regex(@"^(\w+)(\d+):(.*)$").Match(FullPath);
			var DriverName = Match.Groups[1].Value.ToLower() + ":";
			int FileSystemNumber = 0;
			IHleIoDriver HleIoDriver = null;
			Int32.TryParse(Match.Groups[2].Value, out FileSystemNumber);
			if (!Drivers.TryGetValue(DriverName, out HleIoDriver))
			{
				throw(new KeyNotFoundException("Can't find HleIoDriver '" + DriverName + "'"));
			}

			return new ParsePathInfo()
			{
				HleIoDrvFileArg = new HleIoDrvFileArg()
				{
					DriverName = DriverName,
					HleIoDriver = HleIoDriver,
					FileSystemNumber = FileSystemNumber,
					FileArgument = null,
				},
				LocalPath = Match.Groups[3].Value,
			};
		}

		public IHleIoDriver GetDriver(string Name)
		{
			return Drivers[Name];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Name"></param>
		/// <param name="Driver"></param>
		public void SetDriver(string Name, IHleIoDriver Driver)
		{
			//Drivers.Add(Name, Driver);
			Drivers[Name] = Driver;
			try
			{
				Driver.IoInit();
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DeviceName"></param>
		/// <returns></returns>
		public ParsePathInfo ParseDeviceName(string DeviceName)
		{
			var Match = new Regex(@"^([a-zA-Z]+)(\d*):$").Match(DeviceName);
			int FileSystemNumber = 0;
			Int32.TryParse(Match.Groups[2].Value, out FileSystemNumber);

			var BaseDeviceName = Match.Groups[1].Value + ":";

			//Drivers[
			if (!Drivers.ContainsKey(BaseDeviceName))
			{
				throw(new NotImplementedException(String.Format("Unknown device '{0}'", BaseDeviceName)));
			}

			return new ParsePathInfo()
			{
				HleIoDrvFileArg = new HleIoDrvFileArg()
				{
					HleIoDriver = Drivers[BaseDeviceName],
					FileSystemNumber = FileSystemNumber,
					FileArgument = null,
				},
				LocalPath = "",
			};
		}

		/// <summary>
		/// 
		/// </summary>
		public string CurrentDirectoryPath = "";

		/// <summary>
		/// Changes the current directory.
		/// </summary>
		/// <param name="DirectoryPath"></param>
		public void Chdir(string DirectoryPath)
		{
			CurrentDirectoryPath = DirectoryPath;
		}
	}
}
