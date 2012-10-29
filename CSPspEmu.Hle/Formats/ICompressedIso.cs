namespace CSPspEmu.Hle.Formats
{
	public interface ICompressedIso
	{
		long UncompressedLength { get; }
		int BlockSize { get; }
		byte[] ReadBlockDecompressed(uint Block);
	}
}
