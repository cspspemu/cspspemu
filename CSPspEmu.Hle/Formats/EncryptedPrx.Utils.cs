using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Formats
{
	unsafe public partial class EncryptedPrx
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
			public uint tag;

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
		public struct TAG_INFO_OLD
		{
			/// <summary>
			/// 4 byte value at offset 0xD0 in the PRX file
			/// </summary>
			public uint tag;

			/// <summary>
			/// 144 bytes keys
			/// </summary>
			private uint[] _key;

			public uint[] key
			{
				set
				{
					_key = new uint[144];
					for (int i = 0; i < value.Length; i++) {
						_key[i * 4 + 3] = ((value[i] >> 24) & 0xFF);
						_key[i * 4 + 2] = ((value[i] >> 16) & 0xFF);
						_key[i * 4 + 1] = ((value[i] >> 8) & 0xFF);
						_key[i * 4 + 0] = (value[i] & 0xFF);
					}
				}
				get
				{
					return _key;
				}
			}
			
			/// <summary>
			/// code for scramble
			/// </summary>
			public int code;

			/// <summary>
			/// code extra for scramble
			/// </summary>
			public uint codeExtra;
		}

		static public readonly uint[] g_key0 = new uint[36] {
			0x7b21f3be, 0x299c5e1d, 0x1c9c5e71, 0x96cb4645, 0x3c9b1be0, 0xeb85de3d,
			0x4a7f2022, 0xc2206eaa, 0xd50b3265, 0x55770567, 0x3c080840, 0x981d55f2,
			0x5fd8f6f3, 0xee8eb0c5, 0x944d8152, 0xf8278651, 0x2705bafa, 0x8420e533,
			0x27154ae9, 0x4819aa32, 0x59a3aa40, 0x2cb3cf65, 0xf274466d, 0x3a655605,
			0x21b0f88f, 0xc5b18d26, 0x64c19051, 0xd669c94e, 0xe87035f2, 0x9d3a5909,
			0x6f4e7102, 0xdca946ce, 0x8416881b, 0xbab097a5, 0x249125c6, 0xb34c0872,
		};

		static public readonly uint[] g_keyEBOOT1xx = new uint[36] {
			0x18CB69EF, 0x158E8912, 0xDEF90EBB, 0x4CB0FB23, 0x3687EE18, 0x868D4A6E,
			0x19B5C756, 0xEE16551D, 0xE7CB2D6C, 0x9747C660, 0xCE95143F, 0x2956F477,
			0x03824ADE, 0x210C9DF1, 0x5029EB24, 0x81DFE69F, 0x39C89B00, 0xB00C8B91,
			0xEF2DF9C2, 0xE13A93FC, 0x8B94A4A8, 0x491DD09D, 0x686A400D, 0xCED4C7E4,
			0x96C8B7C9, 0x1EAADC28, 0xA4170B84, 0x505D5DDC, 0x5DA6C3CF, 0x0E5DFA2D,
			0x6E7919B5, 0xCE5E29C7, 0xAAACDB94, 0x45F70CDD, 0x62A73725, 0xCCE6563D
		};

		static public readonly List<TAG_INFO_OLD> g_oldTagInfo = new List<TAG_INFO_OLD>()
		{
			new TAG_INFO_OLD() { tag = 0x00000000, key = g_key0, code = 0x42 },
			new TAG_INFO_OLD() { tag = 0x08000000, key = g_keyEBOOT1xx, code = 0x4B },
		};
	}
}
