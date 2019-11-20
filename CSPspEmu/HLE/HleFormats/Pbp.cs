using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats
{
    public class Pbp : IFormatDetector
    {
        public enum Types
        {
            ParamSfo = 0,
            Icon0Png,
            Icon1Pmf,
            Pic0Png,
            Pic1Png,
            Snd0At3,
            PspData,
            PsarData,
        }

        public static readonly string[] Names =
        {
            "param.sfo", "icon0.png", "icon1.pmf", "pic0.png", "pic1.png", "snd0.at3", "psp.data", "psar.data"
        };

        public struct HeaderStruct
        {
            public enum MagicEnum : uint
            {
                ExpectedValue = 0x50425000
            }

            public MagicEnum Magic;
            public uint Version;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)] public uint[] Offsets;
        }

        protected Stream Stream;
        protected HeaderStruct Header;
        protected Dictionary<string, Stream> Files;

        public Pbp Load(Stream stream)
        {
            Stream = stream;
            Header = stream.ReadStruct<HeaderStruct>();
            Files = new Dictionary<string, Stream>();

            if (Header.Magic != HeaderStruct.MagicEnum.ExpectedValue)
            {
                throw new Exception("Not a PBP file");
            }

            var offsets = Header.Offsets.Concat(new[] {(uint) stream.Length}).ToArray();

            for (int n = 0; n < 8; n++)
            {
                Files[Names[n]] = stream.SliceWithBounds(offsets[n + 0], offsets[n + 1]);
            }

            return this;
        }

        public bool ContainsKey(Types type) => Files.ContainsKey(Names[(int) type]);

        public bool ContainsKey(string key) => Files.ContainsKey(key);

        public Stream this[Types type] => Files[Names[(int) type]];

        public Stream this[string key] => Files[key];
    }
}