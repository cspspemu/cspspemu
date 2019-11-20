using System;
using System.IO;
using System.Linq;
using CSharpUtils;
using CSPspEmu.Utils;
using Kirk = CSPspEmu.Core.Components.Crypto.Kirk;

namespace CSPspEmu.Hle.Formats
{
    public unsafe class EncryptedPrx
    {
        Kirk _kirk;

        /// <summary>
        /// 
        /// </summary>
        public HeaderStruct Header { get; private set; }

        private TagInfo GetTagInfo(uint checkTag)
        {
            var result = GTagInfo.SingleOrDefault(tag => tag.Tag == checkTag);
            if (result == null) throw new InvalidDataException($"Can't find tag1 0x{checkTag:X}");
            return result;
        }

        private TagInfo2 GetTagInfo2(uint checkTag)
        {
            var result = GTagInfo2.SingleOrDefault(tag => tag.Tag == checkTag);
            if (result == null) throw new InvalidDataException($"Can't find tag2 0x{checkTag:X}");
            return result;
        }

        protected void ExtraV2Mangle(byte* buffer1, byte codeExtra)
        {
            var gDataTmp = new byte[20 + 0xA0];
            fixed (byte* buffer2 = gDataTmp) // aligned
            {
                PointerUtils.Memcpy(buffer2 + 20, buffer1, 0xA0);
                var pl2 = (uint*) buffer2;
                pl2[0] = 5;
                pl2[1] = pl2[2] = 0;
                pl2[3] = codeExtra;
                pl2[4] = 0xA0;

                var ret = _kirk.HleUtilsBufferCopyWithRange(
                    buffer2,
                    20 + 0xA0,
                    buffer2,
                    20 + 0xA0,
                    Kirk.CommandEnum.PspKirkCmdDecrypt
                );

                if (ret != 0)
                {
                    throw new Exception($"extra de-mangle returns {ret}, ");
                }
                // copy result back
                PointerUtils.Memcpy(buffer1, buffer2, 0xA0);
            }
        }

        protected byte[] DecryptPrx1(byte[] pbInBytes, bool showInfo = false)
        {
            var cbTotal = pbInBytes.Length;
            var pbOutBytes = new byte[cbTotal];
            pbInBytes.CopyTo(pbOutBytes, 0);

            fixed (byte* pbIn = pbInBytes)
            fixed (byte* pbOut = pbOutBytes)
            {
                //var headerPointer = (HeaderStruct*) pbIn;
                Header = *(HeaderStruct*) pbIn;
                var pti = GetTagInfo(Header.Tag);

                if (showInfo)
                {
                    Console.WriteLine("TAG_INFO: {0}", pti);
                }

                // build conversion into pbOut
                PointerUtils.Memcpy(pbOut, pbIn, pbInBytes.Length);
                PointerUtils.Memset(pbOut, 0, 0x150);
                PointerUtils.Memset(pbOut, 0x55, 0x40);

                // step3 demangle in place
                var h7Header = (Kirk.KirkAes128CbcHeader*) &pbOut[0x2C];
                h7Header->Mode = Kirk.KirkMode.DecryptCbc;
                h7Header->Unknown4 = 0;
                h7Header->Unknown8 = 0;
                h7Header->KeySeed = pti.Code; // initial seed for PRX
                h7Header->Datasize = 0x70; // size

                // redo part of the SIG check (step2)
                var buffer1Bytes = new byte[0x150];
                fixed (byte* buffer1 = buffer1Bytes)
                {
                    PointerUtils.Memcpy(buffer1 + 0x00, pbIn + 0xD0, 0x80);
                    PointerUtils.Memcpy(buffer1 + 0x80, pbIn + 0x80, 0x50);
                    PointerUtils.Memcpy(buffer1 + 0xD0, pbIn + 0x00, 0x80);

                    if (pti.CodeExtra != 0)
                    {
                        ExtraV2Mangle(buffer1 + 0x10, pti.CodeExtra);
                    }

                    PointerUtils.Memcpy(pbOut + 0x40 /* 0x2C+20 */, buffer1 + 0x40, 0x40);
                }

                for (var iXor = 0; iXor < 0x70; iXor++)
                {
                    pbOut[0x40 + iXor] = (byte) (pbOut[0x40 + iXor] ^ pti.Key[0x14 + iXor]);
                }

                var ret = _kirk.HleUtilsBufferCopyWithRange(
                    pbOut + 0x2C,
                    20 + 0x70,
                    pbOut + 0x2C,
                    20 + 0x70,
                    Kirk.CommandEnum.PspKirkCmdDecrypt
                );

                if (ret != 0)
                    throw new Exception(CStringFormater.Sprintf("mangle#7 returned 0x%08X, ", ret));

                for (var iXor = 0x6F; iXor >= 0; iXor--)
                    pbOut[0x40 + iXor] = (byte) (pbOut[0x2C + iXor] ^ pti.Key[0x20 + iXor]);

                PointerUtils.Memset(pbOut + 0x80, 0, 0x30); // $40 bytes kept, clean up

                pbOut[0xA0] = 1;
                // copy unscrambled parts from header
                PointerUtils.Memcpy(pbOut + 0xB0, pbIn + 0xB0, 0x20); // file size + lots of zeros
                PointerUtils.Memcpy(pbOut + 0xD0, pbIn + 0x00, 0x80); // ~PSP header

                // step4: do the actual decryption of code block
                //  point 0x40 bytes into the buffer to key info
                ret = _kirk.HleUtilsBufferCopyWithRange(
                    pbOut,
                    cbTotal,
                    pbOut + 0x40,
                    cbTotal - 0x40,
                    Kirk.CommandEnum.PspKirkCmdDecryptPrivate
                );

                if (ret != 0)
                    throw new Exception(CStringFormater.Sprintf("mangle#1 returned 0x%08X", ret));

                //File.WriteAllBytes("../../../TestInput/temp.bin", _pbOut);

                var outputSize = *(int*) &pbIn[0xB0];

                return pbOutBytes.Slice(0, outputSize).ToArray();
            }
        }

        protected int Scramble(uint* buf, int size, int code)
        {
            buf[0] = 5;
            buf[1] = buf[2] = 0;
            buf[3] = (uint) code;
            buf[4] = (uint) size;

            if (_kirk.HleUtilsBufferCopyWithRange((byte*) buf, size + 0x14, (byte*) buf, size + 0x14,
                    Kirk.CommandEnum.PspKirkCmdDecrypt) !=
                Kirk.ResultEnum.Ok)
            {
                return -1;
            }

            return 0;
        }

