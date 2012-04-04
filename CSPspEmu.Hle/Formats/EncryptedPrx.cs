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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Stream"></param>
		public void Load(byte[] inbuf)
		{
			var Kirk = new Kirk();
			Kirk.kirk_init();
			var outbuf = new byte[(int)inbuf.Length];
			inbuf.CopyTo(outbuf, 0);

			fixed (byte* _inbuf = inbuf)
			fixed (byte* _outbuf = outbuf)
			{
				var HeaderPointer = (HeaderStruct*)_inbuf;
				this.Header = *(HeaderStruct*)_inbuf;
				var pti = g_oldTagInfo.Where(Tag => Tag.tag == this.Header.tag).Single();

				for (int i = 0; i < 0x150; i++)
				{
					outbuf[i] = 0;
				}
				for (int i = 0; i < 0x40; i++)
				{
					outbuf[i] = 0x55;
				}

				//HeaderPointer->psp_size = 5;
				//HeaderPointer->boot_entry = 0;

				var h7_header = (Kirk.KIRK_AES128CBC_HEADER*)&_outbuf[0x2C];
				h7_header->Mode = Core.Crypto.Kirk.KirkMode.DecryptCbc;
				h7_header->Unknown4 = 0;
				h7_header->Unknown8 = 0;
				h7_header->KeySeed = pti.code;
				h7_header->Datasize = 0x70;

				byte[] header = new byte[0x150];

				Array.Copy(inbuf, 0xD0, header, 0x00, 0x80);
				Array.Copy(inbuf, 0x80, header, 0x80, 0x50);
				Array.Copy(inbuf, 0x00, header, 0xD0, 0x80);

				if (pti.codeExtra != 0)
				{
					//ScramblePRXV2(header, (byte)pti.codeExtra);
					throw (new NotImplementedException());
				}

				Array.Copy(header, 0x40, outbuf, 0x40, 0x40);

				for (int iXOR = 0; iXOR < 0x70; iXOR++) outbuf[0x40 + iXOR] = (byte)(outbuf[0x40 + iXOR] ^ (byte)pti.key[0x14 + iXOR]);
				for (int k = 0; k < 0x30; k++) outbuf[k + 0x80] = 0;


				// Scramble the data by calling CMD7.
				var bScrambleOut = new byte[outbuf.Length];
				var bScrambleIn = outbuf;
				fixed (byte* _bScrambleOut = bScrambleOut)
				{
					Kirk.hleUtilsBufferCopyWithRange(_bScrambleOut, 0x70, &_outbuf[0x2C], 0x70, Kirk.CommandEnum.PSP_KIRK_CMD_DECRYPT);
				}

				for (int iXOR = 0x6F; iXOR >= 0; iXOR--)
				{
					outbuf[0x40 + iXOR] = (byte)(outbuf[0x2C + iXOR] ^ (byte)pti.key[0x20 + iXOR]);
				}

				for (int k = 0; k < 0x30; k++)
				{
					outbuf[k + 0x80] = 0;
				}

				var KirkHeader = (Kirk.AES128CMACHeader *)&_outbuf[0x40];
				KirkHeader->Mode = Kirk.KirkMode.Cmd1;

				Array.Copy(inbuf, 0x40 + 0x70, outbuf, 0x40 + 0x70, 0x20);
				Array.Copy(inbuf, 0, outbuf, 0x40 + 0x90, 0x80);

				// Call KIRK CMD1 for final decryption.
				/*
				ByteBuffer bDataOut = ByteBuffer.wrap(outbuf);
				ByteBuffer bHeaderIn = bDataOut.duplicate();
				bHeaderIn.position(0x40);
				hleUtilsBufferCopyWithRange(bDataOut, size, bHeaderIn, size, 0x1);
				*/
				
//#if true
				{
					int size = outbuf.Length;
					var Result = Kirk.hleUtilsBufferCopyWithRange(&_outbuf[0x00], size, (byte *)KirkHeader, size, Kirk.CommandEnum.PSP_KIRK_CMD_DECRYPT_PRIVATE, true);
					Console.WriteLine(Result);
				}
//#endif
				File.WriteAllBytes("../../../TestInput/temp.bin", outbuf);

				//File.write
				//outbuf

				//Kirk.kirk_CMD1();
				//Console.WriteLine("{0:X}", this.Header.tag);
			}
		}
	}
}
