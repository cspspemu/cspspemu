using System;

namespace CSPspEmu.Core.Components.Crypto
{
    public unsafe partial class Kirk
    {
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
    }
}