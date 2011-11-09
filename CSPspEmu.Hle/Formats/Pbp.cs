using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSharpUtils.Extensions;
using System.Runtime.InteropServices;

namespace CSPspEmu.Hle.Formats
{
	unsafe public class Pbp : IFormatDetector
	{
		static readonly public String[] Names = new[] { "param.sfo", "icon0.png", "icon1.pmf", "pic0.png", "pic1.png", "snd0.at3", "psp.data", "psar.data" };

		public struct HeaderStruct
		{
			public enum MagicEnum : uint
			{
				ExpectedValue = 0x50425000
			}

			public MagicEnum Magic;
			public uint Version;

			[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
			public uint[] Offsets;
		}

		protected Stream Stream;
		protected HeaderStruct Header;
		protected Dictionary<String, Stream> Files;

		public Pbp Load(Stream Stream)
		{
			this.Stream = Stream;
			this.Header = Stream.ReadStruct<HeaderStruct>();
			this.Files = new Dictionary<string, Stream>();

			if (Header.Magic != HeaderStruct.MagicEnum.ExpectedValue)
			{
				throw(new Exception("Not a PBP file"));
			}

			var Offsets = Header.Offsets.Concat(new[] { (uint)Stream.Length }).ToArray();

			for (int n = 0; n < 8; n++)
			{
				Files[Names[n]] = Stream.SliceWithBounds(Offsets[n + 0], Offsets[n + 1]);
			}

			return this;
		}

		public Stream this[String Key]
		{
			get
			{
				return Files[Key];
			}
		}
	}
}
