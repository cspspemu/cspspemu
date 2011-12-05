using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Hle.Formats.audio
{
	unsafe public partial class Vag
	{
		static public readonly float[][] VAG_f = new float[][]
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

			[EndianAttribute(Endianness.BigEndian)]
			public uint VagVersion;

			[EndianAttribute(Endianness.BigEndian)]
			public uint DataSize;

			[EndianAttribute(Endianness.BigEndian)]
			public uint SampleRate;

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
