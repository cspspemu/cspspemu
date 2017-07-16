using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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

        public static readonly String[] Names = new[]
            {"param.sfo", "icon0.png", "icon1.pmf", "pic0.png", "pic1.png", "snd0.at3", "psp.data", "psar.data"};

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

            var Offsets = Header.Offsets.Concat(new[] {(uint) Stream.Length}).ToArray();

            for (int n = 0; n < 8; n++)
            {
                Files[Names[n]] = Stream.SliceWithBounds(Offsets[n + 0], Offsets[n + 1]);
            }

            return this;
        }

        public bool ContainsKey(Types Type)
        {
            return Files.ContainsKey(Names[(int) Type]);
        }

        public bool ContainsKey(String Key)
        {
            return Files.ContainsKey(Key);
        }

        public Stream this[Types Type]
        {
            get { return Files[Names[(int) Type]]; }
        }

        public Stream this[String Key]
        {
            get { return Files[Key]; }
        }
    }
}