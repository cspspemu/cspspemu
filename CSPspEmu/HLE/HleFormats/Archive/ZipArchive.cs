using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;

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
            IbmLz77Zpfs = 19,
            WavPack = 97,
            Ppmd = 98,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct LocalFileHeader
        {
            public const uint ExpectedMagic = 0x04034b50;
            public uint Magic; // 0x04034b50
            public ushort VersionNeededToExtract;
            public ushort GeneralPurposeBitFlag;
            public CompressionMethods CompressionMethod;
            public ushort LastModificationFileTime;
            public ushort LastModificationFileDate;
            public uint CRC32;
            public uint CompressedSize;
            public uint UncompressedSize;
            public ushort FileNameLength;
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
                        return new MemoryStream(
                            (new DeflateStream(CompressedStream.SliceWithLength(),
                                CompressionMode.Decompress)).ReadAll(false));
                    case CompressionMethods.Stored:
                        return CompressedStream.SliceWithLength();
                    default:
                        throw (new NotImplementedException("Can't implement method '" +
                                                           LocalFileHeader.CompressionMethod + "'"));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{FileName}:{CompressedStream.Length}";
        }

        protected Dictionary<string, ZipEntry> Entries;
        private bool _caseInsensitive;

        public ZipArchive()
        {
        }

        public ZipArchive(string fileName, bool caseInsensitive = true) => Load(fileName, caseInsensitive);

        public ZipArchive(Stream stream, bool caseInsensitive = true) => Load(stream, caseInsensitive);

        public ZipArchive Load(string fileName, bool caseInsensitive = true) =>
            Load(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), caseInsensitive);

        public string NormalizePath(string fileName)
        {
            var Return = '/' + fileName.Replace('\\', '/').Trim('/');
            if (_caseInsensitive) Return = Return.ToLower();
            return Return;
        }

        public ZipArchive Load(Stream stream, bool caseInsensitive = true)
        {
            _caseInsensitive = caseInsensitive;
            Entries = new Dictionary<string, ZipEntry>();
            while (!stream.Eof())
            {
                var localFileHeader = stream.ReadStruct<LocalFileHeader>();
                if (stream.Eof()) break;
                if (localFileHeader.Magic != LocalFileHeader.ExpectedMagic)
                {
                    //Console.Error.WriteLine("0x{0:X}", LocalFileHeader.Magic);
                    break;
                }
                var fileName = stream.ReadString(localFileHeader.FileNameLength);
                stream.ReadBytes(localFileHeader.ExtraLength);
                //Console.Error.WriteLine(LocalFileHeader.CompressedSize);
                var compressedStream = stream.ReadStream(localFileHeader.CompressedSize);

                Entries.Add(NormalizePath(fileName), new ZipEntry()
                {
                    Zip = this,
                    LocalFileHeader = localFileHeader,
                    FileName = fileName,
                    CompressedStream = compressedStream,
                });
            }

            return this;
        }

        public ZipEntry this[string fileName] => Entries[NormalizePath(fileName)];

        public IEnumerator<ZipEntry> GetEnumerator()
        {
            foreach (var entry in Entries) yield return entry.Value;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}