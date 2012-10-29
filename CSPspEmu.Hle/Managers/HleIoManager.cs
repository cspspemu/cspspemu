using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Core;

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

    public class HleIoWrapper
	{
		HleIoManager HleIoManager;

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
			//return new FileHandle(this.HleIoManager, PathInfo.HleIoDrvFileArg);
			return new FileHandle(PathInfo.HleIoDrvFileArg);
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

    public class HleIoManager : PspEmulatorComponent
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
			//FullPath = FullPath.Replace('\\', '/');

			//Console.Error.WriteLine("FullPath: {0}", FullPath);
			if (FullPath.IndexOf(':') == -1)
			{
				FullPath = CurrentDirectoryPath + "/" + FullPath;
			}

			Console.WriteLine(FullPath);

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
				HleIoDrvFileArg = new HleIoDrvFileArg(DriverName, HleIoDriver, FileSystemNumber, null),
				LocalPath = Match.Groups[3].Value.Replace('\\', '/'),
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
		/// <param name="Name"></param>
		public void RemoveDriver(string Name)
		{
			try
			{
				Drivers[Name].IoExit();
				Drivers.Remove(Name);
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
				HleIoDrvFileArg = new HleIoDrvFileArg(BaseDeviceName, Drivers[BaseDeviceName], FileSystemNumber),
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
