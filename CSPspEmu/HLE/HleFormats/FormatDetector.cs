using System;
using System.IO;
using System.Text;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats
{
    public interface IFormatDetector
    {
    }

    public class FormatDetector
    {
        public enum SubType
        {
            Pbp,
            Psf,
            Elf,
            EncryptedElf,
            Cso,
            Dax,
            Iso,
            Unknown,
        }

        public enum Type
        {
            Executable,
            Iso,
            Other,
            Unknown,
        }

        public SubType DetectSubType(Stream stream)
        {
            var startMagic = stream.SliceWithLength(0, 4).ReadAllContentsAsString(Encoding.ASCII);

            //Console.WriteLine(StartMagic);

            if (startMagic == "\u0000PBP") return SubType.Pbp;
            if (startMagic == "\u0000PSF") return SubType.Psf;
            if (startMagic == "\u007FELF") return SubType.Elf;
            if (startMagic == "~PSP") return SubType.EncryptedElf;
            if (startMagic == "CISO") return SubType.Cso;
            if (startMagic == "DAX\u0000") return SubType.Dax;

            if (stream.SliceWithLength(0x8000, 6).ReadAllContentsAsString() == "\u0001CD001") return SubType.Iso;

            return SubType.Unknown;
        }

        public Type DetectType(Stream stream)
        {
            switch (DetectSubType(stream))
            {
                case SubType.Cso:
                case SubType.Dax:
                case SubType.Iso:
                    return Type.Iso;
                case SubType.Elf:
                case SubType.Pbp:
                    return Type.Executable;
                case SubType.Psf:
                    return Type.Other;
                default:
                    return Type.Unknown;
            }
        }

        public string DetectString(Stream stream) => Enum.GetName(typeof(SubType), DetectSubType(stream));
    }
}