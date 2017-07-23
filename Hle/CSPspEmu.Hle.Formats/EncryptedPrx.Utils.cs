using CSPspEmu.Utils;
using System;

namespace CSPspEmu.Hle.Formats
{
    public unsafe partial class EncryptedPrx
    {
        public struct HeaderStruct
        {
            /// <summary>
            /// 0000 - 
            /// </summary>
            public uint Magic;

            /// <summary>
            /// 0004 - 
            /// </summary>
            public ushort ModAttr;

            /// <summary>
            /// 0006 - 
            /// </summary>
            public ushort CompModAttr;

            /// <summary>
            /// 0008 - 
            /// </summary>
            public byte ModVerLo;

            /// <summary>
            /// 0009 - 
            /// </summary>
            public byte ModVerHi;

            /// <summary>
            /// 000A - 
            /// </summary>
            public fixed byte ModuleName[28];

            /// <summary>
            /// 0026 -
            /// </summary>
            public byte ModVersion;

            /// <summary>
            /// 0027 - 
            /// </summary>
            public byte Nsegments;

            /// <summary>
            /// 0028 -
            /// </summary>
            public uint ElfSize;

            /// <summary>
            /// 002C - 
            /// </summary>
            public uint PspSize;

            /// <summary>
            /// 0030 -
            /// </summary>
            public uint BootEntry;

            /// <summary>
            /// 0034 -
            /// </summary>
            public uint ModinfoOffset;

            /// <summary>
            /// 0038 -
            /// </summary>
            public uint BssSize;

            /// <summary>
            /// 003C -
            /// </summary>
            public fixed ushort SegAlign[4];

            /// <summary>
            /// 0044 - 
            /// </summary>
            public fixed uint SegAddress[4];

            /// <summary>
            /// 0054 -
            /// </summary>
            public fixed uint SegSize[4];

            /// <summary>
            /// 0064 -
            /// </summary>
            public fixed uint Reserved[5];

            /// <summary>
            /// 0078 -
            /// </summary>
            public uint DevkitVersion;

            /// <summary>
            /// 007C -
            /// </summary>
            public byte DecMode;

            /// <summary>
            /// 007D -
            /// </summary>
            public byte Pad;

            /// <summary>
            /// 007E -
            /// </summary>
            public ushort OverlapSize;

            /// <summary>
            /// 0080 -
            /// </summary>
            public fixed byte AesKey[16];

            /// <summary>
            /// 0090 -
            /// </summary>
            public fixed byte CmacKey[16];

            /// <summary>
            /// 00A0 -
            /// </summary>
            public fixed byte CmacHeaderHash[16];

            /// <summary>
            /// 00B0 - Size of the compressed chunk (contents of the file excluding this header)
            /// </summary>
            public int CompressedSize;

            /// <summary>
            /// 00B4 - Offset of the compressed chunk in memory?
            /// </summary>
            public uint CompressedOffset;

            /// <summary>
            /// 00B8 -
            /// </summary>
            public uint Unk1;

            /// <summary>
            /// 00BC
            /// </summary>
            public uint Unk2;

            /// <summary>
            /// 00C0 -
            /// </summary>
            public fixed byte CmacDataHash[16];

            /// <summary>
            /// 00D0 -
            /// </summary>
            public uint Tag;

            /// <summary>
            /// 00D4 -
            /// </summary>
            public fixed byte SigCheck[88];

            /// <summary>
            /// 012C -
            /// </summary>
            public fixed byte Sha1Hash[20];

            /// <summary>
            /// 0140 -
            /// </summary>
            public fixed byte KeyData[16];
        }

        /// <summary>
        /// 
        /// </summary>
        public class TagInfo
        {
            /// <summary>
            /// 4 byte value at offset 0xD0 in the PRX file
            /// </summary>
            public uint Tag;

            /// <summary>
            /// 144 bytes keys
            /// </summary>
            public byte[] Key;

            /// <summary>
            /// 
            /// </summary>
            public uint[] Ikey
            {
                set
                {
                    if (value.Length * 4 != 144) throw(new Exception("Invalid entry"));
                    Key = new byte[144];
                    Buffer.BlockCopy(value, 0, Key, 0, value.Length * 4);
                }
            }

            /// <summary>
            /// code for scramble
            /// </summary>
            public int Code;

            /// <summary>
            /// code extra for scramble
            /// </summary>
            public byte CodeExtra;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return CStringFormater.Sprintf("TAG_INFO(tag=0x%08X, key=%s, code=%02X, codeExtra=%02X)", Tag,
                    (Key != null) ? BitConverter.ToString(Key) : "null", Code, CodeExtra);
            }
        }

        public class TagInfo2
        {
            /// <summary>
            /// 4 byte value at offset 0xD0 in the PRX file
            /// </summary>
            public uint Tag;

            /// <summary>
            /// 16 bytes keys
            /// </summary>
            public byte[] Key;

            /// <summary>
            /// code for scramble
            /// </summary>
            public byte Code;

            public override string ToString()
            {
                return CStringFormater.Sprintf("TAG_INFO2(tag=0x%08X, key=%s, code=%02X)", Tag,
                    (Key != null) ? BitConverter.ToString(Key) : "null", Code);
            }
        }
    }
}