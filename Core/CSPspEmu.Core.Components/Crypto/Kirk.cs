//#define USE_DOTNET_CRYPTO

using System;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Components.Crypto
{
    public unsafe delegate void PointerAction(byte* address);

    public unsafe partial class Kirk
    {
        static Logger Logger = Logger.GetLogger("Kirk");

        //small struct for temporary keeping AES & CMAC key from CMD1 header
        struct HeaderKeys
        {
#pragma warning disable 649
            public fixed byte Aes[16];
            public fixed byte Cmac[16];
#pragma warning restore 649
        }

        byte[] fuseID = new byte[16]; // Emulate FUSEID	

//#if !USE_DOTNET_CRYPTO
        Crypto.AesCtx _aesKirk1; //global
//#endif

        Random _random;

        bool _isKirkInitialized; //"init" emulation

        /// <summary>
        /// 
        /// </summary>
        public Kirk()
        {
            kirk_init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outbuff"></param>
        /// <param name="inbuff"></param>
        /// <param name="size"></param>
        /// <param name="generateTrash"></param>
        /// <returns></returns>
        public void kirk_CMD0(byte* outbuff, byte* inbuff, int size, bool generateTrash)
        {
            var cmacHeaderHashBytes = new byte[16];
            var cmacDataHashBytes = new byte[16];

            fixed (byte* cmacHeaderHash = cmacHeaderHashBytes)
            fixed (byte* cmacDataHash = cmacDataHashBytes)
//#if !USE_DOTNET_CRYPTO
            fixed (Crypto.AesCtx* aesKirk1Ptr = &_aesKirk1)
//#endif
            {
                check_initialized();

                Aes128CmacHeader* header = (Aes128CmacHeader*) outbuff;

                Crypto.Memcpy(outbuff, inbuff, size);

                if (header->Mode != KirkMode.Cmd1)
                {
                    throw(new KirkException(ResultEnum.PspKirkInvalidMode,
                        $"Expected mode Cmd1 but found {header->Mode}"));
                }

                HeaderKeys* keys = (HeaderKeys*) outbuff; //0-15 AES key, 16-31 CMAC key

                //FILL PREDATA WITH RANDOM DATA
                if (generateTrash) kirk_CMD14(outbuff + sizeof(Aes128CmacHeader), header->DataOffset);

                //Make sure data is 16 aligned
                var chkSize = header->DataSize;
                if ((chkSize % 16) != 0) chkSize += 16 - (chkSize % 16);

                //ENCRYPT DATA
                Crypto.AesCtx k1;
                Crypto.AES_set_key(&k1, keys->Aes, 128);

                Crypto.AES_cbc_encrypt(&k1, inbuff + sizeof(Aes128CmacHeader) + header->DataOffset,
                    outbuff + sizeof(Aes128CmacHeader) + header->DataOffset, chkSize);

                //CMAC HASHES
                Crypto.AesCtx cmacKey;
                Crypto.AES_set_key(&cmacKey, keys->Cmac, 128);

                Crypto.AES_CMAC(&cmacKey, outbuff + 0x60, 0x30, cmacHeaderHash);

                Crypto.AES_CMAC(&cmacKey, outbuff + 0x60, 0x30 + chkSize + header->DataOffset, cmacDataHash);

                Crypto.Memcpy(header->CmacHeaderHash, cmacHeaderHash, 16);
                Crypto.Memcpy(header->CmacDataHash, cmacDataHash, 16);

                //ENCRYPT KEYS
                Crypto.AES_cbc_encrypt(aesKirk1Ptr, inbuff, outbuff, 16 * 2);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void check_initialized()
        {
            if (!_isKirkInitialized) throw new KirkException(ResultEnum.PspKirkNotInit, "Not initialized");
        }

        /// <summary>
        /// Cypher-Block Chaining decoding.
        /// Master decryption command, used by firmware modules. Applies CMAC checking.
        /// </summary>
        /// <param name="outbuff"></param>
        /// <param name="inbuff"></param>
        /// <param name="size"></param>
        /// <param name="doCheck"></param>
        /// <returns></returns>
        public void kirk_CMD1(byte* outbuff, byte* inbuff, int size, bool doCheck = true)
        {
            fixed (Crypto.AesCtx* aesKirk1Ptr = &_aesKirk1)
            {
                check_initialized();
                var header = *(Aes128CmacHeader*) inbuff;
                if (header.Mode != KirkMode.Cmd1)
                {
                    //Console.Error.WriteLine("ResultEnum.PSP_KIRK_INVALID_MODE");
                    Console.Error.WriteLine("{0}", header.ToStringDefault(true));
                    Console.WriteLine("Input:");
                    ArrayUtils.HexDump(PointerUtils.PointerToByteArray(inbuff, 0x100));
                    Console.WriteLine("Output:");
                    ArrayUtils.HexDump(PointerUtils.PointerToByteArray(outbuff, 0x100));
                    throw (new KirkException(ResultEnum.PspKirkInvalidMode,
                        $"Expected mode Cmd1 but found {header.Mode}"));
                }

                Console.WriteLine("Input:");
                ArrayUtils.HexDump(PointerUtils.PointerToByteArray(inbuff, 0x100));

                //Console.WriteLine("header.DataOffset = 0x{0:X8}", header.DataOffset);
                //Console.WriteLine("header.DataSize = 0x{0:X8}", header.DataSize);

                // Decrypts AES and CMAC keys.

                HeaderKeys keys; //0-15 AES key, 16-31 CMAC key

#if USE_DOTNET_CRYPTO
				DecryptAes(kirk1_key, inbuff, (byte*)&keys, 16 * 2); //decrypt AES & CMAC key to temp buffer
#else
                Crypto.AES_cbc_decrypt(aesKirk1Ptr, inbuff, (byte*) &keys, 16 * 2);
#endif

                // HOAX WARRING! I have no idea why the hash check on last IPL block fails, so there is an option to disable checking
                if (doCheck)
                {
                    kirk_CMD10(inbuff, size);
                }

                //var AES = new RijndaelManaged();

#if USE_DOTNET_CRYPTO
				DecryptAes(
					PointerUtils.PointerToByteArray(keys.AES, 16),
					inbuff + sizeof(AES128CMACHeader) + header->DataOffset,
					outbuff,
					header->DataSize
				);
#else
                Crypto.AesCtx k1;
                Crypto.AES_set_key(&k1, keys.Aes, 128);

                Crypto.AES_cbc_decrypt(
                    ctx: &k1,
                    src: inbuff + sizeof(Aes128CmacHeader) + header.DataOffset,
                    dst: outbuff,
                    size: header.DataSize
                );
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outbuff"></param>
        /// <param name="inbuff"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public void kirk_CMD4(byte* outbuff, byte* inbuff, int size)
        {
            check_initialized();

            KirkAes128CbcHeader* header = (KirkAes128CbcHeader*) inbuff;
            if (header->Mode != KirkMode.EncryptCbc)
            {
                throw (new KirkException(ResultEnum.PspKirkInvalidMode));
            }
            if (header->Datasize == 0)
            {
                throw (new KirkException(ResultEnum.PspKirkDataSizeIsZero));
            }

            kirk_4_7_get_key(header->KeySeed, (key) =>
            {
                // Set the key
                Crypto.AesCtx aesKey;
                Crypto.AES_set_key(&aesKey, key, 128);
                Crypto.AES_cbc_encrypt(&aesKey, inbuff + sizeof(KirkAes128CbcHeader), outbuff, size);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outbuff"></param>
        /// <param name="inbuff"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public void kirk_CMD7(byte* outbuff, byte* inbuff, int size)
        {
            check_initialized();

            var header = (KirkAes128CbcHeader*) inbuff;
            if (header->Mode != KirkMode.DecryptCbc)
            {
                throw (new KirkException(ResultEnum.PspKirkInvalidMode));
            }
            if (header->Datasize == 0)
            {
                throw (new KirkException(ResultEnum.PspKirkDataSizeIsZero));
            }

#if USE_DOTNET_CRYPTO
			var Output = DecryptAes(
				PointerUtils.PointerToByteArray(inbuff + sizeof(KIRK_AES128CBC_HEADER), size),
				_kirk_4_7_get_key(Header->KeySeed)
			);

			PointerUtils.ByteArrayToPointer(Output, outbuff);
#else
            kirk_4_7_get_key(header->KeySeed, (key) =>
            {
                //Set the key
                Crypto.AesCtx aesKey;
                Crypto.AES_set_key(&aesKey, key, 128);

                Crypto.AES_cbc_decrypt(&aesKey, inbuff + sizeof(KirkAes128CbcHeader), outbuff, size);
            });
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inbuff"></param>
        /// <param name="insize"></param>
        /// <returns></returns>
        public void kirk_CMD10(byte* inbuff, int insize)
        {
            var cmacHeaderHashBytes = new byte[16];
            var cmacDataHashBytes = new byte[16];
            fixed (byte* cmacHeaderHash = cmacHeaderHashBytes)
            fixed (byte* cmacDataHash = cmacDataHashBytes)
#if !USE_DOTNET_CRYPTO
            fixed (Crypto.AesCtx* aesKirk1Ptr = &_aesKirk1)
#endif
            {
                check_initialized();

                Aes128CmacHeader header = *(Aes128CmacHeader*) inbuff;

                if (!(header.Mode == KirkMode.Cmd1 || header.Mode == KirkMode.Cmd2 || header.Mode == KirkMode.Cmd3))
                {
                    throw(new KirkException(ResultEnum.PspKirkInvalidMode));
                }

                if (header.DataSize == 0)
                {
                    throw (new KirkException(ResultEnum.PspKirkDataSizeIsZero));
                }

                if (header.Mode != KirkMode.Cmd1)
                {
                    // Checks for cmd 2 & 3 not included right now
                    throw (new KirkException(ResultEnum.PspKirkInvalidSigCheck));
                }

                HeaderKeys keys; //0-15 AES key, 16-31 CMAC key

#if USE_DOTNET_CRYPTO
				DecryptAes(kirk1_key, inbuff, (byte *)&keys, 16 * 2);
#else
                Crypto.AES_cbc_decrypt(aesKirk1Ptr, inbuff, (byte*) &keys,
                    32); //decrypt AES & CMAC key to temp buffer
#endif

                Crypto.AesCtx cmacKey;
                Crypto.AES_set_key(&cmacKey, keys.Cmac, 128);

                Crypto.AES_CMAC(&cmacKey, inbuff + 0x60, 0x30, cmacHeaderHash);

                //Make sure data is 16 aligned
                var chkSize = header.DataSize;
                if ((chkSize % 16) != 0) chkSize += 16 - (chkSize % 16);
                Crypto.AES_CMAC(&cmacKey, inbuff + 0x60, 0x30 + chkSize + header.DataOffset, cmacDataHash);

                if (Crypto.Memcmp(cmacHeaderHash, header.CmacHeaderHash, 16) != 0)
                {
                    Logger.Error("header hash invalid");
                    throw (new KirkException(ResultEnum.PspSubcwrHeaderHashInvalid));
                }
                if (Crypto.Memcmp(cmacDataHash, header.CmacDataHash, 16) != 0)
                {
                    Logger.Error("data hash invalid");
                    throw (new KirkException(ResultEnum.PspSubcwrHeaderHashInvalid));
                }
            }
        }

        /// <summary>
        /// Generate Random Data
        /// </summary>
        /// <param name="output"></param>
        /// <param name="outputSize"></param>
        /// <returns></returns>
        public void kirk_CMD14(byte* output, int outputSize)
        {
            check_initialized();
            for (var i = 0; i < outputSize; i++) output[i] = (byte) (_random.Next() & 0xFF);
        }

        /// <summary>
        /// Initializes kirk
        /// </summary>
        /// <returns></returns>
        public void kirk_init()
        {
            fixed (byte* kirk1KeyPtr = Kirk1Key)
            fixed (Crypto.AesCtx* aesKirk1Ptr = &_aesKirk1)
            {
                Crypto.AES_set_key(aesKirk1Ptr, kirk1KeyPtr, 128);
            }
            _isKirkInitialized = true;
            _random = new Random();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyType"></param>
        /// <returns></returns>
        public byte[] _kirk_4_7_get_key(int keyType)
        {
            switch (keyType)
            {
                case 0x03: return Kirk7Key03;
                case 0x04: return Kirk7Key04;
                case 0x05: return Kirk7Key05;
                case 0x0C: return Kirk7Key0C;
                case 0x0D: return Kirk7Key0D;
                case 0x0E: return Kirk7Key0E;
                case 0x0F: return Kirk7Key0F;
                case 0x10: return Kirk7Key10;
                case 0x11: return Kirk7Key11;
                case 0x12: return Kirk7Key12;
                case 0x38: return Kirk7Key38;
                case 0x39: return Kirk7Key39;
                case 0x3A: return Kirk7Key3A;
                case 0x4B: return Kirk7Key4B;
                case 0x53: return Kirk7Key53;
                case 0x57: return Kirk7Key57;
                case 0x5D: return Kirk7Key5D;
                case 0x63: return Kirk7Key63;
                case 0x64: return Kirk7Key64;
                default: throw new NotImplementedException($"Invalid Key Type: 0x{keyType:X}");
                //throw (new KirkException(KIRK_INVALID_SIZE));
                //default: return (byte*)KIRK_INVALID_SIZE; break; //need to get the real error code for that, placeholder now :)
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="keyType"></param>
        /// <param name="pointerAction"></param>
        /// <returns></returns>
        public void kirk_4_7_get_key(int keyType, PointerAction pointerAction)
        {
            fixed (byte* key = _kirk_4_7_get_key(keyType))
            {
                pointerAction(key);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outbuff"></param>
        /// <param name="inbuff"></param>
        /// <param name="size"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public void kirk_CMD1_ex(byte* outbuff, byte* inbuff, int size, Aes128CmacHeader* header)
        {
            var bufferBytes = new byte[size];
            fixed (byte* buffer = bufferBytes)
            {
                Crypto.Memcpy(buffer, header, sizeof(Aes128CmacHeader));
                Crypto.Memcpy(buffer + sizeof(Aes128CmacHeader), inbuff, header->DataSize);
                kirk_CMD1(outbuff, buffer, size);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fuse"></param>
        /// <returns></returns>
        public int SceUtilsSetFuseId(void* fuse)
        {
            //PointerUtils.Memcpy(new ArraySegment<byte>(fuseID, 0, 16), (byte*)fuse);
            PointerUtils.Memcpy(fuseID, (byte*) fuse, 16);
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="inbuff"></param>
        /// <returns></returns>
        public int kirk_decrypt_keys(byte* keys, byte* inbuff)
        {
            fixed (Crypto.AesCtx* aesKirk1Ptr = &_aesKirk1)
            {
                Crypto.AES_cbc_decrypt(aesKirk1Ptr, inbuff, keys, 16 * 2); //decrypt AES & CMAC key to temp buffer
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inbuff"></param>
        /// <param name="insize"></param>
        /// <returns></returns>
        public void kirk_forge(byte* inbuff, int insize)
        {
            var header = (Aes128CmacHeader*) inbuff;
            var cmacHeaderHashBytes = new byte[16];
            var cmacDataHashBytes = new byte[16];

            fixed (byte* cmacHeaderHash = cmacHeaderHashBytes)
            fixed (byte* cmacDataHash = cmacDataHashBytes)
            fixed (Crypto.AesCtx* aesKirk1Ptr = &_aesKirk1)
            {
                check_initialized();
                if (!(header->Mode == KirkMode.Cmd1 || header->Mode == KirkMode.Cmd2 || header->Mode == KirkMode.Cmd3))
                {
                    throw (new KirkException(ResultEnum.PspKirkInvalidMode));
                }
                if (header->DataSize == 0)
                {
                    throw (new KirkException(ResultEnum.PspKirkDataSizeIsZero));
                }

                if (header->Mode != KirkMode.Cmd1)
                {
                    // Checks for cmd 2 & 3 not included right now
                    //throw(new KirkException(ResultEnum.PSP_KIRK_INVALID_MODE));
                    throw(new KirkException(ResultEnum.PspKirkInvalidSigCheck));
                }

                HeaderKeys keys; //0-15 AES key, 16-31 CMAC key

                Crypto.AES_cbc_decrypt(aesKirk1Ptr, inbuff, (byte*) &keys,
                    32); //decrypt AES & CMAC key to temp buffer
                Crypto.AesCtx cmacKey;
                Crypto.AES_set_key(&cmacKey, keys.Cmac, 128);
                Crypto.AES_CMAC(&cmacKey, inbuff + 0x60, 0x30, cmacHeaderHash);
                if (Crypto.Memcmp(cmacHeaderHash, header->CmacHeaderHash, 16) != 0)
                {
                    throw (new KirkException(ResultEnum.PspKirkInvalidHeaderHash));
                }

                //Make sure data is 16 aligned
                var chkSize = header->DataSize;
                if ((chkSize % 16) != 0) chkSize += 16 - (chkSize % 16);
                Crypto.AES_CMAC(&cmacKey, inbuff + 0x60, 0x30 + chkSize + header->DataOffset, cmacDataHash);

                if (Crypto.Memcmp(cmacDataHash, header->CmacDataHash, 16) != 0)
                {
                    //printf("data hash invalid, correcting...\n");
                }
                else
                {
                    Logger.Error("data hash is already valid!");
                    throw(new NotImplementedException());
                    //return 100;
                }
                // Forge collision for data hash
                Crypto.Memcpy(cmacDataHash, header->CmacDataHash, 0x10);
                Crypto.AES_CMAC_forge(&cmacKey, inbuff + 0x60, 0x30 + chkSize + header->DataOffset, cmacDataHash);
                //printf("Last row in bad file should be :\n"); for(i=0;i<0x10;i++) printf("%02x", cmac_data_hash[i]);
                //printf("\n\n");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Out"></param>
        /// <param name="In"></param>
        /// <param name="insize"></param>
        /// <returns></returns>
        public void kirk_CMD5(byte* Out, byte* In, int insize)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Out"></param>
        /// <param name="In"></param>
        /// <param name="insize"></param>
        /// <returns></returns>
        public void ExecuteKirkCmd8(byte* Out, byte* In, int insize)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Out"></param>
        /// <param name="outSize"></param>
        /// <param name="In"></param>
        /// <param name="inSize"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public int SceUtilsBufferCopyWithRange(byte* Out, int outSize, byte* In, int inSize, int command)
        {
            return (int) HleUtilsBufferCopyWithRange(Out, outSize, In, inSize, (CommandEnum) command);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Out"></param>
        /// <param name="outSize"></param>
        /// <param name="In"></param>
        /// <param name="inSize"></param>
        /// <param name="command"></param>
        /// <param name="doChecks"></param>
        /// <returns></returns>
        public ResultEnum HleUtilsBufferCopyWithRange(byte* Out, int outSize, byte* In, int inSize, CommandEnum command,
            bool doChecks = true)
        {
            try
            {
                switch (command)
                {
                    case CommandEnum.PspKirkCmdDecryptPrivate:
                        kirk_CMD1(Out, In, inSize, doChecks);
                        break;
                    case CommandEnum.PspKirkCmdEncrypt:
                        kirk_CMD4(Out, In, inSize);
                        break;
                    case CommandEnum.PspKirkCmdEncryptFuse:
                        kirk_CMD5(Out, In, inSize);
                        break;
                    case CommandEnum.PspKirkCmdDecrypt:
                        kirk_CMD7(Out, In, inSize);
                        break;
                    case CommandEnum.PspKirkCmdDecryptFuse:
                        ExecuteKirkCmd8(Out, In, inSize);
                        break;
                    case CommandEnum.PspKirkCmdPrivSigCheck:
                        kirk_CMD10(In, inSize);
                        break;
                    case CommandEnum.PspKirkCmdSha1Hash:
                        KirkSha1(Out, In, inSize);
                        break;
                    case CommandEnum.PspKirkCmdEcdsaGenKeys:
                        ExecuteKirkCmd12(Out, outSize);
                        break;
                    case CommandEnum.PspKirkCmdEcdsaMultiplyPoint:
                        ExecuteKirkCmd13(Out, outSize, In, inSize);
                        break;
                    case CommandEnum.PspKirkCmdPrng:
                        kirk_CMD14(Out, inSize);
                        break;
                    case CommandEnum.PspKirkCmdEcdsaSign:
                        ExecuteKirkCmd16(Out, outSize, In, inSize);
                        break;
                    case CommandEnum.PspKirkCmdEcdsaVerify:
                        ExecuteKirkCmd17(In, inSize);
                        break;
                    default: throw(new KirkException(ResultEnum.PspKirkInvalidOperation));
                }

                return ResultEnum.Ok;
            }
            catch (KirkException kirkException)
            {
                return kirkException.Result;
            }
        }
    }
}