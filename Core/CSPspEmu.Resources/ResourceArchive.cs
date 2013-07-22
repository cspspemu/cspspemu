using System.IO;

namespace CSPspEmu.Resources
{
	public class ResourceArchive
	{
		public static Stream GetFlash0ZipFileStream()
		{
			return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("CSPspEmu.Resources.flash0.zip");
		}

		public static Stream GetTranslationsStream()
		{
			return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("CSPspEmu.Resources.Translations.xml");
		}
	}
}
