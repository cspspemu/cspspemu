using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

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
	}
}
