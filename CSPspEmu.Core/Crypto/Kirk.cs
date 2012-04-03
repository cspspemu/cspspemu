using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Extensions;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.IO;

namespace CSPspEmu.Core.Crypto
{
	unsafe public delegate void PointerAction(byte* Address);

	unsafe public partial class Kirk
	{
		//small struct for temporary keeping AES & CMAC key from CMD1 header
		struct header_keys
		{
			public fixed byte AES[16];
			public fixed byte CMAC[16];
		}

		byte[] fuseID = new byte[16]; // Emulate FUSEID	

		Crypto.AES_ctx _aes_kirk1; //global

		Random Random;

		bool IsKirkInitialized; //"init" emulation

		/// <summary>
		/// 
		/// </summary>
		/// <param name="outbuff"></param>
		/// <param name="inbuff"></param>
		/// <param name="size"></param>
		/// <param name="generate_trash"></param>
		/// <returns></returns>
		public int kirk_CMD0(byte* outbuff, byte* inbuff, int size, bool generate_trash)
		{
			var _cmac_header_hash = new byte[16];
			var _cmac_data_hash = new byte[16];

			fixed (byte* cmac_header_hash = _cmac_header_hash)
			fixed (byte* cmac_data_hash = _cmac_data_hash)
			fixed (Crypto.AES_ctx* aes_kirk1_ptr = &_aes_kirk1)
			{

				if (!IsKirkInitialized) return KIRK_NOT_INITIALIZED;
	
				AES128CMACHeader* header = (AES128CMACHeader*)outbuff;
	
				Crypto.memcpy(outbuff, inbuff, size);
	
				if(header->Mode != KIRK_MODE_CMD1) return KIRK_INVALID_MODE;
	
				header_keys *keys = (header_keys *)outbuff; //0-15 AES key, 16-31 CMAC key
	
				//FILL PREDATA WITH RANDOM DATA
				if(generate_trash) kirk_CMD14(outbuff+sizeof(AES128CMACHeader), header->DataOffset);
	
				//Make sure data is 16 aligned
				int chk_size = header->DataSize;
				if((chk_size % 16) != 0) chk_size += 16 - (chk_size % 16);
	
				//ENCRYPT DATA
				Crypto.AES_ctx k1;
				Crypto.AES_set_key(&k1, keys->AES, 128);
	
				Crypto.AES_cbc_encrypt(&k1, inbuff+sizeof(AES128CMACHeader)+header->DataOffset, outbuff+sizeof(AES128CMACHeader)+header->DataOffset, chk_size);
	
				//CMAC HASHES
				Crypto.AES_ctx cmac_key;
				Crypto.AES_set_key(&cmac_key, keys->CMAC, 128);
		
				Crypto.AES_CMAC(&cmac_key, outbuff + 0x60, 0x30, cmac_header_hash);

				Crypto.AES_CMAC(&cmac_key, outbuff + 0x60, 0x30 + chk_size + header->DataOffset, cmac_data_hash);

				Crypto.memcpy(header->CMAC_header_hash, cmac_header_hash, 16);
				Crypto.memcpy(header->CMAC_data_hash, cmac_data_hash, 16);

				//ENCRYPT KEYS
				Crypto.AES_cbc_encrypt(aes_kirk1_ptr, inbuff, outbuff, 16 * 2);
			}

			return KIRK_OPERATION_SUCCESS;
		}

