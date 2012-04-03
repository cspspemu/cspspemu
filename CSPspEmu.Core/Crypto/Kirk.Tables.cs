using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Endian;

namespace CSPspEmu.Core.Crypto
{
	unsafe public partial class Kirk
	{
		// Kirk return values
		public const int KIRK_OPERATION_SUCCESS = 0;
		public const int KIRK_NOT_ENABLED = 1;
		public const int KIRK_INVALID_MODE = 2;
		public const int KIRK_HEADER_HASH_INVALID = 3;
		public const int KIRK_DATA_HASH_INVALID = 4;
		public const int KIRK_SIG_CHECK_INVALID = 5;
		public const int KIRK_UNK_1 = 6;
		public const int KIRK_UNK_2 = 7;
		public const int KIRK_UNK_3 = 8;
		public const int KIRK_UNK_4 = 9;
		public const int KIRK_UNK_5 = 0xA; // 10
		public const int KIRK_UNK_6 = 0xB; // 11
		public const int KIRK_NOT_INITIALIZED = 0xC; // 12
		public const int KIRK_INVALID_OPERATION = 0xD; // 13
		public const int KIRK_INVALID_SEED_CODE = 0xE; // 14
		public const int KIRK_INVALID_SIZE = 0xF; // 15
		public const int KIRK_DATA_SIZE_ZERO = 0x10; // 16

		public class KirkException : Exception
		{
			int Result;

			public KirkException(int Result)
			{
				this.Result = Result;
			}
		}

		/// <summary>
		/// SIZE: 0014
		/// </summary>
		public struct KIRK_AES128CBC_HEADER
		{
			/// <summary>
			/// 0000 - 
			/// </summary>
			public int Mode;

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
		public struct AES128CMACHeader
		{
			/// <summary>
			/// 0000 -
			/// </summary>
			public fixed byte AES_key[16];

			/// <summary>
			/// 0010 -
			/// </summary>
			public fixed byte CMAC_key[16];

			/// <summary>
			/// 0020 -
			/// </summary>
			public fixed byte CMAC_header_hash[16];

			/// <summary>
			/// 0030 -
			/// </summary>
			public fixed byte CMAC_data_hash[16];

			/// <summary>
			/// 0040 -
			/// </summary>
			public fixed byte Unknown1[32];

			/// <summary>
			/// 0060 -
			/// </summary>
			public uint Mode;

			/// <summary>
			/// 0061 -
			/// </summary>
			public byte UseECDSAhash;

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

		//"mode" in header
		public const int KIRK_MODE_CMD1 = 1;
		public const int KIRK_MODE_CMD2 = 2;
		public const int KIRK_MODE_CMD3 = 3;
		public const int KIRK_MODE_ENCRYPT_CBC = 4;
		public const int KIRK_MODE_DECRYPT_CBC = 5;

		//sceUtilsBufferCopyWithRange errors
		public const int SUBCWR_NOT_16_ALGINED = 0x90A;
		public const int SUBCWR_HEADER_HASH_INVALID = 0x920;
		public const int SUBCWR_BUFFER_TOO_SMALL = 0x1000;

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
			PSP_KIRK_CMD_DECRYPT_PRIVATE = 0x1,

			/// <summary>
			/// Used for key type 3 (blacklisting), encrypts and signs data with a ECDSA signature.
			/// Encrypt Operation (inverse of 0x03)
			/// Private Sig + Cipher
			/// 
			/// Code: 2, 0x02
			/// </summary>
			PSP_KIRK_CMD_ENCRYPT_SIGN = 0x2,

			/// <summary>
			/// Used for key type 3 (blacklisting), decrypts and signs data with a ECDSA signature.
			/// Decrypt Operation (inverse of 0x02)
			/// Private Sig + Cipher
			/// 
			/// Code: 3, 0x03
			/// </summary>
			PSP_KIRK_CMD_DECRYPT_SIGN = 0x3,

			/// <summary>
			/// Key table based encryption used for general purposes by several modules.
			/// Encrypt Operation (inverse of 0x07) (IV=0)
			/// Cipher
			/// 
			/// KIRK_CMD_ENCRYPT_IV_0
			/// 
			/// Code: 4, 0x04
			/// </summary>
			PSP_KIRK_CMD_ENCRYPT = 0x4,

			/// <summary>
			/// Fuse ID based encryption used for general purposes by several modules.
			/// Encrypt Operation (inverse of 0x08) (IV=FuseID)
			/// Cipher
			/// 
			/// KIRK_CMD_ENCRYPT_IV_FUSE
			/// 
			/// Code: 5, 0x05
			/// </summary>
			PSP_KIRK_CMD_ENCRYPT_FUSE = 0x5,

			/// <summary>
			/// User specified ID based encryption used for general purposes by several modules.
			/// Encrypt Operation (inverse of 0x09) (IV=UserDefined)
			/// Cipher
			/// 
			/// KIRK_CMD_ENCRYPT_IV_USER
			/// 
			/// Code: 6, 0x06
			/// </summary>
			PSP_KIRK_CMD_ENCRYPT_USER = 0x6,

