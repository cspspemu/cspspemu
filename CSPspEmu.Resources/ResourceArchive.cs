using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSPspEmu.Resources
{
	public class ResourceArchive
	{
		static public Stream GetFlash0ZipFileStream()
		{
			return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("CSPspEmu.Resources.flash0.zip");
		}
	}
}