        protected byte[] DecryptPrx2(byte[] pbIn, bool showInfo = false)
        {
            var size = pbIn.Length;
            var pbOut = new byte[size];
            pbIn.CopyTo(pbOut, 0);

            var tmp1Bytes = new byte[0x150];
            var tmp2Bytes = new byte[0x90 + 0x14];
            var tmp3Bytes = new byte[0x60 + 0x14];

            fixed (byte* inbuf = pbIn)
            fixed (byte* outbuf = pbOut)
            fixed (byte* tmp1 = tmp1Bytes)
            fixed (byte* tmp2 = tmp2Bytes)
            fixed (byte* tmp3 = tmp3Bytes)
            {
                //var headerPointer = (HeaderStruct*) inbuf;
                Header = *(HeaderStruct*) inbuf;
                var pti = GetTagInfo2(Header.Tag);
                Console.WriteLine("{0}", pti);

                var retsize = *(int*) &inbuf[0xB0];

                PointerUtils.Memset(tmp1Bytes, 0, 0x150);
                PointerUtils.Memset(tmp2Bytes, 0, 0x90 + 0x14);
                PointerUtils.Memset(tmp3Bytes, 0, 0x60 + 0x14);

                PointerUtils.Memcpy(outbuf, inbuf, size);

                if (size < 0x160)
                {
                    throw new InvalidDataException("buffer not big enough, ");
                }

                if (size - 0x150 < retsize)
                {
                    throw new InvalidDataException("not enough data, ");
                }

                PointerUtils.Memcpy(tmp1, outbuf, 0x150);

                int i, j;
                //byte *p = tmp2+0x14;

                for (i = 0; i < 9; i++)
                {
                    for (j = 0; j < 0x10; j++)
                    {
                        tmp2Bytes[0x14 + (i << 4) + j] = pti.Key[j];
                    }

                    tmp2Bytes[0x14 + (i << 4)] = (byte) i;
                }

                if (Scramble((uint*) tmp2, 0x90, pti.Code) < 0)
                {
                    throw new InvalidDataException("error in Scramble#1, ");
                }

                PointerUtils.Memcpy(outbuf, tmp1 + 0xD0, 0x5C);
                PointerUtils.Memcpy(outbuf + 0x5C, tmp1 + 0x140, 0x10);
                PointerUtils.Memcpy(outbuf + 0x6C, tmp1 + 0x12C, 0x14);
                PointerUtils.Memcpy(outbuf + 0x80, tmp1 + 0x080, 0x30);
                PointerUtils.Memcpy(outbuf + 0xB0, tmp1 + 0x0C0, 0x10);
                PointerUtils.Memcpy(outbuf + 0xC0, tmp1 + 0x0B0, 0x10);
                PointerUtils.Memcpy(outbuf + 0xD0, tmp1 + 0x000, 0x80);

                PointerUtils.Memcpy(tmp3 + 0x14, outbuf + 0x5C, 0x60);

                if (Scramble((uint*) tmp3, 0x60, pti.Code) < 0)
                {
                    throw new InvalidDataException("error in Scramble#2, ");
                }

                PointerUtils.Memcpy(outbuf + 0x5C, tmp3, 0x60);
                PointerUtils.Memcpy(tmp3, outbuf + 0x6C, 0x14);
                PointerUtils.Memcpy(outbuf + 0x70, outbuf + 0x5C, 0x10);
                PointerUtils.Memset(outbuf + 0x18, 0, 0x58);
                PointerUtils.Memcpy(outbuf + 0x04, outbuf, 0x04);

                *((uint*) outbuf) = 0x014C;
                PointerUtils.Memcpy(outbuf + 0x08, tmp2, 0x10);

                /* sha-1 */
                if (_kirk.HleUtilsBufferCopyWithRange(outbuf, 3000000, outbuf, 3000000,
                        Kirk.CommandEnum.PspKirkCmdSha1Hash) !=
                    Kirk.ResultEnum.Ok)
                {
                    throw new InvalidDataException("error in sceUtilsBufferCopyWithRange 0xB, ");
                }

                if (PointerUtils.Memcmp(outbuf, tmp3, 0x14) != 0)
                {
                    throw new InvalidDataException("WARNING (SHA-1 incorrect), ");
                }

                int iXor;

                for (iXor = 0; iXor < 0x40; iXor++)
                {
                    tmp3[iXor + 0x14] = (byte) (outbuf[iXor + 0x80] ^ tmp2Bytes[iXor + 0x10]);
                }

                if (Scramble((uint*) tmp3, 0x40, pti.Code) != 0)
                {
                    throw new InvalidDataException("error in Scramble#3, ");
                }

                for (iXor = 0x3F; iXor >= 0; iXor--)
                {
                    outbuf[iXor + 0x40] = (byte) (tmp3Bytes[iXor] ^ tmp2Bytes[iXor + 0x50]); // uns 8
                }

                PointerUtils.Memset(outbuf + 0x80, 0, 0x30);
                *(uint*) &outbuf[0xA0] = 1;

                PointerUtils.Memcpy(outbuf + 0xB0, outbuf + 0xC0, 0x10);
                PointerUtils.Memset(outbuf + 0xC0, 0, 0x10);

                // the real decryption
                var ret = _kirk.HleUtilsBufferCopyWithRange(outbuf, size, outbuf + 0x40, size - 0x40,
                    Kirk.CommandEnum.PspKirkCmdDecryptPrivate);
                if (ret != 0)
                {
                    throw new InvalidDataException(
                        $"error in sceUtilsBufferCopyWithRange 0x1 (0x{ret:X}), ");
                }

                if (retsize < 0x150)
                {
                    // Fill with 0
                    PointerUtils.Memset(outbuf + retsize, 0, 0x150 - retsize);
                }

                return pbOut.Slice(0, retsize).ToArray();
            }
        }