			/// <summary>
			/// Key table based decryption used for general purposes by several modules.
			/// Decrypt Operation (inverse of 0x04)
			/// Cipher
			/// 
			/// KIRK_CMD_DECRYPT_IV_0
			/// 
			/// Code: 7, 0x07
			/// </summary>
			PSP_KIRK_CMD_DECRYPT = 0x7,

			/// <summary>
			/// Fuse ID based decryption used for general purposes by several modules.
			/// Decrypt Operation (inverse of 0x05)
			/// Cipher
			/// 
			/// KIRK_CMD_DECRYPT_IV_FUSE
			/// 
			/// Code: 8, 0x08
			/// </summary>
			PSP_KIRK_CMD_DECRYPT_FUSE = 0x8,

			/// <summary>
			/// User specified ID based decryption used for general purposes by several modules.
			/// Decrypt Operation (inverse of 0x06)
			/// Cipher
			/// 
			/// KIRK_CMD_DECRYPT_IV_USER
			/// 
			/// Code: 9, 0x09
			/// </summary>
			PSP_KIRK_CMD_DECRYPT_USER = 0x9,

			/// <summary>
			/// Private signature (SCE) checking command.
			/// Private Signature Check (checks for private SCE sig)
			/// Sig Gens
			/// 
			/// KIRK_CMD_PRIV_SIG_CHECK
			/// 
			/// Code: 10, 0x0A
			/// </summary>
			PSP_KIRK_CMD_PRIV_SIG_CHECK = 0xA,

			/// <summary>
			/// SHA1 hash generating command.
			/// SHA1 Hash
			/// Sig Gens
			/// 
			/// KIRK_CMD_SHA1_HASH
			/// 
			/// Code: 11, 0x0B
			/// </summary>
			PSP_KIRK_CMD_SHA1_HASH = 0xB,

			/// <summary>
			/// ECDSA key generating mul1 command. 
			/// Mul1
			/// Sig Gens
			/// 
			/// Code: 12, 0x0C
			/// </summary>
			PSP_KIRK_CMD_ECDSA_GEN_KEYS = 0xC,

			/// <summary>
			/// ECDSA key generating mul2 command. 
			/// Mul2
			/// Sig Gens
			/// 
			/// Code: 13, 0x0D
			/// </summary>
			PSP_KIRK_CMD_ECDSA_MULTIPLY_POINT = 0xD,

			/// <summary>
			/// Random number generating command. 
			/// Random Number Gen
			/// Sig Gens
			/// 
			/// Code: 14, 0x0E
			/// </summary>
			PSP_KIRK_CMD_PRNG = 0xE,

			/// <summary>
			/// KIRK initialization command.
			/// (absolutely no idea? could be KIRK initialization)
			/// Sig Gens
			/// 
			/// Code: 15, 0x0F
			/// </summary>
			PSP_KIRK_CMD_INIT = 0xF,

			/// <summary>
			/// ECDSA signing command.
			/// Signature Gen
			/// 
			/// Code: 16, 0x10
			/// </summary>
			PSP_KIRK_CMD_ECDSA_SIGN = 0x10,

			/// <summary>
			/// ECDSA checking command.
			/// Signature Check (checks for generated sigs)
			/// Sig Checks
			/// 
			/// Code: 17, 0x11
			/// </summary>
			PSP_KIRK_CMD_ECDSA_VERIFY = 0x11,

			/// <summary>
			/// Certificate checking command.
			/// Certificate Check (idstorage signatures)
			/// Sig Checks
			/// 
			/// Code: 18, 0x12
			/// </summary>
			PSP_KIRK_CMD_CERT_VERIFY = 0x12,
		}

		/// <summary>
		/// 
		/// </summary>
		public enum ResultEnum
		{
			OK = 0,
			PSP_KIRK_NOT_ENABLED = 0x1,
			PSP_KIRK_INVALID_MODE = 0x2,
			PSP_KIRK_INVALID_HEADER_HASH = 0x3,
			PSP_KIRK_INVALID_DATA_HASH = 0x4,
			PSP_KIRK_INVALID_SIG_CHECK = 0x5,
			PSP_KIRK_UNK1 = 0x6,
			PSP_KIRK_UNK2 = 0x7,
			PSP_KIRK_UNK3 = 0x8,
			PSP_KIRK_UNK4 = 0x9,
			PSP_KIRK_UNK5 = 0xA,
			PSP_KIRK_UNK6 = 0xB,
			PSP_KIRK_NOT_INIT = 0xC,
			PSP_KIRK_INVALID_OPERATION = 0xD,
			PSP_KIRK_INVALID_SEED = 0xE,
			PSP_KIRK_INVALID_SIZE = 0xF,
			PSP_KIRK_DATA_SIZE_IS_ZERO = 0x10,
			PSP_SUBCWR_NOT_16_ALGINED = 0x90A,
			PSP_SUBCWR_HEADER_HASH_INVALID = 0x920,
			PSP_SUBCWR_BUFFER_TOO_SMALL = 0x1000,
		}
	}
}
