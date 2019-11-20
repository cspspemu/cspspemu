using System.IO;

namespace CSPspEmu.Hle.Formats
{
    public class IsoLoader
    {
        public static IsoFile GetIso(string isoFile)
        {
            var isoFileStream = (Stream) File.OpenRead(isoFile);
            FormatDetector.SubType detectedFormat;

            switch (detectedFormat = new FormatDetector().DetectSubType(isoFileStream))
            {
                case FormatDetector.SubType.Cso:
                    isoFileStream = new CompressedIsoProxyStream(new Cso(isoFileStream));
                    break;
                case FormatDetector.SubType.Dax:
                    isoFileStream = new CompressedIsoProxyStream(new Dax(isoFileStream));
                    break;
                case FormatDetector.SubType.Iso:
                    break;
                default:
                    throw new InvalidDataException($"Can't set an ISO for '{detectedFormat}' path '{isoFile}'");
            }

            return new IsoFile(isoFileStream, isoFile);
        }
    }
}