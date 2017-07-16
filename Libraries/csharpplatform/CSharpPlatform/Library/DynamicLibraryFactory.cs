using CSPspEmu.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform.Library
{
	public class DynamicLibraryFactory
	{
		static public IDynamicLibrary CreateForLibrary(string NameWindows, string NameLinux = null, string NameMac = null, string NameAndroid = null)
		{
			if (NameLinux == null) NameLinux = NameWindows;
			if (NameMac == null) NameMac = NameLinux;
			if (NameAndroid == null) NameAndroid = NameLinux;
			
			string Name = NameWindows;
			switch (Platform.OS)
			{
				case OS.Windows: Name = NameWindows; break;
				case OS.Mac: Name = NameMac; break;
				case OS.Android: Name = NameAndroid; break;
				default:
				case OS.Linux: Name = NameLinux; break;
			}

			switch (Platform.OS)
			{
				case OS.Windows: return new DynamicLibraryWindows(Name);
				default: return new DynamicLibraryPosix(Name);
			}
		}

		static public void MapLibraryToType<TType>(IDynamicLibrary DynamicLibrary)
		{
			var Type = typeof(TType);
			foreach (var Field in Type.GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (Field.FieldType.IsSubclassOf(typeof(Delegate)))
				{
					if (Field.GetValue(null) == null)
					{
						var Method = DynamicLibrary.GetMethod(Field.Name);
						if (Method != IntPtr.Zero)
						{
							Field.SetValue(
								null,
								Marshal.GetDelegateForFunctionPointer(
									Method,
									Field.FieldType
								)
							);

						}
						else
						{
							//Console.WriteLine(Field.Name);
						}
					}
				}
			}
		}
	}
}
