using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GLES
{
	public class OpenglEs
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool SetDllDirectory(string lpPathName);

		static protected void LoadLibrary()
		{
			var Location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var FileToFind = String.Format("/{0}/libGLESv2.dll", Environment.Is64BitProcess ? "x64" : "x86");

			foreach (var Paths1 in new[] { "", "/..", "/../..", "/../../.." })
			{
				foreach (var Paths2 in new[] { "", "/libs" })
				{
					var TryPath = Location + Paths1 + Paths2 + FileToFind;
					//Console.WriteLine("{0}", TryPath);
					if (File.Exists(TryPath))
					{
						SetDllDirectory(Path.GetDirectoryName(TryPath));
						return;
					}
				}
			}

			throw(new Exception("Can't find libGLESv2"));
			//Directory.GetCurrentDirectory()
			//SetDllDirectory(@"C:\projects\cspspemu");
		}

		static bool LoadedAlready = false;

		public static void LoadLibraryOnce()
		{
			if (!LoadedAlready)
			{
				LoadedAlready = true;
				LoadLibrary();
			}
		}
	}
}
