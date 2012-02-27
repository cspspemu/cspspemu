using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Formats
{
	public interface ICompressedIso
	{
		long UncompressedLength { get; }
		int BlockSize { get; }
		byte[] ReadBlockDecompressed(uint Block);
	}
}
