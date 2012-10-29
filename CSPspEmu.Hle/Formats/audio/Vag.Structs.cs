using CSharpUtils.Endian;

namespace CSPspEmu.Hle.Formats.audio
{
	public unsafe partial class Vag
	{
		public static readonly float[][] VAG_f = new float[][]
		{
			new[] {   0.0f / 64.0f,   0.0f / 64.0f },
			new[] {  60.0f / 64.0f,   0.0f / 64.0f },
			new[] { 115.0f / 64.0f, -52.0f / 64.0f },
			new[] {  98.0f / 64.0f, -55.0f / 64.0f },
			new[] { 122.0f / 64.0f, -60.0f / 64.0f },
		};

		public struct Header
		{
			public uint Magic;

			public uint_be VagVersion;

			public uint_be DataSize;

			public uint_be SampleRate;

			public fixed byte Name[16];
		}

		public struct Block
		{
			public enum TypeEnum : byte
			{
				None = 0,
				LoopEnd = 3,
				LoopStart = 6,
				End = 7,
			}

			public byte Modificator;
			public TypeEnum Type;
			public fixed byte Data[14];
		}
	}
}
