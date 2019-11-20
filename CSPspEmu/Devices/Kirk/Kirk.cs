//#define USE_DOTNET_CRYPTO

using System;
using System.IO;
using System.Security.Cryptography;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Components.Crypto
{
    public unsafe delegate void PointerAction(byte* address);

    public unsafe class Kirk
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] DecryptAes(byte[] input, byte[] key, byte[] iv = null)
        {
            if (iv == null) iv = new byte[16];

            Logger.Notice("DecryptAes({0}, {1}, {2})", input.Length, key.Length, iv.Length);

            using (var aes = CreateAes())
            {
                aes.Padding = PaddingMode.Zeros;
                var decryptor = aes.CreateDecryptor(key, iv);

                var dataSize = input.Length;

                if ((dataSize % 16) != 0)
                {
                    var input2 = new byte[MathUtils.NextAligned(input.Length, 16)];
                    Array.Copy(input, input2, input.Length);
                    input = input2;
                }

                return new CryptoStream(new MemoryStream(input), decryptor, CryptoStreamMode.Read).ReadBytes(dataSize);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] EncryptAes(byte[] input, byte[] key, byte[] iv = null)
        {
            if (iv == null) iv = new byte[16];

            using (var aes = CreateAes())
            {
                aes.Padding = PaddingMode.Zeros;
                var encryptor = aes.CreateEncryptor(key, iv);

                return new CryptoStream(new MemoryStream(input), encryptor, CryptoStreamMode.Read).ReadAll(
                    dispose: true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="size"></param>
        public static void DecryptAes(byte[] key, byte* input, byte* output, int size)
        {
            var inputArray = PointerUtils.PointerToByteArray(input, size);
            var outputArray = DecryptAes(inputArray, key);
            PointerUtils.ByteArrayToPointer(outputArray, output);
        }
        
        /// <summary>
        /// PSP_KIRK_CMD_ECDSA_GEN_KEYS
        /// 
        /// Command: 12, 0xC
        /// </summary>
        /// <param name="Out"></param>
        /// <param name="outsize"></param>
        /// <returns></returns>
        public void ExecuteKirkCmd12(byte* Out, int outsize)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// PSP_KIRK_CMD_ECDSA_MULTIPLY_POINT
        /// 
        /// Command: 13, 0xD
        /// </summary>
        /// <param name="Out"></param>
        /// <param name="outsize"></param>
        /// <param name="In"></param>
        /// <param name="insize"></param>
        /// <returns></returns>
        public void ExecuteKirkCmd13(byte* Out, int outsize, byte* In, int insize)
        {
            //var ecdsa = ECDsa.Create();
            //ecdsa.
            throw new NotImplementedException();
        }

        /// <summary>
        /// PSP_KIRK_CMD_ECDSA_SIGN
        /// 
        /// Command: 16, 0x10
        /// </summary>
        /// <param name="Out"></param>
        /// <param name="outsize"></param>
        /// <param name="In"></param>
        /// <param name="insize"></param>
        /// <returns></returns>
        public void ExecuteKirkCmd16(byte* Out, int outsize, byte* In, int insize)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// PSP_KIRK_CMD_ECDSA_VERIFY
        /// 
        /// Command: 17, 0x11
        /// </summary>
        /// <param name="In"></param>
        /// <param name="insize"></param>
        /// <returns></returns>
        public void ExecuteKirkCmd17(byte* In, int insize)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// SIZE: 0004
        /// </summary>
        public struct KirkSha1Header
        {
            /// <summary>
            /// 0000 - Size of the input data source where will be generated the hash from.
            /// </summary>
            public int DataSize;
        }

        /// <summary>
        /// Creates a SHA1 Hash
        /// 
        /// Command: 11, 0xB
        /// </summary>
        /// <param name="outputBuffer"></param>
        /// <param name="inputBuffer"></param>
        /// <param name="inputSize"></param>
        /// <returns></returns>
        public void KirkSha1(byte* outputBuffer, byte* inputBuffer, int inputSize)
        {
            //CheckInitialized();

            var header = (KirkSha1Header*) inputBuffer;
            if (inputSize == 0 || header->DataSize == 0)
            {
                throw(new KirkException(ResultEnum.PspKirkDataSizeIsZero));
            }

            //Size <<= 4;
            //Size >>= 4;
            inputSize &= 0x0FFFFFFF;
            inputSize = (inputSize < header->DataSize) ? inputSize : header->DataSize;

            var sha1Hash = Sha1(
                PointerUtils.PointerToByteArray(inputBuffer + 4, inputSize)
            );

            PointerUtils.Memcpy(outputBuffer, sha1Hash, sha1Hash.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] Sha1(byte[] input)
        {
            return (new SHA1CryptoServiceProvider()).ComputeHash(input);
        }
        
        // kirk1
        public byte[] Kirk1Key =
            {0x98, 0xC9, 0x40, 0x97, 0x5C, 0x1D, 0x10, 0xE8, 0x7F, 0xE6, 0x0E, 0xA3, 0xFD, 0x03, 0xA8, 0xBA};

        public byte[] Kirk7Key03 =
            {0x98, 0x02, 0xC4, 0xE6, 0xEC, 0x9E, 0x9E, 0x2F, 0xFC, 0x63, 0x4C, 0xE4, 0x2F, 0xBB, 0x46, 0x68};

        public byte[] Kirk7Key04 =
            {0x99, 0x24, 0x4C, 0xD2, 0x58, 0xF5, 0x1B, 0xCB, 0xB0, 0x61, 0x9C, 0xA7, 0x38, 0x30, 0x07, 0x5F};

        public byte[] Kirk7Key05 =
            {0x02, 0x25, 0xD7, 0xBA, 0x63, 0xEC, 0xB9, 0x4A, 0x9D, 0x23, 0x76, 0x01, 0xB3, 0xF6, 0xAC, 0x17};

        public byte[] Kirk7Key0C =
            {0x84, 0x85, 0xC8, 0x48, 0x75, 0x08, 0x43, 0xBC, 0x9B, 0x9A, 0xEC, 0xA7, 0x9C, 0x7F, 0x60, 0x18};

        public byte[] Kirk7Key0D =
            {0xB5, 0xB1, 0x6E, 0xDE, 0x23, 0xA9, 0x7B, 0x0E, 0xA1, 0x7C, 0xDB, 0xA2, 0xDC, 0xDE, 0xC4, 0x6E};

        public byte[] Kirk7Key0E =
            {0xC8, 0x71, 0xFD, 0xB3, 0xBC, 0xC5, 0xD2, 0xF2, 0xE2, 0xD7, 0x72, 0x9D, 0xDF, 0x82, 0x68, 0x82};

        public byte[] Kirk7Key0F =
            {0x0A, 0xBB, 0x33, 0x6C, 0x96, 0xD4, 0xCD, 0xD8, 0xCB, 0x5F, 0x4B, 0xE0, 0xBA, 0xDB, 0x9E, 0x03};

        public byte[] Kirk7Key10 =
            {0x32, 0x29, 0x5B, 0xD5, 0xEA, 0xF7, 0xA3, 0x42, 0x16, 0xC8, 0x8E, 0x48, 0xFF, 0x50, 0xD3, 0x71};

        public byte[] Kirk7Key11 =
            {0x46, 0xF2, 0x5E, 0x8E, 0x4D, 0x2A, 0xA5, 0x40, 0x73, 0x0B, 0xC4, 0x6E, 0x47, 0xEE, 0x6F, 0x0A};

        public byte[] Kirk7Key12 =
            {0x5D, 0xC7, 0x11, 0x39, 0xD0, 0x19, 0x38, 0xBC, 0x02, 0x7F, 0xDD, 0xDC, 0xB0, 0x83, 0x7D, 0x9D};

        public byte[] Kirk7Key38 =
            {0x12, 0x46, 0x8D, 0x7E, 0x1C, 0x42, 0x20, 0x9B, 0xBA, 0x54, 0x26, 0x83, 0x5E, 0xB0, 0x33, 0x03};

        public byte[] Kirk7Key39 =
            {0xC4, 0x3B, 0xB6, 0xD6, 0x53, 0xEE, 0x67, 0x49, 0x3E, 0xA9, 0x5F, 0xBC, 0x0C, 0xED, 0x6F, 0x8A};

        public byte[] Kirk7Key3A =
            {0x2C, 0xC3, 0xCF, 0x8C, 0x28, 0x78, 0xA5, 0xA6, 0x63, 0xE2, 0xAF, 0x2D, 0x71, 0x5E, 0x86, 0xBA};

        // For 1.xx games eboot.bin
        public byte[] Kirk7Key4B =
            {0x0C, 0xFD, 0x67, 0x9A, 0xF9, 0xB4, 0x72, 0x4F, 0xD7, 0x8D, 0xD6, 0xE9, 0x96, 0x42, 0x28, 0x8B};

        public byte[] Kirk7Key53 =
            {0xAF, 0xFE, 0x8E, 0xB1, 0x3D, 0xD1, 0x7E, 0xD8, 0x0A, 0x61, 0x24, 0x1C, 0x95, 0x92, 0x56, 0xB6};

        public byte[] Kirk7Key57 =
            {0x1C, 0x9B, 0xC4, 0x90, 0xE3, 0x06, 0x64, 0x81, 0xFA, 0x59, 0xFD, 0xB6, 0x00, 0xBB, 0x28, 0x70};

        public byte[] Kirk7Key5D =
            {0x11, 0x5A, 0x5D, 0x20, 0xD5, 0x3A, 0x8D, 0xD3, 0x9C, 0xC5, 0xAF, 0x41, 0x0F, 0x0F, 0x18, 0x6F};

        public byte[] Kirk7Key63 =
            {0x9C, 0x9B, 0x13, 0x72, 0xF8, 0xC6, 0x40, 0xCF, 0x1C, 0x62, 0xF5, 0xD5, 0x92, 0xDD, 0xB5, 0x82};

        public byte[] Kirk7Key64 =
            {0x03, 0xB3, 0x02, 0xE8, 0x5F, 0xF3, 0x81, 0xB1, 0x3B, 0x8D, 0xAA, 0x2A, 0x90, 0xFF, 0x5E, 0x61};
        
        /// <summary>
        /// 
        /// </summary>
        public class KirkException : Exception
        {
            public ResultEnum Result;

            public KirkException(ResultEnum result, string message = "")
                : base($"KirkException: {result} : {message}")
            {
                Result = result;
            }
        }

        /// <summary>
        /// SIZE: 0014
        /// </summary>
        public struct KirkAes128CbcHeader
        {
            /// <summary>
            /// 0000 - 
            /// </summary>
            public KirkMode Mode;

            /// <summary>
            /// 0004 - 
            /// </summary>
            public int Unknown4;

            /// <summary>
            /// 0008 - 
            /// </summary>
            public int Unknown8;

            /// <summary>
            /// 000C - 
            /// </summary>
            public int KeySeed;

            /// <summary>
            /// 0010 - 
            /// </summary>
            public int Datasize;
        }

        /// <summary>
        /// SIZE: 0090
        /// </summary>
        public struct Aes128CmacHeader
        {
            /// <summary>
            /// 0000 -
            /// </summary>
            public fixed byte AesKey[16];

            /// <summary>
            /// 0010 -
            /// </summary>
            public fixed byte CmacKey[16];

            /// <summary>
            /// 0020 -
            /// </summary>
            public fixed byte CmacHeaderHash[16];

            /// <summary>
            /// 0030 -
            /// </summary>
            public fixed byte CmacDataHash[16];

            /// <summary>
            /// 0040 -
            /// </summary>
            public fixed byte Unknown1[32];

            /// <summary>
            /// 0060 -
            /// </summary>
            public KirkMode Mode;

            /// <summary>
            /// 0061 -
            /// </summary>
            public byte UseEcdsAhash;

            /// <summary>
            /// 0064 -
            /// </summary>
            public fixed byte Unknown2[11];

            /// <summary>
            /// 0070 -
            /// </summary>
            public int DataSize;

            /// <summary>
            /// 0074 -
            /// </summary>
            public int DataOffset;

            /// <summary>
            /// 0078 -
            /// </summary>
            public fixed byte Unknown3[8];

            /// <summary>
            /// 0080 -
            /// </summary>
            public fixed byte Unknown4[16];
        }

        /// <summary>
        /// "mode" in header
        /// </summary>
        public enum KirkMode
        {
            /// <summary>
            /// KIRK_MODE_CMD1 = 1
            /// </summary>
            Cmd1 = 1,

            /// <summary>
            /// KIRK_MODE_CMD2 = 2
            /// </summary>
            Cmd2 = 2,

            /// <summary>
            /// KIRK_MODE_CMD3 = 3
            /// </summary>
            Cmd3 = 3,

            /// <summary>
            /// KIRK_MODE_ENCRYPT_CBC = 4
            /// </summary>
            EncryptCbc = 4,

            /// <summary>
            /// KIRK_MODE_DECRYPT_CBC = 5
            /// </summary>
            DecryptCbc = 5,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum CommandEnum : uint
        {
            /// <summary>
            /// Master decryption command, used by firmware modules. Applies CMAC checking.
            /// Super-Duper decryption (no inverse)
            /// Private Sig + Cipher
            /// 
            /// KIRK_CMD_DECRYPT_PRIVATE
            /// 
            /// Code: 1, 0x01
            /// </summary>
            PspKirkCmdDecryptPrivate = 0x1,

            /// <summary>
            /// Used for key type 3 (blacklisting), encrypts and signs data with a ECDSA signature.
            /// Encrypt Operation (inverse of 0x03)
            /// Private Sig + Cipher
            /// 
            /// Code: 2, 0x02
            /// </summary>
            PspKirkCmdEncryptSign = 0x2,

            /// <summary>
            /// Used for key type 3 (blacklisting), decrypts and signs data with a ECDSA signature.
            /// Decrypt Operation (inverse of 0x02)
            /// Private Sig + Cipher
            /// 
            /// Code: 3, 0x03
            /// </summary>
            PspKirkCmdDecryptSign = 0x3,

            /// <summary>
            /// Key table based encryption used for general purposes by several modules.
            /// Encrypt Operation (inverse of 0x07) (IV=0)
            /// Cipher
            /// 
            /// KIRK_CMD_ENCRYPT_IV_0
            /// 
            /// Code: 4, 0x04
            /// </summary>
            PspKirkCmdEncrypt = 0x4,

            /// <summary>
            /// Fuse ID based encryption used for general purposes by several modules.
            /// Encrypt Operation (inverse of 0x08) (IV=FuseID)
            /// Cipher
            /// 
            /// KIRK_CMD_ENCRYPT_IV_FUSE
            /// 
            /// Code: 5, 0x05
            /// </summary>
            PspKirkCmdEncryptFuse = 0x5,

            /// <summary>
            /// User specified ID based encryption used for general purposes by several modules.
            /// Encrypt Operation (inverse of 0x09) (IV=UserDefined)
            /// Cipher
            /// 
            /// KIRK_CMD_ENCRYPT_IV_USER
            /// 
            /// Code: 6, 0x06
            /// </summary>
            PspKirkCmdEncryptUser = 0x6,

            /// <summary>
            /// Key table based decryption used for general purposes by several modules.
            /// Decrypt Operation (inverse of 0x04)
            /// Cipher
            /// 
            /// KIRK_CMD_DECRYPT_IV_0
            /// 
            /// Code: 7, 0x07
            /// </summary>
            PspKirkCmdDecrypt = 0x7,

            /// <summary>
            /// Fuse ID based decryption used for general purposes by several modules.
            /// Decrypt Operation (inverse of 0x05)
            /// Cipher
            /// 
            /// KIRK_CMD_DECRYPT_IV_FUSE
            /// 
            /// Code: 8, 0x08
            /// </summary>
            PspKirkCmdDecryptFuse = 0x8,

            /// <summary>
            /// User specified ID based decryption used for general purposes by several modules.
            /// Decrypt Operation (inverse of 0x06)
            /// Cipher
            /// 
            /// KIRK_CMD_DECRYPT_IV_USER
            /// 
            /// Code: 9, 0x09
            /// </summary>
            PspKirkCmdDecryptUser = 0x9,

            /// <summary>
            /// Private signature (SCE) checking command.
            /// Private Signature Check (checks for private SCE sig)
            /// Sig Gens
            /// 
            /// KIRK_CMD_PRIV_SIG_CHECK
            /// 
            /// Code: 10, 0x0A
            /// </summary>
            PspKirkCmdPrivSigCheck = 0xA,

            /// <summary>
            /// SHA1 hash generating command.
            /// SHA1 Hash
            /// Sig Gens
            /// 
            /// KIRK_CMD_SHA1_HASH
            /// 
            /// Code: 11, 0x0B
            /// </summary>
            PspKirkCmdSha1Hash = 0xB,

            /// <summary>
            /// ECDSA key generating mul1 command. 
            /// Mul1
            /// Sig Gens
            /// 
            /// Code: 12, 0x0C
            /// </summary>
            PspKirkCmdEcdsaGenKeys = 0xC,

            /// <summary>
            /// ECDSA key generating mul2 command. 
            /// Mul2
            /// Sig Gens
            /// 
            /// Code: 13, 0x0D
            /// </summary>
            PspKirkCmdEcdsaMultiplyPoint = 0xD,

            /// <summary>
            /// Random number generating command. 
            /// Random Number Gen
            /// Sig Gens
            /// 
            /// Code: 14, 0x0E
            /// </summary>
            PspKirkCmdPrng = 0xE,

            /// <summary>
            /// KIRK initialization command.
            /// (absolutely no idea? could be KIRK initialization)
            /// Sig Gens
            /// 
            /// Code: 15, 0x0F
            /// </summary>
            PspKirkCmdInit = 0xF,

            /// <summary>
            /// ECDSA signing command.
            /// Signature Gen
            /// 
            /// Code: 16, 0x10
            /// </summary>
            PspKirkCmdEcdsaSign = 0x10,

            /// <summary>
            /// ECDSA checking command.
            /// Signature Check (checks for generated sigs)
            /// Sig Checks
            /// 
            /// Code: 17, 0x11
            /// </summary>
            PspKirkCmdEcdsaVerify = 0x11,

            /// <summary>
            /// Certificate checking command.
            /// Certificate Check (idstorage signatures)
            /// Sig Checks
            /// 
            /// Code: 18, 0x12
            /// </summary>
            PspKirkCmdCertVerify = 0x12,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum ResultEnum
        {
            /// <summary>
            /// KIRK_OPERATION_SUCCESS
            /// </summary>
            Ok = 0,

            /// <summary>
            /// KIRK_NOT_ENABLED
            /// </summary>
            PspKirkNotEnabled = 0x1,

            /// <summary>
            /// KIRK_INVALID_MODE
            /// </summary>
            PspKirkInvalidMode = 0x2,

            /// <summary>
            /// KIRK_HEADER_HASH_INVALID
            /// </summary>
            PspKirkInvalidHeaderHash = 0x3,

            /// <summary>
            /// KIRK_DATA_HASH_INVALID
            /// </summary>
            PspKirkInvalidDataHash = 0x4,

            /// <summary>
            /// KIRK_SIG_CHECK_INVALID
            /// </summary>
            PspKirkInvalidSigCheck = 0x5,

            /// <summary>
            /// KIRK_UNK_1
            /// </summary>
            PspKirkUnk1 = 0x6,

            /// <summary>
            /// KIRK_UNK_2
            /// </summary>
            PspKirkUnk2 = 0x7,

            /// <summary>
            /// KIRK_UNK_3
            /// </summary>
            PspKirkUnk3 = 0x8,

            /// <summary>
            /// KIRK_UNK_4
            /// </summary>
            PspKirkUnk4 = 0x9,

            /// <summary>
            /// KIRK_UNK_5
            /// </summary>
            PspKirkUnk5 = 0xA,

            /// <summary>
            /// KIRK_UNK_6
            /// </summary>
            PspKirkUnk6 = 0xB,

            /// <summary>
            /// KIRK_NOT_INITIALIZED
            /// </summary>
            PspKirkNotInit = 0xC,

            /// <summary>
            /// KIRK_INVALID_OPERATION
            /// </summary>
            PspKirkInvalidOperation = 0xD,

            /// <summary>
            /// KIRK_INVALID_SEED_CODE
            /// </summary>
            PspKirkInvalidSeed = 0xE,

            /// <summary>
            /// KIRK_INVALID_SIZE
            /// </summary>
            PspKirkInvalidSize = 0xF,

            /// <summary>
            /// KIRK_DATA_SIZE_ZERO
            /// </summary>
            PspKirkDataSizeIsZero = 0x10,

            /// <summary>
            /// SUBCWR_NOT_16_ALGINED
            /// </summary>
            PspSubcwrNot16Algined = 0x90A,

            /// <summary>
            /// SUBCWR_HEADER_HASH_INVALID
            /// </summary>
            PspSubcwrHeaderHashInvalid = 0x920,

            /// <summary>
            /// SUBCWR_BUFFER_TOO_SMALL
            /// </summary>
            PspSubcwrBufferTooSmall = 0x1000,
        }

        private static Aes CreateAes() => Aes.Create() ?? throw new Exception("Can't find AES");
    }
}