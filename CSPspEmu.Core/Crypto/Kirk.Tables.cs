using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Crypto
{
	unsafe public partial class Kirk
	{
		//Kirk return values
		const int KIRK_OPERATION_SUCCESS = 0;
		const int KIRK_NOT_ENABLED = 1;
		const int KIRK_INVALID_MODE = 2;
		const int KIRK_HEADER_HASH_INVALID = 3;
		const int KIRK_DATA_HASH_INVALID = 4;
		const int KIRK_SIG_CHECK_INVALID = 5;
		const int KIRK_UNK_1 = 6;
		const int KIRK_UNK_2 = 7;
		const int KIRK_UNK_3 = 8;
		const int KIRK_UNK_4 = 9;
		const int KIRK_UNK_5 = 0xA;
		const int KIRK_UNK_6 = 0xB;
		const int KIRK_NOT_INITIALIZED = 0xC;
		const int KIRK_INVALID_OPERATION = 0xD;
		const int KIRK_INVALID_SEED_CODE = 0xE;
		const int KIRK_INVALID_SIZE = 0xF;
		const int KIRK_DATA_SIZE_ZERO = 0x10;

		public struct KIRK_AES128CBC_HEADER
		{
			public int mode;    //0
			public int unk_4;   //4
			public int unk_8;   //8
			public int keyseed; //C
			public int data_size;   //10
		} //0x14

		public struct KIRK_CMD1_HEADER
		{
			public fixed byte AES_key[16];            //0
			public fixed byte CMAC_key[16];           //10
			public fixed byte CMAC_header_hash[16];   //20
			public fixed byte CMAC_data_hash[16];     //30
			public fixed byte unused[32];             //40
			public uint mode;                  //60
			public fixed byte unk3[12];               //64
			public int data_size;             //70
			public int data_offset;           //74  
			public fixed byte unk4[8];                //78
			public fixed byte unk5[16];               //80
		} //0x90

		struct KIRK_SHA1_HEADER
		{
			public int data_size;             //0     
		}            //4

		//mode passed to sceUtilsBufferCopyWithRange
		const int KIRK_CMD_DECRYPT_PRIVATE = 1;
		const int KIRK_CMD_ENCRYPT_IV_0 = 4;
		const int KIRK_CMD_ENCRYPT_IV_FUSE = 5;
		const int KIRK_CMD_ENCRYPT_IV_USER = 6;
		const int KIRK_CMD_DECRYPT_IV_0 = 7;
		const int KIRK_CMD_DECRYPT_IV_FUSE = 8;
		const int KIRK_CMD_DECRYPT_IV_USER = 9;
		const int KIRK_CMD_PRIV_SIG_CHECK = 10;
		const int KIRK_CMD_SHA1_HASH = 11;

		//"mode" in header
		const int KIRK_MODE_CMD1 = 1;
		const int KIRK_MODE_CMD2 = 2;
		const int KIRK_MODE_CMD3 = 3;
		const int KIRK_MODE_ENCRYPT_CBC = 4;
		const int KIRK_MODE_DECRYPT_CBC = 5;

		//sceUtilsBufferCopyWithRange errors
		const int SUBCWR_NOT_16_ALGINED = 0x90A;
		const int SUBCWR_HEADER_HASH_INVALID = 0x920;
		const int SUBCWR_BUFFER_TOO_SMALL = 0x1000;

		/*
			  // Private Sig + Cipher
			  0x01: Super-Duper decryption (no inverse)
			  0x02: Encrypt Operation (inverse of 0x03)
			  0x03: Decrypt Operation (inverse of 0x02)

			  // Cipher
			  0x04: Encrypt Operation (inverse of 0x07) (IV=0)
			  0x05: Encrypt Operation (inverse of 0x08) (IV=FuseID)
			  0x06: Encrypt Operation (inverse of 0x09) (IV=UserDefined)
			  0x07: Decrypt Operation (inverse of 0x04)
			  0x08: Decrypt Operation (inverse of 0x05)
			  0x09: Decrypt Operation (inverse of 0x06)
	  
			  // Sig Gens
			  0x0A: Private Signature Check (checks for private SCE sig)
			  0x0B: SHA1 Hash
			  0x0C: Mul1
			  0x0D: Mul2
			  0x0E: Random Number Gen
			  0x0F: (absolutely no idea? could be KIRK initialization)
			  0x10: Signature Gen
			  // Sig Checks
			  0x11: Signature Check (checks for generated sigs)
			  0x12: Certificate Check (idstorage signatures)
		*/

	}
}