        public byte[] DecryptPrx(byte[] pbIn, bool showInfo = false)
        {
            try
            {
                return DecryptPrx1(pbIn, showInfo);
            }
            catch (InvalidDataException)
            {
                return DecryptPrx2(pbIn, showInfo);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pbIn"></param>
        /// <param name="showInfo"></param>
        public byte[] Decrypt(byte[] pbIn, bool showInfo = false)
        {
            _kirk = new Kirk();
            _kirk.kirk_init();

            return DecryptPrx(pbIn, showInfo);
        }
        
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
        
        /////////////////////////////////////
        
        public TagInfo[] GTagInfo = {
            // 1.x PRXs
            new TagInfo {Tag = 0x00000000, Ikey = GKey0, Code = 0x42},
            new TagInfo {Tag = 0x02000000, Ikey = GKey2, Code = 0x45},
            new TagInfo {Tag = 0x03000000, Ikey = GKey3, Code = 0x46},

            // 2.0 PRXs
            new TagInfo {Tag = 0x4467415d, Ikey = GKey44, Code = 0x59, CodeExtra = 0x59},
            new TagInfo {Tag = 0x207bbf2f, Ikey = GKey20, Code = 0x5A, CodeExtra = 0x5A},
            new TagInfo {Tag = 0x3ace4dce, Ikey = GKey3A, Code = 0x5B, CodeExtra = 0x5B},

            // misc
            new TagInfo {Tag = 0x07000000, Ikey = GKeyIndexdat1Xx, Code = 0x4A}, // 1.xx index.dat
            new TagInfo {Tag = 0x08000000, Ikey = GKeyEboot1Xx, Code = 0x4B}, // 1.xx game eboot.bin
            new TagInfo
            {
                Tag = 0xC0CB167C,
                Ikey = GKeyEboot2Xx,
                Code = 0x5D,
                CodeExtra = 0x5D
            }, // 2.xx+ game eboot.bin

            new TagInfo {Tag = 0x0B000000, Ikey = GKeyUpdater, Code = 0x4E}, // updater data.psp
            new TagInfo {Tag = 0x0C000000, Ikey = GKeyDemos27X, Code = 0x4F}, // 2.7x demos
            new TagInfo {Tag = 0x0F000000, Ikey = GKeyMeimg250, Code = 0x52}, // 2.50 meimg and me_sdimg
            new TagInfo
            {
                Tag = 0x862648D1,
                Ikey = GKeyMeimg260,
                Code = 0x52,
                CodeExtra = 0x52
            }, // 2.60+ meimg and me_sdimg

            new TagInfo {Tag = 0x207BBF2F, Ikey = GKeyUnk1, Code = 0x5A, CodeExtra = 0x5A}, // unknown

            new TagInfo {Tag = 0x09000000, Ikey = GKeyGameshare1Xx, Code = 0x4C}, // gameshare 1.xx
            new TagInfo
            {
                Tag = 0xBB67C59F,
                Ikey = GKeyGameshare2Xx,
                Code = 0x5E,
                CodeExtra = 0x5E
            }, // gameshare 2.xx (old)
        };

        public static readonly uint[] GKey0 = {
            0x7b21f3be, 0x299c5e1d, 0x1c9c5e71, 0x96cb4645, 0x3c9b1be0, 0xeb85de3d,
            0x4a7f2022, 0xc2206eaa, 0xd50b3265, 0x55770567, 0x3c080840, 0x981d55f2,
            0x5fd8f6f3, 0xee8eb0c5, 0x944d8152, 0xf8278651, 0x2705bafa, 0x8420e533,
            0x27154ae9, 0x4819aa32, 0x59a3aa40, 0x2cb3cf65, 0xf274466d, 0x3a655605,
            0x21b0f88f, 0xc5b18d26, 0x64c19051, 0xd669c94e, 0xe87035f2, 0x9d3a5909,
            0x6f4e7102, 0xdca946ce, 0x8416881b, 0xbab097a5, 0x249125c6, 0xb34c0872,
        };

        public static readonly uint[] GKey2 = {
            0xccfda932, 0x51c06f76, 0x046dcccf, 0x49e1821e, 0x7d3b024c, 0x9dda5865,
            0xcc8c9825, 0xd1e97db5, 0x6874d8cb, 0x3471c987, 0x72edb3fc, 0x81c8365d,
            0xe161e33a, 0xfc92db59, 0x2009b1ec, 0xb1a94ce4, 0x2f03696b, 0x87e236d8,
            0x3b2b8ce9, 0x0305e784, 0xf9710883, 0xb039db39, 0x893bea37, 0xe74d6805,
            0x2a5c38bd, 0xb08dc813, 0x15b32375, 0x46be4525, 0x0103fd90, 0xa90e87a2,
            0x52aba66a, 0x85bf7b80, 0x45e8ce63, 0x4dd716d3, 0xf5e30d2d, 0xaf3ae456,
        };

        public static readonly uint[] GKey3 = {
            0xa6c8f5ca, 0x6d67c080, 0x924f4d3a, 0x047ca06a, 0x08640297, 0x4fd4a758,
            0xbd685a87, 0x9b2701c2, 0x83b62a35, 0x726b533c, 0xe522fa0c, 0xc24b06b4,
            0x459d1cac, 0xa8c5417b, 0x4fea62a2, 0x0615d742, 0x30628d09, 0xc44fab14,
            0x69ff715e, 0xd2d8837d, 0xbeed0b8b, 0x1e6e57ae, 0x61e8c402, 0xbe367a06,
            0x543f2b5e, 0xdb3ec058, 0xbe852075, 0x1e7e4dcc, 0x1564ea55, 0xec7825b4,
            0xc0538cad, 0x70f72c7f, 0x49e8c3d0, 0xeda97ec5, 0xf492b0a4, 0xe05eb02a,
        };

        public static readonly uint[] GKey44 = {
            0xef80e005, 0x3a54689f, 0x43c99ccd, 0x1b7727be, 0x5cb80038, 0xdd2efe62,
            0xf369f92c, 0x160f94c5, 0x29560019, 0xbf3c10c5, 0xf2ce5566, 0xcea2c626,
            0xb601816f, 0x64e7481e, 0x0c34debd, 0x98f29cb0, 0x3fc504d7, 0xc8fb39f0,
            0x0221b3d8, 0x63f936a2, 0x9a3a4800, 0x6ecc32e3, 0x8e120cfd, 0xb0361623,
            0xaee1e689, 0x745502eb, 0xe4a6c61c, 0x74f23eb4, 0xd7fa5813, 0xb01916eb,
            0x12328457, 0xd2bc97d2, 0x646425d8, 0x328380a5, 0x43da8ab1, 0x4b122ac9,
        };

        public static readonly uint[] GKey20 = {
            0x33b50800, 0xf32f5fcd, 0x3c14881f, 0x6e8a2a95, 0x29feefd5, 0x1394eae3,
            0xbd6bd443, 0x0821c083, 0xfab379d3, 0xe613e165, 0xf5a754d3, 0x108b2952,
            0x0a4b1e15, 0x61eadeba, 0x557565df, 0x3b465301, 0xae54ecc3, 0x61423309,
            0x70c9ff19, 0x5b0ae5ec, 0x989df126, 0x9d987a5f, 0x55bc750e, 0xc66eba27,
            0x2de988e8, 0xf76600da, 0x0382dccb, 0x5569f5f2, 0x8e431262, 0x288fe3d3,
            0x656f2187, 0x37d12e9c, 0x2f539eb4, 0xa492998e, 0xed3958f7, 0x39e96523,
        };

        public static readonly uint[] GKey3A = {
            0x67877069, 0x3abd5617, 0xc23ab1dc, 0xab57507d, 0x066a7f40, 0x24def9b9,
            0x06f759e4, 0xdcf524b1, 0x13793e5e, 0x0359022d, 0xaae7e1a2, 0x76b9b2fa,
            0x9a160340, 0x87822fba, 0x19e28fbb, 0x9e338a02, 0xd8007e9a, 0xea317af1,
            0x630671de, 0x0b67ca7c, 0x865192af, 0xea3c3526, 0x2b448c8e, 0x8b599254,
            0x4602e9cb, 0x4de16cda, 0xe164d5bb, 0x07ecd88e, 0x99ffe5f8, 0x768800c1,
            0x53b091ed, 0x84047434, 0xb426dbbc, 0x36f948bb, 0x46142158, 0x749bb492,
        };

        // KEYS FROM MESG_LED.PRX (3.52)
        public static readonly uint[] GKeyEboot1Xx = {
            0x18CB69EF, 0x158E8912, 0xDEF90EBB, 0x4CB0FB23, 0x3687EE18, 0x868D4A6E,
            0x19B5C756, 0xEE16551D, 0xE7CB2D6C, 0x9747C660, 0xCE95143F, 0x2956F477,
            0x03824ADE, 0x210C9DF1, 0x5029EB24, 0x81DFE69F, 0x39C89B00, 0xB00C8B91,
            0xEF2DF9C2, 0xE13A93FC, 0x8B94A4A8, 0x491DD09D, 0x686A400D, 0xCED4C7E4,
            0x96C8B7C9, 0x1EAADC28, 0xA4170B84, 0x505D5DDC, 0x5DA6C3CF, 0x0E5DFA2D,
            0x6E7919B5, 0xCE5E29C7, 0xAAACDB94, 0x45F70CDD, 0x62A73725, 0xCCE6563D,
        };

        public static readonly uint[] GKeyEboot2Xx = {
            0xDA8E36FA, 0x5DD97447, 0x76C19874, 0x97E57EAF, 0x1CAB09BD, 0x9835BAC6,
            0x03D39281, 0x03B205CF, 0x2882E734, 0xE714F663, 0xB96E2775, 0xBD8AAFC7,
            0x1DD3EC29, 0xECA4A16C, 0x5F69EC87, 0x85981E92, 0x7CFCAE21, 0xBAE9DD16,
            0xE6A97804, 0x2EEE02FC, 0x61DF8A3D, 0xDD310564, 0x9697E149, 0xC2453F3B,
            0xF91D8456, 0x39DA6BC8, 0xB3E5FEF5, 0x89C593A3, 0xFB5C8ABC, 0x6C0B7212,
            0xE10DD3CB, 0x98D0B2A8, 0x5FD61847, 0xF0DC2357, 0x7701166A, 0x0F5C3B68,
        };

        public static readonly uint[] GKeyUpdater = {
            0xA5603CBF, 0xD7482441, 0xF65764CC, 0x1F90060B, 0x4EA73E45, 0xE551D192,
            0xE7B75D8A, 0x465A506E, 0x40FB1022, 0x2C273350, 0x8096DA44, 0x9947198E,
            0x278DEE77, 0x745D062E, 0xC148FA45, 0x832582AF, 0x5FDB86DA, 0xCB15C4CE,
            0x2524C62F, 0x6C2EC3B1, 0x369BE39E, 0xF7EB1FC4, 0x1E51CE1A, 0xD70536F4,
            0xC34D39D8, 0x7418FB13, 0xE3C84DE1, 0xB118F03C, 0xA2018D4E, 0xE6D8770D,
            0x5720F390, 0x17F96341, 0x60A4A68F, 0x1327DD28, 0x05944C64, 0x0C2C4C12,
        };

        public static readonly uint[] GKeyMeimg250 = {
            0xA381FEBC, 0x99B9D5C9, 0x6C560A8D, 0x30309F95, 0x792646CC, 0x82B64E5E,
            0x1A3951AD, 0x0A182EC4, 0xC46131B4, 0x77C50C8A, 0x325F16C6, 0x02D1942E,
            0x0AA38AC4, 0x2A940AC6, 0x67034726, 0xE52DB133, 0xD2EF2107, 0x85C81E90,
            0xC8D164BA, 0xC38DCE1D, 0x948BA275, 0x0DB84603, 0xE2473637, 0xCD74FCDA,
            0x588E3D66, 0x6D28E822, 0x891E548B, 0xF53CF56D, 0x0BBDDB66, 0xC4B286AA,
            0x2BEBBC4B, 0xFC261FF4, 0x92B8E705, 0xDCEE6952, 0x5E0442E5, 0x8BEB7F21,
        };

        public static readonly uint[] GKeyMeimg260 = {
            0x11BFD698, 0xD7F9B324, 0xDD524927, 0x16215B86, 0x504AC36D, 0x5843B217,
            0xE5A0DA47, 0xBB73A1E7, 0x2915DB35, 0x375CFD3A, 0xBB70A905, 0x272BEFCA,
            0x2E960791, 0xEA0799BB, 0xB85AE6C8, 0xC9CAF773, 0x250EE641, 0x06E74A9E,
            0x5244895D, 0x466755A5, 0x9A84AF53, 0xE1024174, 0xEEBA031E, 0xED80B9CE,
            0xBC315F72, 0x5821067F, 0xE8313058, 0xD2D0E706, 0xE6D8933E, 0xD7D17FB4,
            0x505096C4, 0xFDA50B3B, 0x4635AE3D, 0xEB489C8A, 0x422D762D, 0x5A8B3231,
        };

        public static readonly uint[] GKeyDemos27X = {
            0x1ABF102F, 0xD596D071, 0x6FC552B2, 0xD4F2531F, 0xF025CDD9, 0xAF9AAF03,
            0xE0CF57CF, 0x255494C4, 0x7003675E, 0x907BC884, 0x002D4EE4, 0x0B687A0D,
            0x9E3AA44F, 0xF58FDA81, 0xEC26AC8C, 0x3AC9B49D, 0x3471C037, 0xB0F3834D,
            0x10DC4411, 0xA232EA31, 0xE2E5FA6B, 0x45594B03, 0xE43A1C87, 0x31DAD9D1,
            0x08CD7003, 0xFA9C2FDF, 0x5A891D25, 0x9B5C1934, 0x22F366E5, 0x5F084A32,
            0x695516D5, 0x2245BE9F, 0x4F6DD705, 0xC4B8B8A1, 0xBC13A600, 0x77B7FC3B,
        };

        public static readonly uint[] GKeyUnk1 = {
            0x33B50800, 0xF32F5FCD, 0x3C14881F, 0x6E8A2A95, 0x29FEEFD5, 0x1394EAE3,
            0xBD6BD443, 0x0821C083, 0xFAB379D3, 0xE613E165, 0xF5A754D3, 0x108B2952,
            0x0A4B1E15, 0x61EADEBA, 0x557565DF, 0x3B465301, 0xAE54ECC3, 0x61423309,
            0x70C9FF19, 0x5B0AE5EC, 0x989DF126, 0x9D987A5F, 0x55BC750E, 0xC66EBA27,
            0x2DE988E8, 0xF76600DA, 0x0382DCCB, 0x5569F5F2, 0x8E431262, 0x288FE3D3,
            0x656F2187, 0x37D12E9C, 0x2F539EB4, 0xA492998E, 0xED3958F7, 0x39E96523,
        };

        public static readonly uint[] GKeyGameshare1Xx = {
            0x721B53E8, 0xFC3E31C6, 0xF85BA2A2, 0x3CF0AC72, 0x54EEA7AB, 0x5959BFCB,
            0x54B8836B, 0xBC431313, 0x989EF2CF, 0xF0CE36B2, 0x98BA4CF8, 0xE971C931,
            0xA0375DC8, 0x08E52FA0, 0xAC0DD426, 0x57E4D601, 0xC56E61C7, 0xEF1AB98A,
            0xD1D9F8F4, 0x5FE9A708, 0x3EF09D07, 0xFA0C1A8C, 0xA91EEA5C, 0x58F482C5,
            0x2C800302, 0x7EE6F6C3, 0xFF6ABBBB, 0x2110D0D0, 0xD3297A88, 0x980012D3,
            0xDC59C87B, 0x7FDC5792, 0xDB3F5DA6, 0xFC23B787, 0x22698ED3, 0xB680E812,
        };

        public static readonly uint[] GKeyGameshare2Xx = {
            0x94A757C7, 0x9FD39833, 0xF8508371, 0x328B0B29, 0x2CBCB9DA, 0x2918B9C6,
            0x944C50BA, 0xF1DCE7D0, 0x640C3966, 0xC90B3D08, 0xF4AD17BA, 0x6CA0F84B,
            0xF7767C67, 0xA4D3A55A, 0x4A085C6A, 0x6BB27071, 0xFA8B38FB, 0x3FDB31B8,
            0x8B7196F2, 0xDB9BED4A, 0x51625B84, 0x4C1481B4, 0xF684F508, 0x30B44770,
            0x93AA8E74, 0x90C579BC, 0x246EC88D, 0x2E051202, 0xC774842E, 0xA185D997,
            0x7A2B3ADD, 0xFE835B6D, 0x508F184D, 0xEB4C4F13, 0x0E1993D3, 0xBA96DFD2,
        };

        public static readonly uint[] GKeyIndexdat1Xx = {
            0x76CB00AF, 0x111CE62F, 0xB7B27E36, 0x6D8DE8F9, 0xD54BF16A, 0xD9E90373,
            0x7599D982, 0x51F82B0E, 0x636103AD, 0x8E40BC35, 0x2F332C94, 0xF513AAE9,
            0xD22AFEE9, 0x04343987, 0xFC5BB80C, 0x12349D89, 0x14A481BB, 0x25ED3AE8,
            0x7D500E4F, 0x43D1B757, 0x7B59FDAD, 0x4CFBBF34, 0xC3D17436, 0xC1DA21DB,
            0xA34D8C80, 0x962B235D, 0x3E420548, 0x09CF9FFE, 0xD4883F5C, 0xD90E9CB5,
            0x00AEF4E9, 0xF0886DE9, 0x62A58A5B, 0x52A55546, 0x971941B5, 0xF5B79FAC,
        };
        
        ////////////////
        public TagInfo2[] GTagInfo2 =
        {
            new TagInfo2() {Tag = 0x380228F0, Key = Keys620_5V, Code = 0x5A}, // -- PSPgo PSPgo 6.XX vshmain
            new TagInfo2() {Tag = 0x4C942AF0, Key = Keys620_5K, Code = 0x43}, // PSPgo 6.XX
            new TagInfo2() {Tag = 0x4C9428F0, Key = Keys6205, Code = 0x43}, // PSPgo
            new TagInfo2() {Tag = 0x4C9429F0, Key = Keys570_5K, Code = 0x43}, // PSPgo 5.70
            new TagInfo2() {Tag = 0x4C941DF0, Key = Keys6201, Code = 0x43}, // -- 6.00-6.20
            new TagInfo2() {Tag = 0x4C941CF0, Key = Keys6200, Code = 0x43},
            new TagInfo2() {Tag = 0x457B1EF0, Key = Keys6203, Code = 0x5B}, // pops_04g.prx
            new TagInfo2() {Tag = 0x457B0BF0, Key = Keys600U1457B0Bf0, Code = 0x5B}, // -- 5.55 user modules
            new TagInfo2() {Tag = 0x457B0CF0, Key = Keys600U1457B0Cf0, Code = 0x5B},
            new TagInfo2() {Tag = 0x4C9419F0, Key = Keys5001, Code = 0x43}, // -- 5.00 - 5.50
            new TagInfo2() {Tag = 0x4C9418F0, Key = Keys5000, Code = 0x43},
            new TagInfo2() {Tag = 0x4C941FF0, Key = Keys5002, Code = 0x43},
            new TagInfo2() {Tag = 0x4C9417F0, Key = Keys5001, Code = 0x43},
            new TagInfo2() {Tag = 0x4C9416F0, Key = Keys5000, Code = 0x43},
            new TagInfo2() {Tag = 0x4C9414F0, Key = Keys3900, Code = 0x43}, // -- 3.90 keys
            new TagInfo2() {Tag = 0x4C9415F0, Key = Keys3901, Code = 0x43},
            new TagInfo2() {Tag = 0xD82310F0, Key = Keys02Ge, Code = 0x51}, // -- ipl and file tables
            new TagInfo2() {Tag = 0xD8231EF0, Key = Keys03Ge, Code = 0x51},
            new TagInfo2() {Tag = 0xD82328F0, Key = Keys05Ge, Code = 0x51},
            new TagInfo2() {Tag = 0x4C9412F0, Key = Keys3700, Code = 0x43}, // -- 3.60-3.7X keys
            new TagInfo2() {Tag = 0x4C9413F0, Key = Keys3701, Code = 0x43},
            new TagInfo2() {Tag = 0x457B10F0, Key = Keys3702, Code = 0x5B},
            new TagInfo2() {Tag = 0x4C940DF0, Key = Keys3600, Code = 0x43},
            new TagInfo2() {Tag = 0x4C9410F0, Key = Keys3601, Code = 0x43},
            new TagInfo2() {Tag = 0x4C940BF0, Key = Keys3300, Code = 0x43}, // -- 3.30-3.51
            new TagInfo2() {Tag = 0x457B0AF0, Key = Keys3301, Code = 0x5B},
            new TagInfo2() {Tag = 0x38020AF0, Key = Keys3302, Code = 0x5A},
            new TagInfo2() {Tag = 0x4C940AF0, Key = Keys3303, Code = 0x43},
            new TagInfo2() {Tag = 0x4C940CF0, Key = Keys3304, Code = 0x43},
            new TagInfo2() {Tag = 0xcfef09f0, Key = Keys3100, Code = 0x62}, // -- 3.10
            new TagInfo2() {Tag = 0x457b08f0, Key = Keys3101, Code = 0x5B},
            new TagInfo2() {Tag = 0x380208F0, Key = Keys3102, Code = 0x5A},
            new TagInfo2() {Tag = 0xcfef08f0, Key = Keys3103, Code = 0x62},
            new TagInfo2() {Tag = 0xCFEF07F0, Key = Keys3030, Code = 0x62}, // -- 2.60-3.03
            new TagInfo2() {Tag = 0xCFEF06F0, Key = Keys3000, Code = 0x62},
            new TagInfo2() {Tag = 0x457B06F0, Key = Keys3001, Code = 0x5B},
            new TagInfo2() {Tag = 0x380206F0, Key = Keys3002, Code = 0x5A},
            new TagInfo2() {Tag = 0xCFEF05F0, Key = Keys2800, Code = 0x62},
            new TagInfo2() {Tag = 0x457B05F0, Key = Keys2801, Code = 0x5B},
            new TagInfo2() {Tag = 0x380205F0, Key = Keys2802, Code = 0x5A},
            new TagInfo2() {Tag = 0x16D59E03, Key = Keys2600, Code = 0x62},
            new TagInfo2() {Tag = 0x76202403, Key = Keys2601, Code = 0x5B},
            new TagInfo2() {Tag = 0x0F037303, Key = Keys2602, Code = 0x5A},
            new TagInfo2() {Tag = 0x457B28F0, Key = Keys620E, Code = 0x5B}, // -- misc ?
            new TagInfo2() {Tag = 0xADF305F0, Key = Demokeys280, Code = 0x60}, // 2.80 demos data.psp
            new TagInfo2() {Tag = 0xADF306F0, Key = Demokeys3Xx1, Code = 0x60}, // 3.XX demos 1
            new TagInfo2() {Tag = 0xADF308F0, Key = Demokeys3Xx2, Code = 0x60}, // 3.XX demos 2
            new TagInfo2() {Tag = 0x8004FD03, Key = Ebootbin271New, Code = 0x5D}, // 2.71 eboot.bin
            new TagInfo2() {Tag = 0xD91605F0, Key = Ebootbin280New, Code = 0x5D}, // 2.80 eboot.bin
            new TagInfo2() {Tag = 0xD91606F0, Key = Ebootbin300New, Code = 0x5D}, // 3.00 eboot.bin
            new TagInfo2() {Tag = 0xD91608F0, Key = Ebootbin310New, Code = 0x5D}, // 3.10 eboot.bin
            new TagInfo2() {Tag = 0xD91609F0, Key = KeyD91609F0, Code = 0x5D}, // 5.00 eboot.bin
            new TagInfo2() {Tag = 0x2E5E10F0, Key = Key_2E5E10F0, Code = 0x48}, // 6.XX eboot.bin
            new TagInfo2() {Tag = 0x2E5E12F0, Key = Key_2E5E12F0, Code = 0x48}, // 6.XX eboot.bin
            new TagInfo2() {Tag = 0x2E5E12F0, Key = Key_2E5E13F0, Code = 0x48}, // 6.XX eboot.bin
            new TagInfo2() {Tag = 0xD9160AF0, Key = KeyD9160Af0, Code = 0x5D},
            new TagInfo2() {Tag = 0xD9160BF0, Key = KeyD9160Bf0, Code = 0x5D},
            new TagInfo2() {Tag = 0xD91611F0, Key = KeyD91611F0, Code = 0x5D},
            new TagInfo2() {Tag = 0xD91612F0, Key = KeyD91612F0, Code = 0x5D},
            new TagInfo2() {Tag = 0xD91613F0, Key = KeyD91613F0, Code = 0x5D},
            new TagInfo2() {Tag = 0x0A35EA03, Key = Gameshare260271, Code = 0x5E}, // 2.60-2.71 gameshare
            new TagInfo2() {Tag = 0x7B0505F0, Key = Gameshare280, Code = 0x5E}, // 2.80 gameshare
            new TagInfo2() {Tag = 0x7B0506F0, Key = Gameshare300, Code = 0x5E}, // 3.00 gameshare
            new TagInfo2() {Tag = 0x7B0508F0, Key = Gameshare310, Code = 0x5E}, // 3.10+ gameshare
            new TagInfo2() {Tag = 0x279D08F0, Key = Oneseg310, Code = 0x61}, // 3.10 1SEG
            new TagInfo2() {Tag = 0x279D06F0, Key = Oneseg300, Code = 0x61}, // 3.00 1SEG
            new TagInfo2() {Tag = 0x279D05F0, Key = Oneseg280, Code = 0x61}, // 2.80 1SEG
            new TagInfo2() {Tag = 0xD66DF703, Key = Oneseg260271, Code = 0x61}, // 2.60-2.71 1SEG
            new TagInfo2() {Tag = 0x279D10F0, Key = OnesegSlim, Code = 0x61}, // 02g 1SEG
            new TagInfo2() {Tag = 0x3C2A08F0, Key = MsAppMain, Code = 0x67}, // 1seg ms_application_main.prx
        };

        /* kernel modules 2.60-2.71 */
        public static readonly byte[] Keys2600 =
        {
            0xC3, 0x24, 0x89, 0xD3, 0x80, 0x87, 0xB2, 0x4E,
            0x4C, 0xD7, 0x49, 0xE4, 0x9D, 0x1D, 0x34, 0xD1
        };

        /* user modules 2.60-2.71 */
        public static readonly byte[] Keys2601 =
        {
            0xF3, 0xAC, 0x6E, 0x7C, 0x04, 0x0A, 0x23, 0xE7,
            0x0D, 0x33, 0xD8, 0x24, 0x73, 0x39, 0x2B, 0x4A
        };

        /* vshmain 2.60-2.71 */
        public static readonly byte[] Keys2602 =
        {
            0x72, 0xB4, 0x39, 0xFF, 0x34, 0x9B, 0xAE, 0x82,
            0x30, 0x34, 0x4A, 0x1D, 0xA2, 0xD8, 0xB4, 0x3C
        };

        /* kernel modules 2.80 */
        public static readonly byte[] Keys2800 =
        {
            0xCA, 0xFB, 0xBF, 0xC7, 0x50, 0xEA, 0xB4, 0x40,
            0x8E, 0x44, 0x5C, 0x63, 0x53, 0xCE, 0x80, 0xB1
        };

        /* user modules 2.80 */
        public static readonly byte[] Keys2801 =
        {
            0x40, 0x9B, 0xC6, 0x9B, 0xA9, 0xFB, 0x84, 0x7F,
            0x72, 0x21, 0xD2, 0x36, 0x96, 0x55, 0x09, 0x74
        };

        /* vshmain executable 2.80 */
        public static readonly byte[] Keys2802 =
        {
            0x03, 0xA7, 0xCC, 0x4A, 0x5B, 0x91, 0xC2, 0x07,
            0xFF, 0xFC, 0x26, 0x25, 0x1E, 0x42, 0x4B, 0xB5
        };

        /* kernel modules 3.00 */
        public static readonly byte[] Keys3000 =
        {
            0x9F, 0x67, 0x1A, 0x7A, 0x22, 0xF3, 0x59, 0x0B,
            0xAA, 0x6D, 0xA4, 0xC6, 0x8B, 0xD0, 0x03, 0x77
        };

        /* user modules 3.00 */
        public static readonly byte[] Keys3001 =
        {
            0x15, 0x07, 0x63, 0x26, 0xDB, 0xE2, 0x69, 0x34,
            0x56, 0x08, 0x2A, 0x93, 0x4E, 0x4B, 0x8A, 0xB2
        };

        /* vshmain 3.00 */
        public static readonly byte[] Keys3002 =
        {
            0x56, 0x3B, 0x69, 0xF7, 0x29, 0x88, 0x2F, 0x4C,
            0xDB, 0xD5, 0xDE, 0x80, 0xC6, 0x5C, 0xC8, 0x73
        };

        /* kernel modules 3.00 */
        public static readonly byte[] Keys3030 =
        {
            0x7b, 0xa1, 0xe2, 0x5a, 0x91, 0xb9, 0xd3, 0x13,
            0x77, 0x65, 0x4a, 0xb7, 0xc2, 0x8a, 0x10, 0xaf
        };

        /* kernel modules 3.10 */
        public static readonly byte[] Keys3100 =
        {
            0xa2, 0x41, 0xe8, 0x39, 0x66, 0x5b, 0xfa, 0xbb,
            0x1b, 0x2d, 0x6e, 0x0e, 0x33, 0xe5, 0xd7, 0x3f
        };

        /* user modules 3.10 */
        public static readonly byte[] Keys3101 =
        {
            0xA4, 0x60, 0x8F, 0xAB, 0xAB, 0xDE, 0xA5, 0x65,
            0x5D, 0x43, 0x3A, 0xD1, 0x5E, 0xC3, 0xFF, 0xEA
        };

        /* vshmain 3.10 */
        public static readonly byte[] Keys3102 =
        {
            0xE7, 0x5C, 0x85, 0x7A, 0x59, 0xB4, 0xE3, 0x1D,
            0xD0, 0x9E, 0xCE, 0xC2, 0xD6, 0xD4, 0xBD, 0x2B
        };

        /* reboot.bin 3.10 */
        public static readonly byte[] Keys3103 =
        {
            0x2E, 0x00, 0xF6, 0xF7, 0x52, 0xCF, 0x95, 0x5A,
            0xA1, 0x26, 0xB4, 0x84, 0x9B, 0x58, 0x76, 0x2F
        };

        /* kernel modules 3.30 */
        public static readonly byte[] Keys3300 =
        {
            0x3B, 0x9B, 0x1A, 0x56, 0x21, 0x80, 0x14, 0xED,
            0x8E, 0x8B, 0x08, 0x42, 0xFA, 0x2C, 0xDC, 0x3A
        };

        /* user modules 3.30 */
        public static readonly byte[] Keys3301 =
        {
            0xE8, 0xBE, 0x2F, 0x06, 0xB1, 0x05, 0x2A, 0xB9,
            0x18, 0x18, 0x03, 0xE3, 0xEB, 0x64, 0x7D, 0x26
        };

        /* vshmain 3.30 */
        public static readonly byte[] Keys3302 =
        {
            0xAB, 0x82, 0x25, 0xD7, 0x43, 0x6F, 0x6C, 0xC1,
            0x95, 0xC5, 0xF7, 0xF0, 0x63, 0x73, 0x3F, 0xE7
        };

        /* reboot.bin 3.30 */
        public static readonly byte[] Keys3303 =
        {
            0xA8, 0xB1, 0x47, 0x77, 0xDC, 0x49, 0x6A, 0x6F,
            0x38, 0x4C, 0x4D, 0x96, 0xBD, 0x49, 0xEC, 0x9B
        };

        /* stdio.prx 3.30 */
        public static readonly byte[] Keys3304 =
        {
            0xEC, 0x3B, 0xD2, 0xC0, 0xFA, 0xC1, 0xEE, 0xB9,
            0x9A, 0xBC, 0xFF, 0xA3, 0x89, 0xF2, 0x60, 0x1F
        };

        /* demo data.psp 2.80 */
        public static readonly byte[] Demokeys280 =
        {
            0x12, 0x99, 0x70, 0x5E, 0x24, 0x07, 0x6C, 0xD0,
            0x2D, 0x06, 0xFE, 0x7E, 0xB3, 0x0C, 0x11, 0x26
        };

        /* demo data.psp 3.XX */
        public static readonly byte[] Demokeys3Xx1 =
        {
            0x47, 0x05, 0xD5, 0xE3, 0x56, 0x1E, 0x81, 0x9B,
            0x09, 0x2F, 0x06, 0xDB, 0x6B, 0x12, 0x92, 0xE0
        };

        /* demo data.psp 3.XX */
        public static readonly byte[] Demokeys3Xx2 =
        {
            0xF6, 0x62, 0x39, 0x6E, 0x26, 0x22, 0x4D, 0xCA,
            0x02, 0x64, 0x16, 0x99, 0x7B, 0x9A, 0xE7, 0xB8
        };

        /* new 2.7X eboot.bin */
        public static readonly byte[] Ebootbin271New =
        {
            0xF4, 0xAE, 0xF4, 0xE1, 0x86, 0xDD, 0xD2, 0x9C,
            0x7C, 0xC5, 0x42, 0xA6, 0x95, 0xA0, 0x83, 0x88
        };

        /* new 2.8X eboot.bin */
        public static readonly byte[] Ebootbin280New =
        {
            0xB8, 0x8C, 0x45, 0x8B, 0xB6, 0xE7, 0x6E, 0xB8,
            0x51, 0x59, 0xA6, 0x53, 0x7C, 0x5E, 0x86, 0x31
        };

        /* new 3.XX eboot.bin */
        public static readonly byte[] Ebootbin300New =
        {
            0xED, 0x10, 0xE0, 0x36, 0xC4, 0xFE, 0x83, 0xF3,
            0x75, 0x70, 0x5E, 0xF6, 0xA4, 0x40, 0x05, 0xF7
        };

        /* new 3.XX eboot.bin */
        public static readonly byte[] Ebootbin310New =
        {
            0x5C, 0x77, 0x0C, 0xBB, 0xB4, 0xC2, 0x4F, 0xA2,
            0x7E, 0x3B, 0x4E, 0xB4, 0xB4, 0xC8, 0x70, 0xAF
        };

        /* 2.60-2.71 gameshare */
        public static readonly byte[] Gameshare260271 =
        {
            0xF9, 0x48, 0x38, 0x0C, 0x96, 0x88, 0xA7, 0x74,
            0x4F, 0x65, 0xA0, 0x54, 0xC2, 0x76, 0xD9, 0xB8
        };

        /* 2.80 gameshare */
        public static readonly byte[] Gameshare280 =
        {
            0x2D, 0x86, 0x77, 0x3A, 0x56, 0xA4, 0x4F, 0xDD,
            0x3C, 0x16, 0x71, 0x93, 0xAA, 0x8E, 0x11, 0x43
        };

        /* 3.00 gameshare */
        public static readonly byte[] Gameshare300 =
        {
            0x78, 0x1A, 0xD2, 0x87, 0x24, 0xBD, 0xA2, 0x96,
            0x18, 0x3F, 0x89, 0x36, 0x72, 0x90, 0x92, 0x85
        };

        /* 3.10 gameshare */
        public static readonly byte[] Gameshare310 =
        {
            0xC9, 0x7D, 0x3E, 0x0A, 0x54, 0x81, 0x6E, 0xC7,
            0x13, 0x74, 0x99, 0x74, 0x62, 0x18, 0xE7, 0xDD
        };

        /* 3.60 common kernel modules */
        public static readonly byte[] Keys3600 =
        {
            0x3C, 0x2B, 0x51, 0xD4, 0x2D, 0x85, 0x47, 0xDA,
            0x2D, 0xCA, 0x18, 0xDF, 0xFE, 0x54, 0x09, 0xED
        };

        /* 3.60 specific slim kernel modules */
        public static readonly byte[] Keys3601 =
        {
            0x31, 0x1F, 0x98, 0xD5, 0x7B, 0x58, 0x95, 0x45,
            0x32, 0xAB, 0x3A, 0xE3, 0x89, 0x32, 0x4B, 0x34
        };

        /* 3.70 common and fat kernel modules */
        public static readonly byte[] Keys3700 =
        {
            0x26, 0x38, 0x0A, 0xAC, 0xA5, 0xD8, 0x74, 0xD1,
            0x32, 0xB7, 0x2A, 0xBF, 0x79, 0x9E, 0x6D, 0xDB
        };

        /* 3.70 slim specific kernel modules */
        public static readonly byte[] Keys3701 =
        {
            0x53, 0xE7, 0xAB, 0xB9, 0xC6, 0x4A, 0x4B, 0x77,
            0x92, 0x17, 0xB5, 0x74, 0x0A, 0xDA, 0xA9, 0xEA
        };

        /* some 3.70 slim user modules */
        public static readonly byte[] Keys3702 =
        {
            0x71, 0x10, 0xF0, 0xA4, 0x16, 0x14, 0xD5, 0x93,
            0x12, 0xFF, 0x74, 0x96, 0xDF, 0x1F, 0xDA, 0x89
        };

        /* 1SEG.PBP keys */
        public static readonly byte[] Oneseg310 =
        {
            0xC7, 0x27, 0x72, 0x85, 0xAB, 0xA7, 0xF7, 0xF0,
            0x4C, 0xC1, 0x86, 0xCC, 0xE3, 0x7F, 0x17, 0xCA
        };

        public static readonly byte[] Oneseg300 =
        {
            0x76, 0x40, 0x9E, 0x08, 0xDB, 0x9B, 0x3B, 0xA1,
            0x47, 0x8A, 0x96, 0x8E, 0xF3, 0xF7, 0x62, 0x92
        };

        public static readonly byte[] Oneseg280 =
        {
            0x23, 0xDC, 0x3B, 0xB5, 0xA9, 0x82, 0xD6, 0xEA,
            0x63, 0xA3, 0x6E, 0x2B, 0x2B, 0xE9, 0xE1, 0x54
        };

        public static readonly byte[] Oneseg260271 =
        {
            0x22, 0x43, 0x57, 0x68, 0x2F, 0x41, 0xCE, 0x65,
            0x4C, 0xA3, 0x7C, 0xC6, 0xC4, 0xAC, 0xF3, 0x60
        };

        public static readonly byte[] OnesegSlim =
        {
            0x12, 0x57, 0x0D, 0x8A, 0x16, 0x6D, 0x87, 0x06,
            0x03, 0x7D, 0xC8, 0x8B, 0x62, 0xA3, 0x32, 0xA9
        };

        public static readonly byte[] MsAppMain =
        {
            0x1E, 0x2E, 0x38, 0x49, 0xDA, 0xD4, 0x16, 0x08,
            0x27, 0x2E, 0xF3, 0xBC, 0x37, 0x75, 0x80, 0x93
        };

        /* 3.90 kernel */
        public static readonly byte[] Keys3900 =
        {
            0x45, 0xEF, 0x5C, 0x5D, 0xED, 0x81, 0x99, 0x84,
            0x12, 0x94, 0x8F, 0xAB, 0xE8, 0x05, 0x6D, 0x7D
        };

        /* 3.90 slim */
        public static readonly byte[] Keys3901 =
        {
            0x70, 0x1B, 0x08, 0x25, 0x22, 0xA1, 0x4D, 0x3B,
            0x69, 0x21, 0xF9, 0x71, 0x0A, 0xA8, 0x41, 0xA9
        };

        /* 5.00 kernel */
        public static readonly byte[] Keys5000 =
        {
            0xEB, 0x1B, 0x53, 0x0B, 0x62, 0x49, 0x32, 0x58,
            0x1F, 0x83, 0x0A, 0xF4, 0x99, 0x3D, 0x75, 0xD0
        };

        /* 5.00 kernel 2000 specific */
        public static readonly byte[] Keys5001 =
        {
            0xBA, 0xE2, 0xA3, 0x12, 0x07, 0xFF, 0x04, 0x1B,
            0x64, 0xA5, 0x11, 0x85, 0xF7, 0x2F, 0x99, 0x5B
        };

        /* 5.00 kernel 3000 specific */
        public static readonly byte[] Keys5002 =
        {
            0x2C, 0x8E, 0xAF, 0x1D, 0xFF, 0x79, 0x73, 0x1A,
            0xAD, 0x96, 0xAB, 0x09, 0xEA, 0x35, 0x59, 0x8B
        };

        public static readonly byte[] Keys500C =
        {
            0xA3, 0x5D, 0x51, 0xE6, 0x56, 0xC8, 0x01, 0xCA,
            0xE3, 0x77, 0xBF, 0xCD, 0xFF, 0x24, 0xDA, 0x4D
        };

        /* 5.05 kernel specific */
        public static readonly byte[] Keys505A =
        {
            0x7B, 0x94, 0x72, 0x27, 0x4C, 0xCC, 0x54, 0x3B,
            0xAE, 0xDF, 0x46, 0x37, 0xAC, 0x01, 0x4D, 0x87
        };

        public static readonly byte[] Keys5050 =
        {
            0x2E, 0x8E, 0x97, 0xA2, 0x85, 0x42, 0x70, 0x73,
            0x18, 0xDA, 0xA0, 0x8A, 0xF8, 0x62, 0xA2, 0xB0
        };

        public static readonly byte[] Keys5051 =
        {
            0x58, 0x2A, 0x4C, 0x69, 0x19, 0x7B, 0x83, 0x3D,
            0xD2, 0x61, 0x61, 0xFE, 0x14, 0xEE, 0xAA, 0x11
        };

        /* for psp 2000 file table and ipl pre-decryption */
        public static readonly byte[] Keys02Ge =
        {
            0x9D, 0x09, 0xFD, 0x20, 0xF3, 0x8F, 0x10, 0x69,
            0x0D, 0xB2, 0x6F, 0x00, 0xCC, 0xC5, 0x51, 0x2E
        };

        /* for psp 3000 file table and ipl pre-decryption */
        public static readonly byte[] Keys03Ge =
        {
            0x4F, 0x44, 0x5C, 0x62, 0xB3, 0x53, 0xC4, 0x30,
            0xFC, 0x3A, 0xA4, 0x5B, 0xEC, 0xFE, 0x51, 0xEA
        };

        public static readonly byte[] KeyD91609F0 =
        {
            0xD0, 0x36, 0x12, 0x75, 0x80, 0x56, 0x20, 0x43,
            0xC4, 0x30, 0x94, 0x3E, 0x1C, 0x75, 0xD1, 0xBF
        };

        public static readonly byte[] KeyD9160Af0 =
        {
            0x10, 0xA9, 0xAC, 0x16, 0xAE, 0x19, 0xC0, 0x7E,
            0x3B, 0x60, 0x77, 0x86, 0x01, 0x6F, 0xF2, 0x63
        };

        public static readonly byte[] KeyD9160Bf0 =
        {
            0x83, 0x83, 0xF1, 0x37, 0x53, 0xD0, 0xBE, 0xFC,
            0x8D, 0xA7, 0x32, 0x52, 0x46, 0x0A, 0xC2, 0xC2
        };

        public static readonly byte[] KeyD91611F0 =
        {
            0x61, 0xB0, 0xC0, 0x58, 0x71, 0x57, 0xD9, 0xFA,
            0x74, 0x67, 0x0E, 0x5C, 0x7E, 0x6E, 0x95, 0xB9
        };

        public static readonly byte[] KeyD91612F0 =
        {
/* UMD EBOOT.BIN (OPNSSMP.BIN) */
            0x9E, 0x20, 0xE1, 0xCD, 0xD7, 0x88, 0xDE, 0xC0,
            0x31, 0x9B, 0x10, 0xAF, 0xC5, 0xB8, 0x73, 0x23
        };

        public static readonly byte[] KeyD91613F0 =
        {
            0xEB, 0xFF, 0x40, 0xD8, 0xB4, 0x1A, 0xE1, 0x66,
            0x91, 0x3B, 0x8F, 0x64, 0xB6, 0xFC, 0xB7, 0x12
        };

        public static readonly byte[] Key_2E5E10F0 =
        {
/* UMD EBOOT.BIN 2 (OPNSSMP.BIN) */
            0x9D, 0x5C, 0x5B, 0xAF, 0x8C, 0xD8, 0x69, 0x7E,
            0x51, 0x9F, 0x70, 0x96, 0xE6, 0xD5, 0xC4, 0xE8
        };

        public static readonly byte[] Key_2E5E12F0 =
        {
/* UMD EBOOT.BIN 3 (OPNSSMP.BIN) */
            0x8A, 0x7B, 0xC9, 0xD6, 0x52, 0x58, 0x88, 0xEA,
            0x51, 0x83, 0x60, 0xCA, 0x16, 0x79, 0xE2, 0x07
        };

        public static readonly byte[] Key_2E5E13F0 =
        {
            0xFF, 0xA4, 0x68, 0xC3, 0x31, 0xCA, 0xB7, 0x4C,
            0xF1, 0x23, 0xFF, 0x01, 0x65, 0x3D, 0x26, 0x36
        };

        public static readonly byte[] Keys600U1457B0Bf0 =
        {
            0x7B, 0x94, 0x72, 0x27, 0x4C, 0xCC, 0x54, 0x3B,
            0xAE, 0xDF, 0x46, 0x37, 0xAC, 0x01, 0x4D, 0x87
        };

        public static readonly byte[] Keys600U1457B0Cf0 =
        {
            0xAC, 0x34, 0xBA, 0xB1, 0x97, 0x8D, 0xAE, 0x6F,
            0xBA, 0xE8, 0xB1, 0xD6, 0xDF, 0xDF, 0xF1, 0xA2
        };

        /* for psp go file table and ipl pre-decryption */
        public static readonly byte[] Keys05Ge =
        {
            0x5D, 0xAA, 0x72, 0xF2, 0x26, 0x60, 0x4D, 0x1C,
            0xE7, 0x2D, 0xC8, 0xA3, 0x2F, 0x79, 0xC5, 0x54
        };

        /* 5.70 PSPgo kernel*/
        public static readonly byte[] Keys570_5K =
        {
            0x6D, 0x72, 0xA4, 0xBA, 0x7F, 0xBF, 0xD1, 0xF1,
            0xA9, 0xF3, 0xBB, 0x07, 0x1B, 0xC0, 0xB3, 0x66
        };

        /* 6.00-6.20 kernel and phat */
        public static readonly byte[] Keys6200 =
        {
            0xD6, 0xBD, 0xCE, 0x1E, 0x12, 0xAF, 0x9A, 0xE6,
            0x69, 0x30, 0xDE, 0xDA, 0x88, 0xB8, 0xFF, 0xFB
        };

        /* 6.00-6.20 slim kernel */
        public static readonly byte[] Keys6201 =
        {
            0x1D, 0x13, 0xE9, 0x50, 0x04, 0x73, 0x3D, 0xD2,
            0xE1, 0xDA, 0xB9, 0xC1, 0xE6, 0x7B, 0x25, 0xA7
        };

        public static readonly byte[] Keys6203 =
        {
            0xA3, 0x5D, 0x51, 0xE6, 0x56, 0xC8, 0x01, 0xCA,
            0xE3, 0x77, 0xBF, 0xCD, 0xFF, 0x24, 0xDA, 0x4D
        };

        public static readonly byte[] Keys620E =
        {
            0xB1, 0xB3, 0x7F, 0x76, 0xC3, 0xFB, 0x88, 0xE6,
            0xF8, 0x60, 0xD3, 0x35, 0x3C, 0xA3, 0x4E, 0xF3
        };

        /* PSPgo internal */
        public static readonly byte[] Keys6205 =
        {
            0xF1, 0xBC, 0x17, 0x07, 0xAE, 0xB7, 0xC8, 0x30,
            0xD8, 0x34, 0x9D, 0x40, 0x6A, 0x8E, 0xDF, 0x4E
        };

        /* 6.XX PSPgo kernel */
        public static readonly byte[] Keys620_5K =
        {
            0x41, 0x8A, 0x35, 0x4F, 0x69, 0x3A, 0xDF, 0x04,
            0xFD, 0x39, 0x46, 0xA2, 0x5C, 0x2D, 0xF2, 0x21
        };

        public static readonly byte[] Keys620_5V =
        {
            0xF2, 0x8F, 0x75, 0xA7, 0x31, 0x91, 0xCE, 0x9E,
            0x75, 0xBD, 0x27, 0x26, 0xB4, 0xB4, 0x0C, 0x32
        };
    }
}