using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CSharpUtils;
using CSPspEmu.Core.Crypto;

namespace CSPspEmu.Core
{
	/// <summary>
	// 
	/// </summary>
	/// <see cref="http://silverspring.lan.st/NPSPTD_01.txt"/>
	/// <see cref="http://daxhordes.org/forum/viewtopic.php?f=33&t=808"/>
	public unsafe class IplReader
	{
		/// <summary>
		/// 
		/// </summary>
		protected Stream Stream;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nandReader"></param>
		public IplReader(NandReader nandReader)
		{
			Stream = nandReader.SliceWithLength();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public MemoryStream GetIplData()
		{
			var memoryStream = new MemoryStream();
			foreach (var blockOffset in GetIplOffsets().ToArray())
			{
				//Console.WriteLine(BlockOffset);
				Stream.Position = NandReader.BytesPerBlock * blockOffset;
				memoryStream.WriteBytes(Stream.ReadBytes(NandReader.BytesPerBlock));
			}
			memoryStream.Position = 0;
			return memoryStream;
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

		public IplInfo LoadIplToMemory(Stream outputStream)
		{
			return DecryptIplToMemory(GetIplData().ToArray().Skip(0x4000).ToArray(), outputStream, toMemoryAddress: true);
		}

		public void WriteIplToFile(Stream streamOut)
		{
			DecryptIplToMemory(GetIplData().ToArray().Skip(0x4000).ToArray(), streamOut, toMemoryAddress: false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="iplData"></param>
		/// <param name="outputStream"></param>
		/// <param name="toMemoryAddress"></param>
		/// <returns></returns>
		public static IplInfo DecryptIplToMemory(byte[] iplData, Stream outputStream, bool toMemoryAddress = true)
		{
			var bufferBytes = new byte[0x1000];
			var iplInfo = default(IplInfo);

			//ArrayUtils.HexDump(IplData);

			fixed (byte* ipl = iplData)
			fixed (byte* buffer = bufferBytes)
			{
				for (var n = 0; n < iplData.Length; n += 0x1000)
				{
					var ptr = ipl + n;

					var header = *(Kirk.AES128CMACHeader*)ptr;
					//Console.WriteLine(Header.DataSize);
					var kirk = new Kirk();
					kirk.kirk_init();
					kirk.kirk_CMD1(buffer, ptr, 0x1000, do_check: false);
					var iplBlock = *(IplBlock*)buffer;
					//Console.WriteLine(IplBlock.ToStringDefault());
					if (toMemoryAddress)
					{
						outputStream.Position = iplBlock.LoadAddress;
						Console.WriteLine("IplBlock.LoadAddress: 0x{0:X8}", iplBlock.LoadAddress);
					}
					outputStream.WriteBytes(PointerUtils.PointerToByteArray(&iplBlock.BlockData, (int)iplBlock.BlockSize));
					if (iplBlock.EntryFunction != 0)
					{
						iplInfo.EntryFunction = iplBlock.EntryFunction;
					}
				}
			}

			return iplInfo;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ushort> GetIplOffsets()
		{
			var stream = Stream.SliceWithLength(NandReader.BytesPerBlock * 4);
			while (true)
			{
				var result = stream.ReadStruct<ushort>();
				if (result == 0) break;
				yield return result;
			}
		}
	}
}
