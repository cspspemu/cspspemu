//#define FULL_UNROLL

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Crypto
{
	unsafe public partial class Crypto
	{
#if false
		//#define GETuint(pt) (((uint)(pt)[0] << 24) ^ ((uint)(pt)[1] << 16) ^ ((uint)(pt)[2] <<  8) ^ ((uint)(pt)[3]))
		//#define PUTuint(ct, st) { (ct)[0] = (u8)((st) >> 24); (ct)[1] = (u8)((st) >> 16); (ct)[2] = (u8)((st) >>  8); (ct)[3] = (u8)(st); }

		static private uint GETuint(byte[] pt) {
			return (((uint)(pt)[0] << 24) ^ ((uint)(pt)[1] << 16) ^ ((uint)(pt)[2] <<  8) ^ ((uint)(pt)[3]));
		}
#endif

		static private uint GETuint(byte* pt) {
			return (((uint)(pt)[0] << 24) ^ ((uint)(pt)[1] << 16) ^ ((uint)(pt)[2] <<  8) ^ ((uint)(pt)[3]));
		}

		//memcpy(block_buff, src, 16);
		static public void memcpy(byte* dst, byte* src, int count)
		{
			PointerUtils.Memcpy(dst, src, count);
		}

		static public int memcmp(byte* str1, byte* str2, int count)
		{
			for (int n = 0; n < count; n++)
			{
				byte c1 = str1[n];
				byte c2 = str2[n];
				if (c1 > c2) return -1;
				if (c1 < c2) return +1;
			}
			return 0;
		}


		static public void memcpy(void* dst, void* src, int count)
		{
			memcpy((byte*)dst, (byte*)src, count);
		}

#if false
		static private void PUTuint(byte[] ct, uint st) {
			(ct)[0] = (byte)((st) >> 24);
			(ct)[1] = (byte)((st) >> 16);
			(ct)[2] = (byte)((st) >>  8);
			(ct)[3] = (byte)(st);
		}
#else

		static private void PUTuint(byte* ct, uint st) {
			(ct)[0] = (byte)((st) >> 24);
			(ct)[1] = (byte)((st) >> 16);
			(ct)[2] = (byte)((st) >>  8);
			(ct)[3] = (byte)(st);
		}
#endif

		/// <summary>
		/// Expand the cipher key into the encryption key schedule.
		/// </summary>
		/// <param name="rk"></param>
		/// <param name="cipherKey"></param>
		/// <param name="keyBits"></param>
		/// <returns>the number of rounds for the given cipher key size.</returns>
		static public int rijndaelKeySetupEnc(uint* rk /*4*(Nr + 1)*/, byte* cipherKey, int keyBits)
		{
			int i = 0;
			uint temp;

			rk[0] = GETuint(cipherKey     );
			rk[1] = GETuint(cipherKey +  4);
			rk[2] = GETuint(cipherKey +  8);
			rk[3] = GETuint(cipherKey + 12);
			if (keyBits == 128) {
				for (;;) {
					temp  = rk[3];
					rk[4] = rk[0] ^
						(Te4[(temp >> 16) & 0xff] & 0xff000000) ^
						(Te4[(temp >>  8) & 0xff] & 0x00ff0000) ^
						(Te4[(temp      ) & 0xff] & 0x0000ff00) ^
						(Te4[(temp >> 24)       ] & 0x000000ff) ^
						rcon[i];
					rk[5] = rk[1] ^ rk[4];
					rk[6] = rk[2] ^ rk[5];
					rk[7] = rk[3] ^ rk[6];
					if (++i == 10) {
						return 10;
					}
					rk += 4;
				}
			}
			rk[4] = GETuint(cipherKey + 16);
			rk[5] = GETuint(cipherKey + 20);
			if (keyBits == 192) {
				for (;;) {
					temp = rk[ 5];
					rk[ 6] = rk[ 0] ^
						(Te4[(temp >> 16) & 0xff] & 0xff000000) ^
						(Te4[(temp >>  8) & 0xff] & 0x00ff0000) ^
						(Te4[(temp      ) & 0xff] & 0x0000ff00) ^
						(Te4[(temp >> 24)       ] & 0x000000ff) ^
						rcon[i];
					rk[ 7] = rk[ 1] ^ rk[ 6];
					rk[ 8] = rk[ 2] ^ rk[ 7];
					rk[ 9] = rk[ 3] ^ rk[ 8];
					if (++i == 8) {
						return 12;
					}
					rk[10] = rk[ 4] ^ rk[ 9];
					rk[11] = rk[ 5] ^ rk[10];
					rk += 6;
				}
			}
			rk[6] = GETuint(cipherKey + 24);
			rk[7] = GETuint(cipherKey + 28);
			if (keyBits == 256) {
				for (;;) {
					temp = rk[ 7];
					rk[ 8] = rk[ 0] ^
						(Te4[(temp >> 16) & 0xff] & 0xff000000) ^
						(Te4[(temp >>  8) & 0xff] & 0x00ff0000) ^
						(Te4[(temp      ) & 0xff] & 0x0000ff00) ^
						(Te4[(temp >> 24)       ] & 0x000000ff) ^
						rcon[i];
					rk[ 9] = rk[ 1] ^ rk[ 8];
					rk[10] = rk[ 2] ^ rk[ 9];
					rk[11] = rk[ 3] ^ rk[10];
					if (++i == 7) {
						return 14;
					}
					temp = rk[11];
					rk[12] = rk[ 4] ^
						(Te4[(temp >> 24)       ] & 0xff000000) ^
						(Te4[(temp >> 16) & 0xff] & 0x00ff0000) ^
						(Te4[(temp >>  8) & 0xff] & 0x0000ff00) ^
						(Te4[(temp      ) & 0xff] & 0x000000ff);
					rk[13] = rk[ 5] ^ rk[12];
					rk[14] = rk[ 6] ^ rk[13];
						rk[15] = rk[ 7] ^ rk[14];
					rk += 8;
				}
			}
			return 0;
		}

		/// <summary>
		/// Expand the cipher key into the decryption key schedule.
		/// </summary>
		/// <param name="rk"></param>
		/// <param name="?"></param>
		/// <returns>the number of rounds for the given cipher key size.</returns>
		static public int rijndaelKeySetupDec(uint *rk /*4*(Nr + 1)*/, byte* cipherKey, int keyBits)
		{
			int Nr, i, j;
			uint temp;

			/* expand the cipher key: */
			Nr = rijndaelKeySetupEnc(rk, cipherKey, keyBits);

			/* invert the order of the round keys: */
			for (i = 0, j = 4*Nr; i < j; i += 4, j -= 4) {
				temp = rk[i    ]; rk[i    ] = rk[j    ]; rk[j    ] = temp;
				temp = rk[i + 1]; rk[i + 1] = rk[j + 1]; rk[j + 1] = temp;
				temp = rk[i + 2]; rk[i + 2] = rk[j + 2]; rk[j + 2] = temp;
				temp = rk[i + 3]; rk[i + 3] = rk[j + 3]; rk[j + 3] = temp;
			}
			/* apply the inverse MixColumn transform to all round keys but the first and the last: */
			for (i = 1; i < Nr; i++) {
				rk += 4;
				rk[0] =
					Td0[Te4[(rk[0] >> 24)       ] & 0xff] ^
					Td1[Te4[(rk[0] >> 16) & 0xff] & 0xff] ^
					Td2[Te4[(rk[0] >>  8) & 0xff] & 0xff] ^
					Td3[Te4[(rk[0]      ) & 0xff] & 0xff];
				rk[1] =
					Td0[Te4[(rk[1] >> 24)       ] & 0xff] ^
					Td1[Te4[(rk[1] >> 16) & 0xff] & 0xff] ^
					Td2[Te4[(rk[1] >>  8) & 0xff] & 0xff] ^
					Td3[Te4[(rk[1]      ) & 0xff] & 0xff];
				rk[2] =
					Td0[Te4[(rk[2] >> 24)       ] & 0xff] ^
					Td1[Te4[(rk[2] >> 16) & 0xff] & 0xff] ^
					Td2[Te4[(rk[2] >>  8) & 0xff] & 0xff] ^
					Td3[Te4[(rk[2]      ) & 0xff] & 0xff];
				rk[3] =
					Td0[Te4[(rk[3] >> 24)       ] & 0xff] ^
					Td1[Te4[(rk[3] >> 16) & 0xff] & 0xff] ^
					Td2[Te4[(rk[3] >>  8) & 0xff] & 0xff] ^
					Td3[Te4[(rk[3]      ) & 0xff] & 0xff];
			}
			return Nr;
		}

		static public void rijndaelEncrypt(uint *rk /*4*(Nr + 1)*/, int Nr, byte* pt /*16*/, byte* ct /*[16]*/) {
			uint s0, s1, s2, s3, t0, t1, t2, t3;
		#if !FULL_UNROLL
			int r;
		#endif

			// map byte array block to cipher state
			// and add initial round key:
			s0 = GETuint(pt     ) ^ rk[0];
			s1 = GETuint(pt +  4) ^ rk[1];
			s2 = GETuint(pt +  8) ^ rk[2];
			s3 = GETuint(pt + 12) ^ rk[3];
		#if FULL_UNROLL
			// round 1:
			t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >>  8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[ 4];
			t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >>  8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[ 5];
			t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >>  8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[ 6];
			t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >>  8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[ 7];
			// round 2:
			s0 = Te0[t0 >> 24] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >>  8) & 0xff] ^ Te3[t3 & 0xff] ^ rk[ 8];
			s1 = Te0[t1 >> 24] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >>  8) & 0xff] ^ Te3[t0 & 0xff] ^ rk[ 9];
			s2 = Te0[t2 >> 24] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >>  8) & 0xff] ^ Te3[t1 & 0xff] ^ rk[10];
			s3 = Te0[t3 >> 24] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >>  8) & 0xff] ^ Te3[t2 & 0xff] ^ rk[11];
			// round 3:
			t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >>  8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[12];
			t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >>  8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[13];
			t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >>  8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[14];
			t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >>  8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[15];
			// round 4:
			s0 = Te0[t0 >> 24] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >>  8) & 0xff] ^ Te3[t3 & 0xff] ^ rk[16];
			s1 = Te0[t1 >> 24] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >>  8) & 0xff] ^ Te3[t0 & 0xff] ^ rk[17];
			s2 = Te0[t2 >> 24] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >>  8) & 0xff] ^ Te3[t1 & 0xff] ^ rk[18];
			s3 = Te0[t3 >> 24] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >>  8) & 0xff] ^ Te3[t2 & 0xff] ^ rk[19];
			// round 5:
			t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >>  8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[20];
			t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >>  8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[21];
			t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >>  8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[22];
			t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >>  8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[23];
			// round 6:
			s0 = Te0[t0 >> 24] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >>  8) & 0xff] ^ Te3[t3 & 0xff] ^ rk[24];
			s1 = Te0[t1 >> 24] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >>  8) & 0xff] ^ Te3[t0 & 0xff] ^ rk[25];
			s2 = Te0[t2 >> 24] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >>  8) & 0xff] ^ Te3[t1 & 0xff] ^ rk[26];
			s3 = Te0[t3 >> 24] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >>  8) & 0xff] ^ Te3[t2 & 0xff] ^ rk[27];
			// round 7:
			t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >>  8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[28];
			t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >>  8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[29];
			t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >>  8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[30];
			t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >>  8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[31];
			// round 8:
			s0 = Te0[t0 >> 24] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >>  8) & 0xff] ^ Te3[t3 & 0xff] ^ rk[32];
			s1 = Te0[t1 >> 24] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >>  8) & 0xff] ^ Te3[t0 & 0xff] ^ rk[33];
			s2 = Te0[t2 >> 24] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >>  8) & 0xff] ^ Te3[t1 & 0xff] ^ rk[34];
			s3 = Te0[t3 >> 24] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >>  8) & 0xff] ^ Te3[t2 & 0xff] ^ rk[35];
			// round 9:
			t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >>  8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[36];
			t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >>  8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[37];
			t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >>  8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[38];
			t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >>  8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[39];
			if (Nr > 10) {
			// round 10:
			s0 = Te0[t0 >> 24] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >>  8) & 0xff] ^ Te3[t3 & 0xff] ^ rk[40];
			s1 = Te0[t1 >> 24] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >>  8) & 0xff] ^ Te3[t0 & 0xff] ^ rk[41];
			s2 = Te0[t2 >> 24] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >>  8) & 0xff] ^ Te3[t1 & 0xff] ^ rk[42];
			s3 = Te0[t3 >> 24] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >>  8) & 0xff] ^ Te3[t2 & 0xff] ^ rk[43];
			// round 11:
			t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >>  8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[44];
			t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >>  8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[45];
			t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >>  8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[46];
			t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >>  8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[47];
			if (Nr > 12) {
				// round 12:
				s0 = Te0[t0 >> 24] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >>  8) & 0xff] ^ Te3[t3 & 0xff] ^ rk[48];
				s1 = Te0[t1 >> 24] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >>  8) & 0xff] ^ Te3[t0 & 0xff] ^ rk[49];
				s2 = Te0[t2 >> 24] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >>  8) & 0xff] ^ Te3[t1 & 0xff] ^ rk[50];
				s3 = Te0[t3 >> 24] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >>  8) & 0xff] ^ Te3[t2 & 0xff] ^ rk[51];
				// round 13:
				t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >>  8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[52];
				t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >>  8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[53];
				t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >>  8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[54];
				t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >>  8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[55];
			}
			}
			rk += Nr << 2;
		#else
			// Nr - 1 full rounds:
			r = Nr >> 1;
			for (;;) {
			t0 =
				Te0[(s0 >> 24)       ] ^
				Te1[(s1 >> 16) & 0xff] ^
				Te2[(s2 >>  8) & 0xff] ^
				Te3[(s3      ) & 0xff] ^
				rk[4];
			t1 =
				Te0[(s1 >> 24)       ] ^
				Te1[(s2 >> 16) & 0xff] ^
				Te2[(s3 >>  8) & 0xff] ^
				Te3[(s0      ) & 0xff] ^
				rk[5];
			t2 =
				Te0[(s2 >> 24)       ] ^
				Te1[(s3 >> 16) & 0xff] ^
				Te2[(s0 >>  8) & 0xff] ^
				Te3[(s1      ) & 0xff] ^
				rk[6];
			t3 =
				Te0[(s3 >> 24)       ] ^
				Te1[(s0 >> 16) & 0xff] ^
				Te2[(s1 >>  8) & 0xff] ^
				Te3[(s2      ) & 0xff] ^
				rk[7];

			rk += 8;
			if (--r == 0) {
				break;
			}

			s0 =
				Te0[(t0 >> 24)       ] ^
				Te1[(t1 >> 16) & 0xff] ^
				Te2[(t2 >>  8) & 0xff] ^
				Te3[(t3      ) & 0xff] ^
				rk[0];
			s1 =
				Te0[(t1 >> 24)       ] ^
				Te1[(t2 >> 16) & 0xff] ^
				Te2[(t3 >>  8) & 0xff] ^
				Te3[(t0      ) & 0xff] ^
				rk[1];
			s2 =
				Te0[(t2 >> 24)       ] ^
				Te1[(t3 >> 16) & 0xff] ^
				Te2[(t0 >>  8) & 0xff] ^
				Te3[(t1      ) & 0xff] ^
				rk[2];
			s3 =
				Te0[(t3 >> 24)       ] ^
				Te1[(t0 >> 16) & 0xff] ^
				Te2[(t1 >>  8) & 0xff] ^
				Te3[(t2      ) & 0xff] ^
				rk[3];
			}
		#endif
			// apply last round and
			// map cipher state to byte array block:
			s0 =
				(Te4[(t0 >> 24)       ] & 0xff000000) ^
				(Te4[(t1 >> 16) & 0xff] & 0x00ff0000) ^
				(Te4[(t2 >>  8) & 0xff] & 0x0000ff00) ^
				(Te4[(t3      ) & 0xff] & 0x000000ff) ^
				rk[0];
			PUTuint(ct     , s0);
			s1 =
				(Te4[(t1 >> 24)       ] & 0xff000000) ^
				(Te4[(t2 >> 16) & 0xff] & 0x00ff0000) ^
				(Te4[(t3 >>  8) & 0xff] & 0x0000ff00) ^
				(Te4[(t0      ) & 0xff] & 0x000000ff) ^
				rk[1];
			PUTuint(ct +  4, s1);
			s2 =
				(Te4[(t2 >> 24)       ] & 0xff000000) ^
				(Te4[(t3 >> 16) & 0xff] & 0x00ff0000) ^
				(Te4[(t0 >>  8) & 0xff] & 0x0000ff00) ^
				(Te4[(t1      ) & 0xff] & 0x000000ff) ^
				rk[2];
			PUTuint(ct +  8, s2);
			s3 =
				(Te4[(t3 >> 24)       ] & 0xff000000) ^
				(Te4[(t0 >> 16) & 0xff] & 0x00ff0000) ^
				(Te4[(t1 >>  8) & 0xff] & 0x0000ff00) ^
				(Te4[(t2      ) & 0xff] & 0x000000ff) ^
				rk[3];
			PUTuint(ct + 12, s3);
		}

		static public void rijndaelDecrypt(uint* rk /*4*(Nr + 1)*/, int Nr, byte* ct, byte *pt)
		{
			uint s0, s1, s2, s3, t0, t1, t2, t3;
		#if !FULL_UNROLL
			int r;
		#endif

			// map byte array block to cipher state
			// and add initial round key:
			s0 = GETuint(ct     ) ^ rk[0];
			s1 = GETuint(ct +  4) ^ rk[1];
			s2 = GETuint(ct +  8) ^ rk[2];
			s3 = GETuint(ct + 12) ^ rk[3];
		#if FULL_UNROLL
			/* round 1: */
			t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >>  8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[ 4];
			t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >>  8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[ 5];
			t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >>  8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[ 6];
			t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >>  8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[ 7];
			/* round 2: */
			s0 = Td0[t0 >> 24] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >>  8) & 0xff] ^ Td3[t1 & 0xff] ^ rk[ 8];
			s1 = Td0[t1 >> 24] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >>  8) & 0xff] ^ Td3[t2 & 0xff] ^ rk[ 9];
			s2 = Td0[t2 >> 24] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >>  8) & 0xff] ^ Td3[t3 & 0xff] ^ rk[10];
			s3 = Td0[t3 >> 24] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >>  8) & 0xff] ^ Td3[t0 & 0xff] ^ rk[11];
			/* round 3: */
			t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >>  8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[12];
			t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >>  8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[13];
			t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >>  8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[14];
			t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >>  8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[15];
			/* round 4: */
			s0 = Td0[t0 >> 24] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >>  8) & 0xff] ^ Td3[t1 & 0xff] ^ rk[16];
			s1 = Td0[t1 >> 24] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >>  8) & 0xff] ^ Td3[t2 & 0xff] ^ rk[17];
			s2 = Td0[t2 >> 24] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >>  8) & 0xff] ^ Td3[t3 & 0xff] ^ rk[18];
			s3 = Td0[t3 >> 24] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >>  8) & 0xff] ^ Td3[t0 & 0xff] ^ rk[19];
			/* round 5: */
			t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >>  8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[20];
			t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >>  8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[21];
			t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >>  8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[22];
			t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >>  8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[23];
			/* round 6: */
			s0 = Td0[t0 >> 24] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >>  8) & 0xff] ^ Td3[t1 & 0xff] ^ rk[24];
			s1 = Td0[t1 >> 24] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >>  8) & 0xff] ^ Td3[t2 & 0xff] ^ rk[25];
			s2 = Td0[t2 >> 24] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >>  8) & 0xff] ^ Td3[t3 & 0xff] ^ rk[26];
			s3 = Td0[t3 >> 24] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >>  8) & 0xff] ^ Td3[t0 & 0xff] ^ rk[27];
			/* round 7: */
			t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >>  8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[28];
			t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >>  8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[29];
			t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >>  8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[30];
			t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >>  8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[31];
			/* round 8: */
			s0 = Td0[t0 >> 24] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >>  8) & 0xff] ^ Td3[t1 & 0xff] ^ rk[32];
			s1 = Td0[t1 >> 24] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >>  8) & 0xff] ^ Td3[t2 & 0xff] ^ rk[33];
			s2 = Td0[t2 >> 24] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >>  8) & 0xff] ^ Td3[t3 & 0xff] ^ rk[34];
			s3 = Td0[t3 >> 24] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >>  8) & 0xff] ^ Td3[t0 & 0xff] ^ rk[35];
			/* round 9: */
			t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >>  8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[36];
			t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >>  8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[37];
			t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >>  8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[38];
			t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >>  8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[39];
			if (Nr > 10) {
			/* round 10: */
			s0 = Td0[t0 >> 24] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >>  8) & 0xff] ^ Td3[t1 & 0xff] ^ rk[40];
			s1 = Td0[t1 >> 24] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >>  8) & 0xff] ^ Td3[t2 & 0xff] ^ rk[41];
			s2 = Td0[t2 >> 24] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >>  8) & 0xff] ^ Td3[t3 & 0xff] ^ rk[42];
			s3 = Td0[t3 >> 24] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >>  8) & 0xff] ^ Td3[t0 & 0xff] ^ rk[43];
			/* round 11: */
			t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >>  8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[44];
			t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >>  8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[45];
			t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >>  8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[46];
			t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >>  8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[47];
			if (Nr > 12) {
				/* round 12: */
				s0 = Td0[t0 >> 24] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >>  8) & 0xff] ^ Td3[t1 & 0xff] ^ rk[48];
				s1 = Td0[t1 >> 24] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >>  8) & 0xff] ^ Td3[t2 & 0xff] ^ rk[49];
				s2 = Td0[t2 >> 24] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >>  8) & 0xff] ^ Td3[t3 & 0xff] ^ rk[50];
				s3 = Td0[t3 >> 24] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >>  8) & 0xff] ^ Td3[t0 & 0xff] ^ rk[51];
				/* round 13: */
				t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >>  8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[52];
				t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >>  8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[53];
				t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >>  8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[54];
				t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >>  8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[55];
			}
			}
			rk += Nr << 2;
		#else
			/*
			 * Nr - 1 full rounds:
			 */
			r = Nr >> 1;
			for (;;) {
			t0 =
				Td0[(s0 >> 24)       ] ^
				Td1[(s3 >> 16) & 0xff] ^
				Td2[(s2 >>  8) & 0xff] ^
				Td3[(s1      ) & 0xff] ^
				rk[4];
			t1 =
				Td0[(s1 >> 24)       ] ^
				Td1[(s0 >> 16) & 0xff] ^
				Td2[(s3 >>  8) & 0xff] ^
				Td3[(s2      ) & 0xff] ^
				rk[5];
			t2 =
				Td0[(s2 >> 24)       ] ^
				Td1[(s1 >> 16) & 0xff] ^
				Td2[(s0 >>  8) & 0xff] ^
				Td3[(s3      ) & 0xff] ^
				rk[6];
			t3 =
				Td0[(s3 >> 24)       ] ^
				Td1[(s2 >> 16) & 0xff] ^
				Td2[(s1 >>  8) & 0xff] ^
				Td3[(s0      ) & 0xff] ^
				rk[7];

			rk += 8;
			if (--r == 0) {
				break;
			}

			s0 =
				Td0[(t0 >> 24)       ] ^
				Td1[(t3 >> 16) & 0xff] ^
				Td2[(t2 >>  8) & 0xff] ^
				Td3[(t1      ) & 0xff] ^
				rk[0];
			s1 =
				Td0[(t1 >> 24)       ] ^
				Td1[(t0 >> 16) & 0xff] ^
				Td2[(t3 >>  8) & 0xff] ^
				Td3[(t2      ) & 0xff] ^
				rk[1];
			s2 =
				Td0[(t2 >> 24)       ] ^
				Td1[(t1 >> 16) & 0xff] ^
				Td2[(t0 >>  8) & 0xff] ^
				Td3[(t3      ) & 0xff] ^
				rk[2];
			s3 =
				Td0[(t3 >> 24)       ] ^
				Td1[(t2 >> 16) & 0xff] ^
				Td2[(t1 >>  8) & 0xff] ^
				Td3[(t0      ) & 0xff] ^
				rk[3];
			}
		#endif
			/*
			 * apply last round and
			 * map cipher state to byte array block:
			 */
			s0 =
				(Td4[(t0 >> 24)       ] & 0xff000000) ^
				(Td4[(t3 >> 16) & 0xff] & 0x00ff0000) ^
				(Td4[(t2 >>  8) & 0xff] & 0x0000ff00) ^
				(Td4[(t1      ) & 0xff] & 0x000000ff) ^
				rk[0];
			PUTuint(pt     , s0);
			s1 =
				(Td4[(t1 >> 24)       ] & 0xff000000) ^
				(Td4[(t0 >> 16) & 0xff] & 0x00ff0000) ^
				(Td4[(t3 >>  8) & 0xff] & 0x0000ff00) ^
				(Td4[(t2      ) & 0xff] & 0x000000ff) ^
				rk[1];
			PUTuint(pt +  4, s1);
			s2 =
				(Td4[(t2 >> 24)       ] & 0xff000000) ^
				(Td4[(t1 >> 16) & 0xff] & 0x00ff0000) ^
				(Td4[(t0 >>  8) & 0xff] & 0x0000ff00) ^
				(Td4[(t3      ) & 0xff] & 0x000000ff) ^
				rk[2];
			PUTuint(pt +  8, s2);
			s3 =
				(Td4[(t3 >> 24)       ] & 0xff000000) ^
				(Td4[(t2 >> 16) & 0xff] & 0x00ff0000) ^
				(Td4[(t1 >>  8) & 0xff] & 0x0000ff00) ^
				(Td4[(t0      ) & 0xff] & 0x000000ff) ^
				rk[3];
			PUTuint(pt + 12, s3);
		}

		/* setup key context for encryption only */
		static public int rijndael_set_key_enc_only(rijndael_ctx* ctx, byte* key, int bits)
		{
			int rounds;

			rounds = rijndaelKeySetupEnc(ctx->ek, key, bits);
			if (rounds == 0)
				return -1;

			ctx->Nr = rounds;
			ctx->enc_only = 1;

			return 0;
		}

		/* setup key context for both encryption and decryption */
		static public int rijndael_set_key(rijndael_ctx *ctx, byte *key, int bits)
		{
			int rounds;

			rounds = rijndaelKeySetupEnc(ctx->ek, key, bits);
			if (rounds == 0)
				return -1;
			if (rijndaelKeySetupDec(ctx->dk, key, bits) != rounds)
				return -1;

			ctx->Nr = rounds;
			ctx->enc_only = 0;

			return 0;
		}

		static public void rijndael_decrypt(rijndael_ctx* ctx, byte* src, byte* dst)
		{
			rijndaelDecrypt(ctx->dk, ctx->Nr, src, dst);
		}

		static public void rijndael_encrypt(rijndael_ctx *ctx, byte *src, byte *dst)
		{
			rijndaelEncrypt(ctx->ek, ctx->Nr, src, dst);
		}

		static public int AES_set_key(AES_ctx *ctx, byte *key, int bits)
		{
			return rijndael_set_key((rijndael_ctx *)ctx, key, bits);
		}

		static public void AES_decrypt(AES_ctx* ctx, byte* src, byte* dst)
		{
			rijndaelDecrypt(ctx->dk, ctx->Nr, src, dst);
		}

		static public void AES_encrypt(AES_ctx* ctx, byte* src, byte* dst)
		{
			rijndaelEncrypt(ctx->ek, ctx->Nr, src, dst);
		}

		static private void xor_128(byte *a, byte *b, byte *Out)
		{
			for (int i=0;i<16; i++)
			{
				Out[i] = (byte)(a[i] ^ b[i]);
			}
		}

		//No IV support!
		static public void AES_cbc_encrypt(AES_ctx* ctx, byte* src, byte* dst, int size)
		{
			var _block_buff = new byte[16];
			fixed (byte* block_buff = _block_buff)
			{
				int i;
				for(i = 0; i < size; i+=16)
				{
					//step 1: copy block to dst
					memcpy(dst, src, 16);
					//step 2: XOR with previous block
					if(i != 0) xor_128(dst, block_buff, dst);
					//step 3: encrypt the block -> it land in block buffer
					AES_encrypt(ctx, dst, block_buff);
					//step 4: copy back the encrypted block to destination
					memcpy(dst, block_buff, 16);
		
					dst += 16;
					src += 16;
				}
			}
		}

		static public void AES_cbc_decrypt(AES_ctx* ctx, byte* src, byte* dst, int size)
		{
			var _block_buff = new byte[16];
			var _block_buff_previous = new byte[16];
			fixed (byte* block_buff = _block_buff)
			fixed (byte* block_buff_previous = _block_buff_previous)
			{
				memcpy(block_buff, src, 16);
				memcpy(block_buff_previous, src, 16);
				AES_decrypt(ctx, src, dst);
	
				dst += 16;
				src += 16;
	
				int i;
				for(i = 16; i < size; i+=16)
				{
					//step1: backup current block for next block decrypt
					memcpy(block_buff, src, 16);
					//step2: copy current block to destination
					memcpy(dst, src, 16);
					//step3: decrypt current buffer in place
					AES_decrypt(ctx, dst, dst);
					//step4: XOR current buffer with previous buffer
					xor_128(dst, block_buff_previous, dst);
					//step5: swap buffers
					memcpy(block_buff_previous, block_buff, 16);
		
					dst += 16;
					src += 16;
				}
			}
		}

		/* AES-CMAC Generation Function */

		static public void leftshift_onebit(byte* input, byte* output)
		{
			int i;
			byte overflow = 0;

			for ( i=15; i>=0; i-- ) 
			{
				output[i] = (byte)(input[i] << 1);
				output[i] |= overflow;
				overflow = (byte)(((input[i] & 0x80) != 0) ? 1 : 0);
			}
		}

		static public void generate_subkey(AES_ctx* ctx, byte* K1, byte* K2)
		{
			var _L = new byte[16];
			var _Z = new byte[16];
			var _tmp = new byte[16];
			int i;

			fixed (byte* tmp = _tmp)
			fixed (byte* L = _L)
			fixed (byte* Z = _Z)
			fixed (byte* const_Rb = _const_Rb)
			{
				for ( i=0; i<16; i++ ) Z[i] = 0;
	
				AES_encrypt(ctx, Z, L);

				if ( (L[0] & 0x80) == 0 ) /* If MSB(L) = 0, then K1 = L << 1 */
				{
					leftshift_onebit(L,K1);
				} else {    /* Else K1 = ( L << 1 ) (+) Rb */
					leftshift_onebit(L,tmp);
					xor_128(tmp,const_Rb,K1);
				}

				if ( (K1[0] & 0x80) == 0 ) 
				{
					leftshift_onebit(K1,K2);
				} else {
					leftshift_onebit(K1,tmp);
					xor_128(tmp,const_Rb,K2);
				}
			}
		}

		static public void padding(byte* lastb, byte* pad, int length)
		{
			int j;
	
			/* original last block */
			for ( j=0; j<16; j++ ) 
			{
				if ( j < length ) 
				{
					pad[j] = lastb[j];
				} else if ( j == length ) {
					pad[j] = 0x80;
				} else {
					pad[j] = 0x00;
				}
			}
		}

		static public void AES_CMAC(AES_ctx* ctx, byte* input, int length, byte* mac)
		{
			var _X = new byte[16];
			var _Y = new byte[16];
			var _M_last = new byte[16];
			var _padded = new byte[16];
			var _K1 = new byte[16];
			var _K2 = new byte[16];
			int n, i, flag;

			fixed (byte* X = _X)
			fixed (byte* Y = _Y)
			fixed (byte* M_last = _M_last)
			fixed (byte* padded = _padded)
			fixed (byte* K1 = _K1)
			fixed (byte* K2 = _K2)
			{
				generate_subkey(ctx,K1,K2);

				n = (length+15) / 16;       /* n is number of rounds */

				if ( n == 0 ) 
				{
					n = 1;
					flag = 0;
				} else {
					if ( (length%16) == 0 ) { /* last block is a complete block */
						flag = 1;
					} else { /* last block is not complete block */
						flag = 0;
					}

				}

				if ( flag != 0 ) { /* last block is complete block */
					xor_128(&input[16*(n-1)],K1,M_last);
				} else {
					padding(&input[16*(n-1)],padded,length%16);
					xor_128(padded,K2,M_last);
				}

				for ( i=0; i<16; i++ ) X[i] = 0;
				for ( i=0; i<n-1; i++ ) 
				{
					xor_128(X,&input[16*i],Y); /* Y := Mi (+) X  */
					AES_encrypt(ctx, Y, X); /* X := AES-128(KEY, Y); */ 
				}

				xor_128(X,M_last,Y);
				AES_encrypt(ctx, Y, X);

				for ( i=0; i<16; i++ ) {
					mac[i] = X[i];
				}
			}
		}

		static public void AES_CMAC_forge(AES_ctx* ctx, byte* input, int length, byte* forge)
		{
			var _X = new byte[16];
			var _Y = new byte[16];
			var _M_last = new byte[16];
			var _padded = new byte[16];
			var _K1 = new byte[16];
			var _K2 = new byte[16];
			int n, i, flag;

			fixed (byte* X = _X)
			fixed (byte* Y = _Y)
			fixed (byte* M_last = _M_last)
			fixed (byte* padded = _padded)
			fixed (byte* K1 = _K1)
			fixed (byte* K2 = _K2)
			{
				generate_subkey(ctx,K1,K2);

				n = (length+15) / 16;       /* n is number of rounds */

				if ( n == 0 )
			   {
					n = 1;
					flag = 0;
				} else {
				  if ( (length%16) == 0 ) { /* last block is a complete block */
						flag = 1;
					} else { /* last block is not complete block */
						flag = 0;
					}

				}

				if ( flag != 0 ) { /* last block is complete block */
					xor_128(&input[16*(n-1)],K1,M_last);
				} else {
					padding(&input[16*(n-1)],padded,length%16);
					xor_128(padded,K2,M_last);
				}

				for ( i=0; i<16; i++ ) X[i] = 0;
				for ( i=0; i<n-1; i++ )
				{
					 xor_128(X,&input[16*i],Y); /* Y := Mi (+) X  */
					 AES_encrypt(ctx, Y, X); /* X := AES-128(KEY, Y); */
				}

				xor_128(X,M_last,Y);
				 AES_decrypt(ctx, forge, X);
				//printf("Pre-crypt value: "); for(i=0;i<0x10;i++) printf("%02x", X[i]); printf("\n");
				xor_128(X,Y,forge);
				xor_128(forge,&input[16*(n-1)],Y);
				//AES_encrypt(Y, X, &aes);

				  //Update original input file so it produces the correct CMAC
				for ( i=0; i<16; i++ ) {
					input[(16*(n-1))+i]= Y[i];
				}
			}
		}
	}
}