		/// <summary>
		/// Cypher-Block Chaining decoding.
		/// Master decryption command, used by firmware modules. Applies CMAC checking.
		/// </summary>
		/// <param name="outbuff"></param>
		/// <param name="inbuff"></param>
		/// <param name="size"></param>
		/// <param name="do_check"></param>
		/// <returns></returns>
		public int kirk_CMD1(byte* outbuff, byte* inbuff, int size, bool do_check = true)
		{
			fixed (Crypto.AES_ctx* aes_kirk1_ptr = &_aes_kirk1)
			{
				if (!IsKirkInitialized) return KIRK_NOT_INITIALIZED;

				var header = (AES128CMACHeader*)inbuff;
				//Console.WriteLine(MathUtils.ByteSwap(header->Mode));
				if (header->Mode != KIRK_MODE_CMD1) return KIRK_INVALID_MODE;

				header_keys keys; //0-15 AES key, 16-31 CMAC key

				Crypto.AES_cbc_decrypt(aes_kirk1_ptr, inbuff, (byte*)&keys, 16 * 2); //decrypt AES & CMAC key to temp buffer

				//AES.CreateDecryptor(

				// HOAX WARRING! I have no idea why the hash check on last IPL block fails, so there is an option to disable checking
				if (do_check)
				{
					int ret = kirk_CMD10(inbuff, size);
					if (ret != KIRK_OPERATION_SUCCESS) return ret;
				}

				//var AES = new RijndaelManaged();

				Crypto.AES_ctx k1;
				Crypto.AES_set_key(&k1, keys.AES, 128);

				Crypto.AES_cbc_decrypt(&k1, inbuff + sizeof(AES128CMACHeader) + header->DataOffset, outbuff, header->DataSize);

				return KIRK_OPERATION_SUCCESS;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="outbuff"></param>
		/// <param name="inbuff"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public int kirk_CMD4(byte* outbuff, byte* inbuff, int size)
		{
			if(!IsKirkInitialized) return KIRK_NOT_INITIALIZED;
	
			KIRK_AES128CBC_HEADER *header = (KIRK_AES128CBC_HEADER*)inbuff;
			if(header->Mode != KIRK_MODE_ENCRYPT_CBC) return KIRK_INVALID_MODE;
			if(header->Datasize == 0) return KIRK_DATA_SIZE_ZERO;

			kirk_4_7_get_key(header->KeySeed, (key) =>
			{
				if (key == (byte*)KIRK_INVALID_SIZE) throw(new KirkException(KIRK_INVALID_SIZE));

				// Set the key
				Crypto.AES_ctx aesKey;
				Crypto.AES_set_key(&aesKey, key, 128);
				Crypto.AES_cbc_encrypt(&aesKey, inbuff + sizeof(KIRK_AES128CBC_HEADER), outbuff, size);
			});

			return KIRK_OPERATION_SUCCESS;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="outbuff"></param>
		/// <param name="inbuff"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public int kirk_CMD7(byte* outbuff, byte* inbuff, int size)
		{
			if (!IsKirkInitialized) return KIRK_NOT_INITIALIZED;
	
			var Header = (KIRK_AES128CBC_HEADER*)inbuff;
			if (Header->Mode != KIRK_MODE_DECRYPT_CBC) return KIRK_INVALID_MODE;
			if (Header->Datasize == 0) return KIRK_DATA_SIZE_ZERO;

#if true
			var Output = DecryptAes(
				PointerUtils.PointerToByteArray(inbuff + sizeof(KIRK_AES128CBC_HEADER), size),
				_kirk_4_7_get_key(Header->KeySeed)
			);

			PointerUtils.ByteArrayToPointer(Output, outbuff);
#else
			kirk_4_7_get_key(header->KeySeed, (key) =>
			{
				//Set the key
				Crypto.AES_ctx aesKey;
				Crypto.AES_set_key(&aesKey, key, 128);

				Crypto.AES_cbc_decrypt(&aesKey, inbuff + sizeof(KIRK_AES128CBC_HEADER), outbuff, size);
			});
#endif

			return KIRK_OPERATION_SUCCESS;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inbuff"></param>
		/// <param name="insize"></param>
		/// <returns></returns>
		public int kirk_CMD10(byte* inbuff, int insize)
		{
			var _cmac_header_hash = new byte[16];
			var _cmac_data_hash = new byte[16];
			fixed (byte* cmac_header_hash = _cmac_header_hash)
			fixed (byte* cmac_data_hash = _cmac_data_hash)
			fixed (Crypto.AES_ctx* aes_kirk1_ptr = &_aes_kirk1)
			{
				if (!IsKirkInitialized) return KIRK_NOT_INITIALIZED;

				AES128CMACHeader* header = (AES128CMACHeader*)inbuff;

				if (!(header->Mode == KIRK_MODE_CMD1 || header->Mode == KIRK_MODE_CMD2 || header->Mode == KIRK_MODE_CMD3)) return KIRK_INVALID_MODE;
				if (header->DataSize == 0) return KIRK_DATA_SIZE_ZERO;

				if (header->Mode != KIRK_MODE_CMD1)
				{
					return KIRK_SIG_CHECK_INVALID; //Checks for cmd 2 & 3 not included right now
				}

				header_keys keys; //0-15 AES key, 16-31 CMAC key

				Crypto.AES_cbc_decrypt(aes_kirk1_ptr, inbuff, (byte*)&keys, 32); //decrypt AES & CMAC key to temp buffer

				Crypto.AES_ctx cmac_key;
				Crypto.AES_set_key(&cmac_key, keys.CMAC, 128);

				Crypto.AES_CMAC(&cmac_key, inbuff + 0x60, 0x30, cmac_header_hash);

				//Make sure data is 16 aligned
				int chk_size = header->DataSize;
				if ((chk_size % 16) != 0) chk_size += 16 - (chk_size % 16);
				Crypto.AES_CMAC(&cmac_key, inbuff + 0x60, 0x30 + chk_size + header->DataOffset, cmac_data_hash);

				if (Crypto.memcmp(cmac_header_hash, header->CMAC_header_hash, 16) != 0)
				{
					Console.WriteLine("header hash invalid");
					return KIRK_HEADER_HASH_INVALID;
				}
				if (Crypto.memcmp(cmac_data_hash, header->CMAC_data_hash, 16) != 0)
				{
					Console.WriteLine("data hash invalid");
					return KIRK_DATA_HASH_INVALID;
				}

				return KIRK_OPERATION_SUCCESS;
			}
		}

		/// <summary>
		/// Generate Random Data
		/// </summary>
		/// <param name="outbuff"></param>
		/// <param name="OutputSize"></param>
		/// <returns></returns>
		public int kirk_CMD14(byte* Output, int OutputSize)
		{
			if (!IsKirkInitialized) return KIRK_NOT_INITIALIZED;
			for (int i = 0; i < OutputSize; i++) Output[i] = (byte)(Random.Next() & 0xFF);
			return KIRK_OPERATION_SUCCESS;
		}

		/// <summary>
		/// Initializes kirk
		/// </summary>
		/// <returns></returns>
		public int kirk_init()
		{
			fixed (byte* kirk1_key_ptr = kirk1_key)
			fixed (Crypto.AES_ctx* aes_kirk1_ptr = &_aes_kirk1)
			{
				Crypto.AES_set_key(aes_kirk1_ptr, kirk1_key_ptr, 128);
			}
			IsKirkInitialized = true;
			Random = new Random();
			return KIRK_OPERATION_SUCCESS;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key_type"></param>
		/// <returns></returns>
		public byte[] _kirk_4_7_get_key(int key_type)
		{
			switch (key_type)
			{
				case (0x03): return kirk7_key03;
				case (0x04): return kirk7_key04;
				case (0x05): return kirk7_key05;
				case (0x0C): return kirk7_key0C;
				case (0x0D): return kirk7_key0D;
				case (0x0E): return kirk7_key0E;
				case (0x0F): return kirk7_key0F;
				case (0x10): return kirk7_key10;
				case (0x11): return kirk7_key11;
				case (0x12): return kirk7_key12;
				case (0x38): return kirk7_key38;
				case (0x39): return kirk7_key39;
				case (0x3A): return kirk7_key3A;
				case (0x4B): return kirk7_key4B;
				case (0x53): return kirk7_key53;
				case (0x57): return kirk7_key57;
				case (0x5D): return kirk7_key5D;
				case (0x63): return kirk7_key63;
				case (0x64): return kirk7_key64;
				default: throw(new NotImplementedException(String.Format("Invalid Key Type: 0x{0:X}", key_type)));
					//throw (new KirkException(KIRK_INVALID_SIZE));
				//default: return (byte*)KIRK_INVALID_SIZE; break; //need to get the real error code for that, placeholder now :)
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key_type"></param>
		/// <returns></returns>
		public void kirk_4_7_get_key(int key_type, PointerAction PointerAction)
		{
			fixed (byte* key = _kirk_4_7_get_key(key_type))
			{
				PointerAction(key);
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
		public int kirk_CMD1_ex(byte* outbuff, byte* inbuff, int size, AES128CMACHeader* header)
		{
			var _buffer = new byte[size];
			fixed (byte* buffer = _buffer)
			{
				Crypto.memcpy(buffer, header, sizeof(AES128CMACHeader));
				Crypto.memcpy(buffer + sizeof(AES128CMACHeader), inbuff, header->DataSize);
				return kirk_CMD1(outbuff, buffer, size, true);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fuse"></param>
		/// <returns></returns>
		public int sceUtilsSetFuseID(void* fuse)
		{
			Marshal.Copy(new IntPtr(fuse), fuseID, 0, 16);
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
			fixed (Crypto.AES_ctx* aes_kirk1_ptr = &_aes_kirk1)
			{
				Crypto.AES_cbc_decrypt(aes_kirk1_ptr, inbuff, (byte*)keys, 16 * 2); //decrypt AES & CMAC key to temp buffer
				return 0;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inbuff"></param>
		/// <param name="insize"></param>
		/// <returns></returns>
		public int kirk_forge(byte* inbuff, int insize)
		{
		   AES128CMACHeader* header = (AES128CMACHeader*)inbuff;
		   Crypto.AES_ctx cmac_key;
		   var _cmac_header_hash = new byte[16];
		   var _cmac_data_hash = new byte[16];
		   int chk_size,i;

		   fixed (byte* cmac_header_hash = _cmac_header_hash)
		   fixed (byte* cmac_data_hash = _cmac_data_hash)
		   fixed (Crypto.AES_ctx* aes_kirk1_ptr = &_aes_kirk1)
		   {
			   if (!IsKirkInitialized) return KIRK_NOT_INITIALIZED;
			   if (!(header->Mode == KIRK_MODE_CMD1 || header->Mode == KIRK_MODE_CMD2 || header->Mode == KIRK_MODE_CMD3)) return KIRK_INVALID_MODE;
			   if (header->DataSize == 0) return KIRK_DATA_SIZE_ZERO;

			   if (header->Mode == KIRK_MODE_CMD1)
			   {
				   header_keys keys; //0-15 AES key, 16-31 CMAC key

				   Crypto.AES_cbc_decrypt(aes_kirk1_ptr, inbuff, (byte*)&keys, 32); //decrypt AES & CMAC key to temp buffer
				   Crypto.AES_set_key(&cmac_key, keys.CMAC, 128);
				   Crypto.AES_CMAC(&cmac_key, inbuff + 0x60, 0x30, cmac_header_hash);
				   if (Crypto.memcmp(cmac_header_hash, header->CMAC_header_hash, 16) != 0) return KIRK_HEADER_HASH_INVALID;

				   //Make sure data is 16 aligned
				   chk_size = header->DataSize;
				   if ((chk_size % 16) != 0) chk_size += 16 - (chk_size % 16);
				   Crypto.AES_CMAC(&cmac_key, inbuff + 0x60, 0x30 + chk_size + header->DataOffset, cmac_data_hash);

				   if (Crypto.memcmp(cmac_data_hash, header->CMAC_data_hash, 16) != 0)
				   {
					   //printf("data hash invalid, correcting...\n");
				   }
				   else
				   {
					   Console.Error.WriteLine("data hash is already valid!");
					   return 100;
				   }
				   // Forge collision for data hash
				   Crypto.memcpy(cmac_data_hash, header->CMAC_data_hash, 0x10);
				   Crypto.AES_CMAC_forge(&cmac_key, inbuff + 0x60, 0x30 + chk_size + header->DataOffset, cmac_data_hash);
				   //printf("Last row in bad file should be :\n"); for(i=0;i<0x10;i++) printf("%02x", cmac_data_hash[i]);
				   //printf("\n\n");

				   return KIRK_OPERATION_SUCCESS;
			   }
			   return KIRK_SIG_CHECK_INVALID; //Checks for cmd 2 & 3 not included right now
		   }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Out"></param>
		/// <param name="In"></param>
		/// <param name="insize"></param>
		/// <returns></returns>
		public ResultEnum kirk_CMD5(byte* Out, byte* In, int insize)
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
		public ResultEnum executeKIRKCmd8(byte* Out, byte* In, int insize)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Out"></param>
		/// <param name="OutSize"></param>
		/// <param name="In"></param>
		/// <param name="InSize"></param>
		/// <param name="Command"></param>
		/// <returns></returns>
		public int sceUtilsBufferCopyWithRange(byte* Out, int OutSize, byte* In, int InSize, int Command)
		{
			return (int)hleUtilsBufferCopyWithRange(Out, OutSize, In, InSize, (CommandEnum)Command);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Out"></param>
		/// <param name="OutSize"></param>
		/// <param name="In"></param>
		/// <param name="InSize"></param>
		/// <param name="Command"></param>
		/// <param name="DoChecks"></param>
		/// <returns></returns>
		public ResultEnum hleUtilsBufferCopyWithRange(byte* Out, int OutSize, byte* In, int InSize, CommandEnum Command, bool DoChecks = true)
		{
			switch (Command)
			{
				case CommandEnum.PSP_KIRK_CMD_DECRYPT_PRIVATE: return (ResultEnum)kirk_CMD1(Out, In, InSize, DoChecks);
				case CommandEnum.PSP_KIRK_CMD_ENCRYPT: return (ResultEnum)kirk_CMD4(Out, In, InSize);
				case CommandEnum.PSP_KIRK_CMD_ENCRYPT_FUSE: return kirk_CMD5(Out, In, InSize);
				case CommandEnum.PSP_KIRK_CMD_DECRYPT: return (ResultEnum)kirk_CMD7(Out, In, InSize);
				case CommandEnum.PSP_KIRK_CMD_DECRYPT_FUSE: return executeKIRKCmd8(Out, In, InSize);
				case CommandEnum.PSP_KIRK_CMD_PRIV_SIG_CHECK: return (ResultEnum)kirk_CMD10(In, InSize);
				case CommandEnum.PSP_KIRK_CMD_SHA1_HASH: return (ResultEnum)KirkSha1(Out, In, InSize);
				case CommandEnum.PSP_KIRK_CMD_ECDSA_GEN_KEYS: return executeKIRKCmd12(Out, OutSize);
				case CommandEnum.PSP_KIRK_CMD_ECDSA_MULTIPLY_POINT: return executeKIRKCmd13(Out, OutSize, In, InSize);
				case CommandEnum.PSP_KIRK_CMD_PRNG: return (ResultEnum)kirk_CMD14(Out, InSize);
				case CommandEnum.PSP_KIRK_CMD_ECDSA_SIGN: return executeKIRKCmd16(Out, OutSize, In, InSize);
				case CommandEnum.PSP_KIRK_CMD_ECDSA_VERIFY: return executeKIRKCmd17(In, InSize);
				default: return ResultEnum.PSP_KIRK_INVALID_OPERATION; // Dummy.
			}
		}
	}
}
