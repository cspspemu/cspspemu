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
		public String Detect(Stream Stream)
		{
			var StartMagic = Stream.SliceWithLength(0, 4).ReadAllContentsAsString(Encoding.ASCII);

			Console.WriteLine(StartMagic);

			if (StartMagic == '\0' + "PBP") return "Pbp";
			if (StartMagic == '\0' + "PSF") return "Psf";
			if (StartMagic == '\x7F' + "ELF") return "Elf";
			if (StartMagic == "CISO") return "Cso";
			if (StartMagic == "DAX" + '\0') return "Dax";

			if (Stream.SliceWithLength(0x8000, 6).ReadAllContentsAsString() == '\x01' + "CD001") return "Iso";

			return "Unknown";
		}
	}
}
