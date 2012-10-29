using System.IO;
using CSPspEmu.Hle.Vfs.Iso;

namespace CSPspEmu.Hle.Formats
{
	public class IsoLoader
	{
		public static IsoFile GetIso(string IsoFile)
		{
			var IsoFileStream = (Stream)File.OpenRead(IsoFile);
			FormatDetector.SubType DetectedFormat;

			switch (DetectedFormat = new FormatDetector().DetectSubType(IsoFileStream))
			{
				case FormatDetector.SubType.Cso:
					IsoFileStream = new CompressedIsoProxyStream(new Cso(IsoFileStream));
					break;
				case FormatDetector.SubType.Dax:
					IsoFileStream = new CompressedIsoProxyStream(new Dax(IsoFileStream));
					break;
				case FormatDetector.SubType.Iso:
					break;
				default:
					throw (new InvalidDataException("Can't set an ISO for '" + DetectedFormat + "'"));
			}

			return new IsoFile(IsoFileStream, IsoFile);
		}
	}
}
