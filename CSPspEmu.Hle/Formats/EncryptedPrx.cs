using System;
using System.IO;
using System.Linq;
using CSharpUtils;
using CSPspEmu.Core.Crypto;

namespace CSPspEmu.Hle.Formats
{
	public unsafe partial class EncryptedPrx
	{
		Kirk Kirk;

		/// <summary>
		/// 
		/// </summary>
		public HeaderStruct Header { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EncryptedPrx()
		{
		}

		private TAG_INFO GetTagInfo(uint CheckTag)
		{
			var Result = g_tagInfo.SingleOrDefault(Tag => Tag.tag == CheckTag);
			if (Result == null) throw(new InvalidDataException(String.Format("Can't find tag1 0x{0:X}", CheckTag)));
			return Result;
		}

		private TAG_INFO2 GetTagInfo2(uint CheckTag)
		{
			var Result = g_tagInfo2.SingleOrDefault(Tag => Tag.tag == CheckTag);
			if (Result == null) throw (new InvalidDataException(String.Format("Can't find tag2 0x{0:X}", CheckTag)));
			return Result;
		}

		protected void ExtraV2Mangle(byte* buffer1, byte codeExtra)
		{
			var g_dataTmp = new byte[20 + 0xA0];
			fixed (byte* buffer2 = g_dataTmp) // aligned
			{
				PointerUtils.Memcpy(buffer2 + 20, buffer1, 0xA0);
				var pl2 = (uint*)buffer2;
				pl2[0] = 5;
				pl2[1] = pl2[2] = 0;
				pl2[3] = codeExtra;
				pl2[4] = 0xA0;

				var ret = Kirk.hleUtilsBufferCopyWithRange(
					buffer2,
					20 + 0xA0,
					buffer2,
					20 + 0xA0,
					Kirk.CommandEnum.PSP_KIRK_CMD_DECRYPT
				);

				if (ret != 0)
				{
					throw (new Exception(CStringFormater.Sprintf("extra de-mangle returns %d, ", ret)));
				}
				// copy result back
				PointerUtils.Memcpy(buffer1, buffer2, 0xA0);
			}
		}

		protected byte[] DecryptPRX1(byte[] _pbIn, bool ShowInfo = false)
		{
			int cbTotal = (int)_pbIn.Length;
			var _pbOut = new byte[cbTotal];
			_pbIn.CopyTo(_pbOut, 0);

			fixed (byte* pbIn = _pbIn)
			fixed (byte* pbOut = _pbOut)
			{
				var HeaderPointer = (HeaderStruct*)pbIn;
				this.Header = *(HeaderStruct*)pbIn;
				var pti = GetTagInfo(this.Header.Tag);

				if (ShowInfo)
				{
					Console.WriteLine("TAG_INFO: {0}", pti);
				}

				// build conversion into pbOut
				PointerUtils.Memcpy(pbOut, pbIn, _pbIn.Length);
				PointerUtils.Memset(pbOut, 0, 0x150);
				PointerUtils.Memset(pbOut, 0x55, 0x40);

				// step3 demangle in place
				var h7_header = (Kirk.KIRK_AES128CBC_HEADER*)&pbOut[0x2C];
				h7_header->Mode = Core.Crypto.Kirk.KirkMode.DecryptCbc;
				h7_header->Unknown4 = 0;
				h7_header->Unknown8 = 0;
				h7_header->KeySeed = pti.code; // initial seed for PRX
				h7_header->Datasize = 0x70; // size

				// redo part of the SIG check (step2)
				var _buffer1 = new byte[0x150];
				fixed (byte* buffer1 = _buffer1)
				{
					PointerUtils.Memcpy(buffer1 + 0x00, pbIn + 0xD0, 0x80);
					PointerUtils.Memcpy(buffer1 + 0x80, pbIn + 0x80, 0x50);
					PointerUtils.Memcpy(buffer1 + 0xD0, pbIn + 0x00, 0x80);

					if (pti.codeExtra != 0)
					{
						ExtraV2Mangle(buffer1 + 0x10, pti.codeExtra);
					}

					PointerUtils.Memcpy(pbOut + 0x40 /* 0x2C+20 */, buffer1 + 0x40, 0x40);
				}

				for (int iXOR = 0; iXOR < 0x70; iXOR++)
				{
					pbOut[0x40 + iXOR] = (byte)(pbOut[0x40 + iXOR] ^ pti.key[0x14 + iXOR]);
				}

				var ret = Kirk.hleUtilsBufferCopyWithRange(
					pbOut + 0x2C,
					20 + 0x70,
					pbOut + 0x2C,
					20 + 0x70,
					Kirk.CommandEnum.PSP_KIRK_CMD_DECRYPT
				);

				if (ret != 0)
				{
					throw (new Exception(CStringFormater.Sprintf("mangle#7 returned 0x%08X, ", ret)));
				}

				for (int iXOR = 0x6F; iXOR >= 0; iXOR--)
				{
					pbOut[0x40 + iXOR] = (byte)(pbOut[0x2C + iXOR] ^ pti.key[0x20 + iXOR]);
				}

				PointerUtils.Memset(pbOut + 0x80, 0, 0x30); // $40 bytes kept, clean up

				pbOut[0xA0] = 1;
				// copy unscrambled parts from header
				PointerUtils.Memcpy(pbOut + 0xB0, pbIn + 0xB0, 0x20); // file size + lots of zeros
				PointerUtils.Memcpy(pbOut + 0xD0, pbIn + 0x00, 0x80); // ~PSP header

				// step4: do the actual decryption of code block
				//  point 0x40 bytes into the buffer to key info
				ret = Kirk.hleUtilsBufferCopyWithRange(
					pbOut,
					cbTotal,
					pbOut + 0x40,
					cbTotal - 0x40,
					Kirk.CommandEnum.PSP_KIRK_CMD_DECRYPT_PRIVATE
				);

				if (ret != 0)
				{
					throw (new Exception(CStringFormater.Sprintf("mangle#1 returned 0x%08X", ret)));
				}

				//File.WriteAllBytes("../../../TestInput/temp.bin", _pbOut);

				var OutputSize = *(int*)&pbIn[0xB0];

				return _pbOut.Slice(0, OutputSize).ToArray();
			}
		}

		protected int Scramble(uint* buf, int size, int code)
		{
			buf[0] = 5;
			buf[1] = buf[2] = 0;
			buf[3] = (uint)code;
			buf[4] = (uint)size;

			if (Kirk.hleUtilsBufferCopyWithRange((byte*)buf, size + 0x14, (byte *)buf, size + 0x14, Kirk.CommandEnum.PSP_KIRK_CMD_DECRYPT) != Kirk.ResultEnum.OK)
			{
				return -1;
			}

			return 0;
		}

		protected byte[] DecryptPRX2(byte[] _pbIn, bool ShowInfo = false)
		{
			int size = (int)_pbIn.Length;
			var _pbOut = new byte[size];
			_pbIn.CopyTo(_pbOut, 0);

			var _tmp1 = new byte[0x150];
			var _tmp2 = new byte[0x90 + 0x14];
			var _tmp3 = new byte[0x60 + 0x14];

			fixed (byte* inbuf = _pbIn)
			fixed (byte* outbuf = _pbOut)
			fixed (byte* tmp1 = _tmp1)
			fixed (byte* tmp2 = _tmp2)
			fixed (byte* tmp3 = _tmp3)
			{
				var HeaderPointer = (HeaderStruct*)inbuf;
				this.Header = *(HeaderStruct*)inbuf;
				var pti = GetTagInfo2(this.Header.Tag);
				Console.WriteLine("{0}", pti);

				int retsize = *(int *)&inbuf[0xB0];

				PointerUtils.Memset(_tmp1, 0, 0x150);
				PointerUtils.Memset(_tmp2, 0, 0x90 + 0x14);
				PointerUtils.Memset(_tmp3, 0, 0x60 + 0x14);

				PointerUtils.Memcpy(outbuf, inbuf, size);

				if (size < 0x160)
				{
					throw (new InvalidDataException("buffer not big enough, "));
				}

				if ((size - 0x150) < retsize)
				{
					throw (new InvalidDataException("not enough data, "));
				}

				PointerUtils.Memcpy(tmp1, outbuf, 0x150);

				int i, j;
				//byte *p = tmp2+0x14;

				for (i = 0; i < 9; i++)
				{
					for (j = 0; j < 0x10; j++)
					{
						_tmp2[0x14 + (i << 4) + j] = pti.key[j];
					}

					_tmp2[0x14 + (i << 4)] = (byte)i;
				}	

				if (Scramble((uint *)tmp2, 0x90, pti.code) < 0)
				{
					throw (new InvalidDataException("error in Scramble#1, "));
				}

				PointerUtils.Memcpy(outbuf, tmp1 + 0xD0, 0x5C);
				PointerUtils.Memcpy(outbuf + 0x5C, tmp1 + 0x140, 0x10);
				PointerUtils.Memcpy(outbuf + 0x6C, tmp1 + 0x12C, 0x14);
				PointerUtils.Memcpy(outbuf + 0x80, tmp1 + 0x080, 0x30);
				PointerUtils.Memcpy(outbuf + 0xB0, tmp1 + 0x0C0, 0x10);
				PointerUtils.Memcpy(outbuf + 0xC0, tmp1 + 0x0B0, 0x10);
				PointerUtils.Memcpy(outbuf + 0xD0, tmp1 + 0x000, 0x80);

				PointerUtils.Memcpy(tmp3 + 0x14, outbuf + 0x5C, 0x60);	

				if (Scramble((uint *)tmp3, 0x60, pti.code) < 0)
				{
					throw (new InvalidDataException("error in Scramble#2, "));
				}

				PointerUtils.Memcpy(outbuf + 0x5C, tmp3, 0x60);
				PointerUtils.Memcpy(tmp3, outbuf + 0x6C, 0x14);
				PointerUtils.Memcpy(outbuf + 0x70, outbuf + 0x5C, 0x10);
				PointerUtils.Memset(outbuf + 0x18, 0, 0x58);
				PointerUtils.Memcpy(outbuf + 0x04, outbuf, 0x04);

				*((uint *)outbuf) = 0x014C;
				PointerUtils.Memcpy(outbuf + 0x08, tmp2, 0x10);

				/* sha-1 */
				if (Kirk.hleUtilsBufferCopyWithRange(outbuf, 3000000, outbuf, 3000000, Core.Crypto.Kirk.CommandEnum.PSP_KIRK_CMD_SHA1_HASH) != Core.Crypto.Kirk.ResultEnum.OK)
				{
					throw (new InvalidDataException("error in sceUtilsBufferCopyWithRange 0xB, "));
				}

				if (PointerUtils.Memcmp(outbuf, tmp3, 0x14) != 0)
				{
					throw (new InvalidDataException("WARNING (SHA-1 incorrect), "));
				}
	
				int iXOR;

				for (iXOR = 0; iXOR < 0x40; iXOR++) {
					tmp3[iXOR+0x14] = (byte)(outbuf[iXOR+0x80] ^ _tmp2[iXOR+0x10]);
				}

				if (Scramble((uint *)tmp3, 0x40, pti.code) != 0)
				{
					throw (new InvalidDataException("error in Scramble#3, "));
				}
	
				for (iXOR = 0x3F; iXOR >= 0; iXOR--) {
					outbuf[iXOR+0x40] = (byte)(_tmp3[iXOR] ^ _tmp2[iXOR+0x50]); // uns 8
				}

				PointerUtils.Memset(outbuf + 0x80, 0, 0x30);
				*(uint *)&outbuf[0xA0] = 1;

				PointerUtils.Memcpy(outbuf + 0xB0, outbuf + 0xC0, 0x10);
				PointerUtils.Memset(outbuf + 0xC0, 0, 0x10);

				// the real decryption
				var ret = Kirk.hleUtilsBufferCopyWithRange(outbuf, size, outbuf + 0x40, size - 0x40, Core.Crypto.Kirk.CommandEnum.PSP_KIRK_CMD_DECRYPT_PRIVATE);
				if (ret != 0)
				{
					throw (new InvalidDataException(String.Format("error in sceUtilsBufferCopyWithRange 0x1 (0x{0:X}), ", ret)));
				}

				if (retsize < 0x150)
				{
					// Fill with 0
					PointerUtils.Memset(outbuf + retsize, 0, 0x150 - retsize);		
				}

				return _pbOut.Slice(0, retsize).ToArray();
			}
		}


		public byte[] DecryptPRX(byte[] _pbIn, bool ShowInfo = false)
		{
			try
			{
				return DecryptPRX1(_pbIn, ShowInfo);
			}
			catch (InvalidDataException)
			{
				return DecryptPRX2(_pbIn, ShowInfo);
			}
		}

		/// <summary>
		/// 
		/// </summary>
        /// <param name="_pbIn"></param>
        /// <param name="ShowInfo"></param>
		public byte[] Decrypt(byte[] _pbIn, bool ShowInfo = false)
		{
			this.Kirk = new Kirk();
			this.Kirk.kirk_init();

			return DecryptPRX(_pbIn, ShowInfo);
		}
	}
}
