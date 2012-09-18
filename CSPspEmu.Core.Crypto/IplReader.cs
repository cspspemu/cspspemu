using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CSharpUtils;
using CSharpUtils.Streams;
using CSPspEmu.Core.Crypto;

namespace CSPspEmu.Core
{
	/// <summary>
	// 
	/// </summary>
	/// <see cref="http://silverspring.lan.st/NPSPTD_01.txt"/>
	/// <see cref="http://daxhordes.org/forum/viewtopic.php?f=33&t=808"/>
	public class IplReader
	{
		/// <summary>
		/// 
		/// </summary>
		protected Stream Stream;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="NandReader"></param>
		public IplReader(NandReader NandReader)
		{
			this.Stream = NandReader.SliceWithLength();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public MemoryStream GetIplData()
		{
			var MemoryStream = new MemoryStream();
			foreach (var BlockOffset in GetIplOffsets().ToArray())
			{
				//Console.WriteLine(BlockOffset);
				Stream.Position = NandReader.BytesPerBlock * BlockOffset;
				MemoryStream.WriteBytes(Stream.ReadBytes(NandReader.BytesPerBlock));
			}
			MemoryStream.Position = 0;
			return MemoryStream;
		}

		[StructLayout(LayoutKind.Sequential, Size = 0xF60)]
		public struct IplBlock
		{
			public uint LoadAddress;
			public uint BlockSize;
			public uint EntryFunction;
			public uint Checksum;
			public byte BlockData;
		}

		public struct IplInfo
		{
			public uint EntryFunction;
		}

		public IplInfo LoadIplToMemory(Stream OutputStream)
		{
			return DecryptIplToMemory(GetIplData().ToArray().Skip(0x4000).ToArray(), OutputStream, ToMemoryAddress: true);
		}

		public void WriteIplToFile(Stream StreamOut)
		{
			DecryptIplToMemory(GetIplData().ToArray().Skip(0x4000).ToArray(), StreamOut, ToMemoryAddress: false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="IplData"></param>
		/// <returns></returns>
		unsafe static public IplInfo DecryptIplToMemory(byte[] IplData, Stream OutputStream, bool ToMemoryAddress = true)
		{
			var buffer = new byte[0x1000];
			var IplInfo = default(IplInfo);

			//ArrayUtils.HexDump(IplData);

			fixed (byte* IplPtr = IplData)
			fixed (byte* bufferPtr = buffer)
			{
				for (int n = 0; n < IplData.Length; n += 0x1000)
				{
					var Ptr = IplPtr + n;

					var Header = *(Kirk.AES128CMACHeader*)Ptr;
					//Console.WriteLine(Header.DataSize);
					var Kirk = new Kirk();
					Kirk.kirk_init();
					Kirk.kirk_CMD1(bufferPtr, Ptr, 0x1000, do_check: false);
					var IplBlock = *(IplBlock*)bufferPtr;
					//Console.WriteLine(IplBlock.ToStringDefault());
					if (ToMemoryAddress)
					{
						OutputStream.Position = IplBlock.LoadAddress;
						Console.WriteLine("IplBlock.LoadAddress: 0x{0:X8}", IplBlock.LoadAddress);
					}
					OutputStream.WriteBytes(PointerUtils.PointerToByteArray(&IplBlock.BlockData, (int)IplBlock.BlockSize));
					if (IplBlock.EntryFunction != 0)
					{
						IplInfo.EntryFunction = IplBlock.EntryFunction;
					}
				}
			}

			return IplInfo;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ushort> GetIplOffsets()
		{
			var Stream = this.Stream.SliceWithLength(NandReader.BytesPerBlock * 4);
			while (true)
			{
				var Result = Stream.ReadStruct<ushort>();
				if (Result == 0) break;
				yield return Result;
			}
		}
	}
}
