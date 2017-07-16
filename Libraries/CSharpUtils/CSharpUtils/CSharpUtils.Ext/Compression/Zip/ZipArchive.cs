using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils.Streams;

namespace CSharpUtils.Compression.Zip
{
	public class ZipArchive
	{
		Stream Stream;
		BinaryReader BinaryReader;

		public void Load(Stream Stream, bool IgnoreErrors = true)
		{
			this.Stream = Stream;
			this.BinaryReader = new BinaryReader(Stream);
			this.Files = new Dictionary<String, File>();
			try
			{
				while (!Stream.Eof())
				{
					ReadRecord();
				}
			}
			catch (Exception e)
			{
				if (!IgnoreErrors)
				{
					throw (new Exception("Error loading Zip", e));
				}
			}
		}

		public enum CompressionMethod : ushort
		{
			Stored = 0, // The file is stored (no compression)
			Shrunk = 1, // The file is Shrunk
			CompressionFactor1 = 2, // The file is Reduced with compression factor 1
			CompressionFactor2 = 3, // The file is Reduced with compression factor 2
			CompressionFactor3 = 4, // The file is Reduced with compression factor 3
			CompressionFactor4 = 5, // The file is Reduced with compression factor 4
			Imploded = 6, // The file is Imploded
			Tokenizing = 7, // Reserved for Tokenizing compression algorithm
			Deflate = 8, // The file is Deflated
			Deflate64 = 9, // Enhanced Deflating using Deflate64(tm)
			Imploding = 10, // PKWARE Data Compression Library Imploding (old IBM TERSE)
			Reserved1 = 11, // Reserved by PKWARE
			Bzip2 = 12, // File is compressed using BZIP2 algorithm
			Reserved2 = 13, // Reserved by PKWARE
			Lzma = 14, // LZMA (EFS)
			Reserved3 = 15, // Reserved by PKWARE
			Reserved4 = 16, // Reserved by PKWARE
			Reserved5 = 17, // Reserved by PKWARE
			Terse = 18, // File is compressed using IBM TERSE (new)
			Lz77 = 19, // IBM LZ77 z Architecture (PFS)
			WavPack = 97, // WavPack compressed data
			PPMd = 98, // PPMd version I, Rev 1
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct LocalFileHeader
		{
			//public uint Signature;
			public ushort VersionNeededToExtract;
			public ushort GeneralPurposeBitFlag;
			public CompressionMethod CompressionMethod;
			public ushort LastModFileTime;
			public ushort LastModFileDate;
			public uint Crc32;
			public uint CompressedSize;
			public uint UncompressedSize;
			public ushort FileNameLength;
			public ushort ExtraFieldLength;
		}

		public class File
		{
			public ZipArchive ZipArchive;
			public LocalFileHeader LocalFileHeader;
			public Stream CompressedStream;
			public Stream ExtraFieldStream;
			public String Name;

			public Stream OpenRead()
			{
				switch (LocalFileHeader.CompressionMethod)
				{
					case CompressionMethod.Stored:
						return SliceStream.CreateWithLength(CompressedStream);
					case CompressionMethod.Deflate:
						return new DeflateStream(SliceStream.CreateWithLength(CompressedStream), CompressionMode.Decompress);
					default:
						throw(new NotImplementedException("Not Implementeed : " + LocalFileHeader.CompressionMethod));
				}
			}
		}

		public Dictionary<String, File> Files;

		public void ReadRecord()
		{
			Debug.Assert(this.Stream.ReadStringz(2) == "PK");
			var StructType = this.BinaryReader.ReadUint16Endian(Endianness.BigEndian);
			switch (StructType)
			{
				/*
				// F.  Central directory structure:
				case 0x0102:
					break;
				*/
				// A.  Local file header:
				case 0x0304: {
					var LocalFileHeader = Stream.ReadStruct<LocalFileHeader>();
					Stream.Skip(LocalFileHeader.ExtraFieldLength);
					var Name = Stream.ReadString(LocalFileHeader.FileNameLength, Encoding.UTF8);
					var CompressedStream = Stream.ReadStream(LocalFileHeader.CompressedSize);
					var ExtraFieldStream = Stream.ReadStream(LocalFileHeader.ExtraFieldLength);

					Files.Add(Name, new File()
					{
						ZipArchive = this,
						LocalFileHeader = LocalFileHeader,
						Name = Name,
						CompressedStream = CompressedStream,
						ExtraFieldStream = ExtraFieldStream,
					});
				} break;
				/*
				// I.  End of central directory record:
				case 0x0506:
					break;
				// G.  Zip64 end of central directory record
				case 0x0606:
					break;
				// H.  Zip64 end of central directory locator
				case 0x0607:
					break;
				// E.  Archive extra data record: 
				case 0x0608:
					break;
				*/
				default:
					throw (new NotImplementedException(String.Format("Unknown {0:X}", StructType)));
			}
		}
	}
}
