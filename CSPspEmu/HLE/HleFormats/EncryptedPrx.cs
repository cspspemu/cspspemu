using System;
using System.IO;
using System.Linq;
using CSharpUtils;
using CSPspEmu.Utils;
using Kirk = CSPspEmu.Core.Components.Crypto.Kirk;

namespace CSPspEmu.Hle.Formats
{
    public unsafe partial class EncryptedPrx
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
    }
}