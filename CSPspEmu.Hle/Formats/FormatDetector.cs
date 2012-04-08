using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats
{
	public interface IFormatDetector
	{
	}

	unsafe public class FormatDetector
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

		public SubType DetectSubType(Stream Stream)
		{
			var StartMagic = Stream.SliceWithLength(0, 4).ReadAllContentsAsString(Encoding.ASCII);

			//Console.WriteLine(StartMagic);

			if (StartMagic == '\0' + "PBP") return SubType.Pbp;
			if (StartMagic == '\0' + "PSF") return SubType.Psf;
			if (StartMagic == '\x7F' + "ELF") return SubType.Elf;
			if (StartMagic == "~PSP") return SubType.EncryptedElf;
			if (StartMagic == "CISO") return SubType.Cso;
			if (StartMagic == "DAX" + '\0') return SubType.Dax;

			if (Stream.SliceWithLength(0x8000, 6).ReadAllContentsAsString() == '\x01' + "CD001") return SubType.Iso;

			return SubType.Unknown;
		}

		public Type DetectType(Stream Stream)
		{
			switch (DetectSubType(Stream))
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

		public String DetectString(Stream Stream)
		{
			return Enum.GetName(typeof(SubType), DetectSubType(Stream));
		}
	}
}
