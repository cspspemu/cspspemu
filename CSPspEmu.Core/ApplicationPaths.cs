using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.IO;

namespace CSPspEmu.Core
{
	public class ApplicationPaths
	{
		static public string ExecutablePath
		{
			get
			{
				return Assembly.GetEntryAssembly().Location;
			}
		}

		static private string _MemoryStickRootFolder;

		static public string MemoryStickRootFolder
		{
			get
			{
				if (_MemoryStickRootFolder == null)
				{
					_MemoryStickRootFolder = Path.GetDirectoryName(Application.ExecutablePath) + "/ms";
					if (_MemoryStickRootFolder.Replace('\\', '/').EndsWith("CSPspEmu/bin/Debug/ms"))
					{
						_MemoryStickRootFolder = Path.GetFullPath(MemoryStickRootFolder + "/../../../../ms");
					}
					else if (_MemoryStickRootFolder.Replace('\\', '/').EndsWith("CSPspEmu/bin/Release/ms"))
					{
						_MemoryStickRootFolder = Path.GetFullPath(MemoryStickRootFolder + "/../../../../ms");
					}
					else if (_MemoryStickRootFolder.Replace('\\', '/').EndsWith("CSPspEmu/bin/RunTests/ms"))
					{
						_MemoryStickRootFolder = Path.GetFullPath(MemoryStickRootFolder + "/../../../../ms");
					}
					try { Directory.CreateDirectory(_MemoryStickRootFolder); }
					catch { }
				}
				return _MemoryStickRootFolder;
			}
		}
	}
}
