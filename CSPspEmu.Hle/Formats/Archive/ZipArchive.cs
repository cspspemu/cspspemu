using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ComponentAce.Compression.Libs.zlib;
using CSPspEmu.Hle.Utils;

namespace CSPspEmu.Hle.Formats.Archive
{
	public class ZipArchive : IEnumerable<ZipArchive.ZipEntry>
	{
		public enum CompressionMethods : ushort
		{
			Stored = 0,
			Shrunk = 1,
			ReducedCompressionFactor1 = 2,
			ReducedCompressionFactor2 = 3,
			ReducedCompressionFactor3 = 4,
			ReducedCompressionFactor4 = 5,
			Imploded = 6,
			TokenizingCompressionAlgorithm = 7,
			Deflate = 8,
			Deflate64 = 9,
			PkwareImploding = 10,
			PkwareReserved11 = 11,
			Bzip2 = 12,
			PkwareReserved13 = 13,
			Lzma = 14,
			PkwareReserved15 = 15,
			PkwareReserved16 = 16,
			PkwareReserved17 = 17,
			IbmTerse = 18,
			IbmLz77ZPFS = 19,
			WavPack = 97,
			Ppmd = 98,
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		public struct LocalFileHeader
		{
			public const uint ExpectedMagic = 0x04034b50;

			/// <summary>
			/// 0x04034b50
			/// </summary>
			public uint Magic;

			/// <summary>
			/// 
			/// </summary>
			public ushort VersionNeededToExtract;

			/// <summary>
			/// 
			/// </summary>
			public ushort GeneralPurposeBitFlag;

			/// <summary>
			/// 
			/// </summary>
			public CompressionMethods CompressionMethod;

			/// <summary>
			/// 
			/// </summary>
			public ushort LastModificationFileTime;

			/// <summary>
			/// 
			/// </summary>
			public ushort LastModificationFileDate;

			/// <summary>
			/// 
			/// </summary>
			public uint CRC32;

			/// <summary>
			/// 
			/// </summary>
			public uint CompressedSize;

			/// <summary>
			/// 
			/// </summary>
			public uint UncompressedSize;

			/// <summary>
			/// 
			/// </summary>
			public ushort FileNameLength;

			/// <summary>
			/// 
			/// </summary>
			public ushort ExtraLength;
		}

		public class ZipEntry
		{
			internal ZipEntry()
			{
			}

			/// <summary>
			/// 
			/// </summary>
			public ZipArchive Zip { get; internal set; }

			/// <summary>
			/// 
			/// </summary>
			public LocalFileHeader LocalFileHeader { get; internal set; }

			/// <summary>
			/// 
			/// </summary>
			public string FileName { get; internal set; }

			/// <summary>
			/// 
			/// </summary>
			public Stream CompressedStream { get; internal set; }

			/// <summary>
			/// 
			/// </summary>
			public Stream OpenUncompressedStream()
			{
				switch (LocalFileHeader.CompressionMethod)
				{
					case CompressionMethods.Deflate:
						//return new System.IO.Compression.DeflateStream(CompressedStream.SliceWithLength(0), CompressionMode.Decompress);
						return new MemoryStream((new System.IO.Compression.DeflateStream(CompressedStream.SliceWithLength(0), CompressionMode.Decompress)).ReadAll(FromStart: false));
					case CompressionMethods.Stored:
						return CompressedStream.SliceWithLength(0);
					default:
						throw (new NotImplementedException("Can't implement method '" + LocalFileHeader.CompressionMethod + "'"));
				}
				
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return String.Format("{0}:{1}", FileName, CompressedStream.Length);
			}
		}

		protected Dictionary<string, ZipEntry> Entries;
		private bool CaseInsensitive;

		public ZipArchive()
		{
		}

		public ZipArchive(String FileName, bool CaseInsensitive = true)
		{
			Load(FileName, CaseInsensitive);
		}

		public ZipArchive(Stream Stream, bool CaseInsensitive = true)
		{
			Load(Stream, CaseInsensitive);
		}

		public ZipArchive Load(string FileName, bool CaseInsensitive = true)
		{
			return Load(File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), CaseInsensitive);
		}

		public String NormalizePath(string FileName)
		{
			var Return = '/' + FileName.Replace('\\', '/').Trim('/');
			if (CaseInsensitive) Return = Return.ToLower();
			return Return;
		}

		public ZipArchive Load(Stream Stream, bool CaseInsensitive = true)
		{
			this.CaseInsensitive = CaseInsensitive;
			Entries = new Dictionary<string, ZipEntry>();
			while (!Stream.Eof())
			{
				var LocalFileHeader = Stream.ReadStruct<LocalFileHeader>();
				if (Stream.Eof()) break;
				if (LocalFileHeader.Magic != LocalFileHeader.ExpectedMagic)
				{
					//Console.Error.WriteLine("0x{0:X}", LocalFileHeader.Magic);
					break;
				}
				var FileName = Stream.ReadString(LocalFileHeader.FileNameLength);
				var Extra = Stream.ReadBytes(LocalFileHeader.ExtraLength);
				//Console.Error.WriteLine(LocalFileHeader.CompressedSize);
				var CompressedStream = Stream.ReadStream(LocalFileHeader.CompressedSize);

				Entries.Add(NormalizePath(FileName), new ZipEntry()
				{
					Zip = this,
					LocalFileHeader = LocalFileHeader,
					FileName = FileName,
					CompressedStream = CompressedStream,
				});
			}

			return this;
		}

		public ZipEntry this[String FileName]
		{
			get
			{
				return Entries[NormalizePath(FileName)];
			}
		}

		public IEnumerator<ZipArchive.ZipEntry> GetEnumerator()
		{
			foreach (var Entry in Entries)
			{
				yield return Entry.Value;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
