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
			public ushort mod_attr;

			/// <summary>
			/// 0006 - 
			/// </summary>
			public ushort comp_mod_attr;

			/// <summary>
			/// 0008 - 
			/// </summary>
			public byte mod_ver_lo;

			/// <summary>
			/// 0009 - 
			/// </summary>
			public byte mod_ver_hi;

			/// <summary>
			/// 000A - 
			/// </summary>
			public fixed byte ModuleName[28];

			/// <summary>
			/// 0026 -
			/// </summary>
			public byte mod_version;

			/// <summary>
			/// 0027 - 
			/// </summary>
			public byte nsegments;

			/// <summary>
			/// 0028 -
			/// </summary>
			public uint elf_size;

			/// <summary>
			/// 002C - 
			/// </summary>
			public uint psp_size;

			/// <summary>
			/// 0030 -
			/// </summary>
			public uint boot_entry;

			/// <summary>
			/// 0034 -
			/// </summary>
			public uint modinfo_offset;

			/// <summary>
			/// 0038 -
			/// </summary>
			public uint bss_size;

			/// <summary>
			/// 003C -
			/// </summary>
			public fixed ushort seg_align[4];

			/// <summary>
			/// 0044 - 
			/// </summary>
			public fixed uint seg_address[4];

			/// <summary>
			/// 0054 -
			/// </summary>
			public fixed uint seg_size[4];

			/// <summary>
			/// 0064 -
			/// </summary>
			public fixed uint reserved[5];

			/// <summary>
			/// 0078 -
			/// </summary>
			public uint devkit_version;

			/// <summary>
			/// 007C -
			/// </summary>
			public byte dec_mode;

			/// <summary>
			/// 007D -
			/// </summary>
			public byte pad;

			/// <summary>
			/// 007E -
			/// </summary>
			public ushort overlap_size;

			/// <summary>
			/// 0080 -
			/// </summary>
			public fixed byte aes_key[16];

			/// <summary>
			/// 0090 -
			/// </summary>
			public fixed byte cmac_key[16];

			/// <summary>
			/// 00A0 -
			/// </summary>
			public fixed byte cmac_header_hash[16];

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
			public uint unk1;

			/// <summary>
			/// 00BC
			/// </summary>
			public uint unk2;

			/// <summary>
			/// 00C0 -
			/// </summary>
			public fixed byte cmac_data_hash[16];

			/// <summary>
			/// 00D0 -
			/// </summary>
			public uint Tag;

			/// <summary>
			/// 00D4 -
			/// </summary>
			public fixed byte sig_check[88];

			/// <summary>
			/// 012C -
			/// </summary>
			public fixed byte sha1_hash[20];

			/// <summary>
			/// 0140 -
			/// </summary>
			public fixed byte key_data[16];
		}

		/// <summary>
		/// 
		/// </summary>
		public class TAG_INFO
		{
			/// <summary>
			/// 4 byte value at offset 0xD0 in the PRX file
			/// </summary>
			public uint tag;

			/// <summary>
			/// 144 bytes keys
			/// </summary>
			public byte[] key;

			/// <summary>
			/// 
			/// </summary>
			public uint[] ikey
			{
				set
				{
					if (value.Length * 4 != 144) throw(new Exception("Invalid entry"));
					key = new byte[144];
					Buffer.BlockCopy(value, 0, key, 0, value.Length * 4);
				}
			}
			
			/// <summary>
			/// code for scramble
			/// </summary>
			public int code;

			/// <summary>
			/// code extra for scramble
			/// </summary>
			public byte codeExtra;

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return CStringFormater.Sprintf("TAG_INFO(tag=0x%08X, key=%s, code=%02X, codeExtra=%02X)", tag, (key != null) ? BitConverter.ToString(key) : "null", code, codeExtra);
			}
		}

		public class TAG_INFO2
		{
			/// <summary>
			/// 4 byte value at offset 0xD0 in the PRX file
			/// </summary>
			public uint tag;

			/// <summary>
			/// 16 bytes keys
			/// </summary>
			public byte[] key;

			/// <summary>
			/// code for scramble
			/// </summary>
			public byte code;

			public override string ToString()
			{
				return CStringFormater.Sprintf("TAG_INFO2(tag=0x%08X, key=%s, code=%02X)", tag, (key != null) ? BitConverter.ToString(key) : "null", code);
			}
		}
	}
}
