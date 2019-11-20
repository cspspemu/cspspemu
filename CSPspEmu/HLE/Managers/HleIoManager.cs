using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Vfs;

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
        public IHleIoDriver HleIoDriver => HleIoDrvFileArg.HleIoDriver;
    }

    public class HleIoWrapper
    {
        HleIoManager _hleIoManager;

        public HleIoWrapper(HleIoManager hleIoManager)
        {
            this._hleIoManager = hleIoManager;
        }

        public void Mkdir(string path, SceMode sceMode)
        {
            var pathInfo = _hleIoManager.ParsePath(path);
            pathInfo.HleIoDriver.IoMkdir(pathInfo.HleIoDrvFileArg, pathInfo.LocalPath, sceMode);
        }

        public FileHandle Open(string fileName, HleIoFlags flags, SceMode mode)
        {
            var pathInfo = _hleIoManager.ParsePath(fileName);
            pathInfo.HleIoDrvFileArg.HleIoDriver.IoOpen(pathInfo.HleIoDrvFileArg, pathInfo.LocalPath, flags, mode);
            //return new FileHandle(this.HleIoManager, PathInfo.HleIoDrvFileArg);
            return new FileHandle(pathInfo.HleIoDrvFileArg);
        }

        public byte[] ReadBytes(string fileName)
        {
            using (var file = Open(fileName, HleIoFlags.Read, SceMode.File))
            {
                return file.ReadAll();
            }
        }

        public void WriteBytes(string fileName, byte[] data)
        {
            using (var file = Open(fileName, HleIoFlags.Create | HleIoFlags.Write | HleIoFlags.Truncate, SceMode.All))
            {
                file.WriteBytes(data);
            }
        }
    }

    public class HleIoManager
    {
        protected readonly Dictionary<string, IHleIoDriver> Drivers = new Dictionary<string, IHleIoDriver>();

        public readonly HleUidPoolSpecial<HleIoDrvFileArg, SceUID> HleIoDrvFileArgPool =
            new HleUidPoolSpecial<HleIoDrvFileArg, SceUID>();

        public HleIoWrapper HleIoWrapper;

        public HleIoManager()
        {
            HleIoWrapper = new HleIoWrapper(this);
        }

        /// <summary>
        /// Returns a tuple of Driver/Index/path.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public ParsePathInfo ParsePath(string fullPath)
        {
            //FullPath = FullPath.Replace('\\', '/');

            //Console.Error.WriteLine("FullPath: {0}", FullPath);
            if (fullPath.IndexOf(':') == -1)
            {
                fullPath = CurrentDirectoryPath + "/" + fullPath;
            }

            //Console.Error.WriteLine("FullPath: {0}", FullPath);
            var match = new Regex(@"^([a-zA-Z]+)(\d*):(.*)$").Match(fullPath);
            var driverName = match.Groups[1].Value.ToLower() + ":";
            var fileSystemNumber = 0;
            IHleIoDriver hleIoDriver = null;
            int.TryParse(match.Groups[2].Value, out fileSystemNumber);
            if (!Drivers.TryGetValue(driverName, out hleIoDriver))
            {
                foreach (var driver in Drivers)
                {
                    Console.WriteLine("Available Driver: '{0}'", driver.Key);
                }
                throw new KeyNotFoundException("Can't find HleIoDriver '" + driverName + "'");
            }

            return new ParsePathInfo
            {
                HleIoDrvFileArg = new HleIoDrvFileArg(driverName, hleIoDriver, fileSystemNumber, null),
                LocalPath = match.Groups[3].Value.Replace('\\', '/'),
            };
        }

        public IHleIoDriver GetDriver(string name)
        {
            return Drivers[name];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="driver"></param>
        public void SetDriver(string name, IHleIoDriver driver)
        {
            //Console.Error.WriteLine("SetDriver: {0}", Name);
            //Drivers.Add(Name, Driver);
            Drivers[name] = driver;
            try
            {
                driver.IoInit();
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void RemoveDriver(string name)
        {
            try
            {
                Drivers[name].IoExit();
                Drivers.Remove(name);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public ParsePathInfo ParseDeviceName(string deviceName)
        {
            var match = new Regex(@"^([a-zA-Z]+)(\d*):$").Match(deviceName);
            int fileSystemNumber = 0;
            int.TryParse(match.Groups[2].Value, out fileSystemNumber);

            var baseDeviceName = match.Groups[1].Value + ":";

            //Drivers[
            if (!Drivers.ContainsKey(baseDeviceName))
            {
                throw new NotImplementedException($"Unknown device '{baseDeviceName}'");
            }

            return new ParsePathInfo
            {
                HleIoDrvFileArg = new HleIoDrvFileArg(baseDeviceName, Drivers[baseDeviceName], fileSystemNumber),
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
        /// <param name="directoryPath"></param>
        public void Chdir(string directoryPath)
        {
            CurrentDirectoryPath = directoryPath;
        }
    }
}