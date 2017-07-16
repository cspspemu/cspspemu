using System.Reflection;
using System.Windows.Forms;
using System.IO;

namespace CSPspEmu.Core
{
	public static class ApplicationPaths
	{
		public static string ExecutablePath
		{
			get
			{
				return Assembly.GetEntryAssembly().Location;
			}
		}

		private static string _memoryStickRootFolder;

		public static string MemoryStickRootFolder
		{
			get
			{
				if (_memoryStickRootFolder == null)
				{
					_memoryStickRootFolder = Path.GetDirectoryName(Application.ExecutablePath) + "/ms";
					if (_memoryStickRootFolder.Replace('\\', '/').EndsWith("CSPspEmu/bin/Debug/ms"))
					{
						_memoryStickRootFolder = Path.GetFullPath(MemoryStickRootFolder + "/../../../../ms");
					}
					else if (_memoryStickRootFolder.Replace('\\', '/').EndsWith("CSPspEmu/bin/Release/ms"))
					{
						_memoryStickRootFolder = Path.GetFullPath(MemoryStickRootFolder + "/../../../../ms");
					}
					else if (_memoryStickRootFolder.Replace('\\', '/').EndsWith("CSPspEmu/bin/RunTests/ms"))
					{
						_memoryStickRootFolder = Path.GetFullPath(MemoryStickRootFolder + "/../../../../ms");
					}
					
					try
					{
						Directory.CreateDirectory(_memoryStickRootFolder);
					}
					catch
					{
						// ignored
					}
				}
				return _memoryStickRootFolder;
			}
		}
	}
}
