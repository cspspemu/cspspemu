using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Crypto;

namespace CSPspEmu.Hle.Formats
{
	unsafe public partial class EncryptedPrx
	{
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

		private TAG_INFO GetTagInfoByTag(uint CheckTag)
		{
			var Result = g_tagInfo.Where(Tag => Tag.tag == CheckTag).Single();
			if (Result == null) throw(new NullReferenceException(String.Format("Can't find tag 0x{0:X}", CheckTag)));
			return Result;
		}

		protected void ExtraV2Mangle(Kirk Kirk, byte* buffer1, byte codeExtra)
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Stream"></param>
		public byte[] Decrypt(byte[] _pbIn)
		{
			var Kirk = new Kirk();
			Kirk.kirk_init();
			int cbTotal = (int)_pbIn.Length;
			var _pbOut = new byte[cbTotal];
			_pbIn.CopyTo(_pbOut, 0);

			fixed (byte* pbIn = _pbIn)
			fixed (byte* pbOut = _pbOut)
			{
				var HeaderPointer = (HeaderStruct*)pbIn;
				this.Header = *(HeaderStruct*)pbIn;
				var pti = GetTagInfoByTag(this.Header.Tag);
				Console.WriteLine("TAG_INFO: {0}", pti);

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
						ExtraV2Mangle(Kirk, buffer1 + 0x10, pti.codeExtra);
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
					throw(new Exception(CStringFormater.Sprintf("mangle#7 returned 0x%08X, ", ret)));
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
					throw(new Exception(CStringFormater.Sprintf("mangle#1 returned 0x%08X", ret)));
				}

				//File.WriteAllBytes("../../../TestInput/temp.bin", _pbOut);

				var OutputSize = *(int*)&pbIn[0xB0];

				return _pbOut.Slice(0, OutputSize).ToArray();

#if false
				/*
				DecryptAes(180, 16, 16)
				DecryptAes(132, 16, 16)
				DecryptAes(32, 16, 16)
				DecryptAes(32, 16, 16)
				DecryptAes(3466076, 16, 16)
				*/

				// Scramble the data by calling CMD7.
				var bScrambleOut = new byte[_pbOut.Length];
				var bScrambleIn = _pbOut;
				fixed (byte* _bScrambleOut = bScrambleOut)
				{
					Kirk.hleUtilsBufferCopyWithRange(_bScrambleOut, 0x70, &pbOut[0x2C], 0x70, Kirk.CommandEnum.PSP_KIRK_CMD_DECRYPT);
				}

				for (int iXOR = 0x6F; iXOR >= 0; iXOR--)
				{
					_pbOut[0x40 + iXOR] = (byte)(_pbOut[0x2C + iXOR] ^ (byte)pti.key[0x20 + iXOR]);
				}

				for (int k = 0; k < 0x30; k++)
				{
					_pbOut[k + 0x80] = 0;
				}

				var KirkHeader = (Kirk.AES128CMACHeader*)&pbOut[0x40];
				KirkHeader->Mode = Kirk.KirkMode.Cmd1;

				Array.Copy(_pbIn, 0x40 + 0x70, _pbOut, 0x40 + 0x70, 0x20);
				Array.Copy(_pbIn, 0, _pbOut, 0x40 + 0x90, 0x80);

				// Call KIRK CMD1 for final decryption.
				/*
				ByteBuffer bDataOut = ByteBuffer.wrap(outbuf);
				ByteBuffer bHeaderIn = bDataOut.duplicate();
				bHeaderIn.position(0x40);
				hleUtilsBufferCopyWithRange(bDataOut, size, bHeaderIn, size, 0x1);
				*/

				//#if true
				{
					int size = _pbOut.Length;
					var Result = Kirk.hleUtilsBufferCopyWithRange(&pbOut[0x00], size, (byte*)KirkHeader, size, Kirk.CommandEnum.PSP_KIRK_CMD_DECRYPT_PRIVATE, true);
					Console.WriteLine(Result);
				}
//#endif
				File.WriteAllBytes("../../../TestInput/temp.bin", _pbOut);

				//File.write
				//outbuf

				//Kirk.kirk_CMD1();
				//Console.WriteLine("{0:X}", this.Header.tag);
#endif
			}
		}
	}
}
