using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils.Extensions;
using CSharpUtils.Streams;

namespace CSharpUtils.Ext.Compression.Zip
{
    /// <summary>
    /// 
    /// </summary>
    public class ZipArchive
    {
        Stream _stream;
        BinaryReader _binaryReader;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ignoreErrors"></param>
        /// <exception cref="Exception"></exception>
        public void Load(Stream stream, bool ignoreErrors = true)
        {
            _stream = stream;
            _binaryReader = new BinaryReader(stream);
            Files = new Dictionary<string, File>();
            try
            {
                while (!stream.Eof())
                {
                    ReadRecord();
                }
            }
            catch (Exception e)
            {
                if (!ignoreErrors)
                {
                    throw (new Exception("Error loading Zip", e));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum CompressionMethod : ushort
        {
            /// <summary>
            /// 
            /// </summary>
            Stored = 0, // The file is stored (no compression)

            /// <summary>
            /// 
            /// </summary>
            Shrunk = 1, // The file is Shrunk

            /// <summary>
            /// 
            /// </summary>
            CompressionFactor1 = 2, // The file is Reduced with compression factor 1

            /// <summary>
            /// 
            /// </summary>
            CompressionFactor2 = 3, // The file is Reduced with compression factor 2

            /// <summary>
            /// 
            /// </summary>
            CompressionFactor3 = 4, // The file is Reduced with compression factor 3

            /// <summary>
            /// 
            /// </summary>
            CompressionFactor4 = 5, // The file is Reduced with compression factor 4

            /// <summary>
            /// 
            /// </summary>
            Imploded = 6, // The file is Imploded

            /// <summary>
            /// 
            /// </summary>
            Tokenizing = 7, // Reserved for Tokenizing compression algorithm

            /// <summary>
            /// 
            /// </summary>
            Deflate = 8, // The file is Deflated

            /// <summary>
            /// 
            /// </summary>
            Deflate64 = 9, // Enhanced Deflating using Deflate64(tm)

            /// <summary>
            /// 
            /// </summary>
            Imploding = 10, // PKWARE Data Compression Library Imploding (old IBM TERSE)

            /// <summary>
            /// 
            /// </summary>
            Reserved1 = 11, // Reserved by PKWARE

            /// <summary>
            /// 
            /// </summary>
            Bzip2 = 12, // File is compressed using BZIP2 algorithm

            /// <summary>
            /// 
            /// </summary>
            Reserved2 = 13, // Reserved by PKWARE

            /// <summary>
            /// 
            /// </summary>
            Lzma = 14, // LZMA (EFS)

            /// <summary>
            /// 
            /// </summary>
            Reserved3 = 15, // Reserved by PKWARE

            /// <summary>
            /// 
            /// </summary>
            Reserved4 = 16, // Reserved by PKWARE

            /// <summary>
            /// 
            /// </summary>
            Reserved5 = 17, // Reserved by PKWARE

            /// <summary>
            /// 
            /// </summary>
            Terse = 18, // File is compressed using IBM TERSE (new)

            /// <summary>
            /// 
            /// </summary>
            Lz77 = 19, // IBM LZ77 z Architecture (PFS)

            /// <summary>
            /// 
            /// </summary>
            WavPack = 97, // WavPack compressed data

            /// <summary>
            /// 
            /// </summary>
            PpMd = 98 // PPMd version I, Rev 1
        }

        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LocalFileHeader
        {
            //public uint Signature;
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
            public CompressionMethod CompressionMethod;

            /// <summary>
            /// 
            /// </summary>
            public ushort LastModFileTime;

            /// <summary>
            /// 
            /// </summary>
            public ushort LastModFileDate;

            /// <summary>
            /// 
            /// </summary>
            public uint Crc32;

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
            public ushort ExtraFieldLength;
        }

        /// <summary>
        /// 
        /// </summary>
        public class File
        {
            /// <summary>
            /// 
            /// </summary>
            public ZipArchive ZipArchive;

            /// <summary>
            /// 
            /// </summary>
            public LocalFileHeader LocalFileHeader;

            /// <summary>
            /// 
            /// </summary>
            public Stream CompressedStream;

            /// <summary>
            /// 
            /// </summary>
            public Stream ExtraFieldStream;

            /// <summary>
            /// 
            /// </summary>
            public string Name;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <exception cref="NotImplementedException"></exception>
            public Stream OpenRead()
            {
                switch (LocalFileHeader.CompressionMethod)
                {
                    case CompressionMethod.Stored:
                        return SliceStream.CreateWithLength(CompressedStream);
                    case CompressionMethod.Deflate:
                        return new DeflateStream(SliceStream.CreateWithLength(CompressedStream),
                            CompressionMode.Decompress);
                    default:
                        throw(new NotImplementedException("Not Implementeed : " + LocalFileHeader.CompressionMethod));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<String, File> Files;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void ReadRecord()
        {
            Debug.Assert(_stream.ReadStringz(2) == "PK");
            var structType = _binaryReader.ReadUint16Endian(Endianness.BigEndian);
            switch (structType)
            {
                /*
                // F.  Central directory structure:
                case 0x0102:
                    break;
                */
                // A.  Local file header:
                case 0x0304:
                {
                    var localFileHeader = _stream.ReadStruct<LocalFileHeader>();
                    _stream.Skip(localFileHeader.ExtraFieldLength);
                    var name = _stream.ReadString(localFileHeader.FileNameLength, Encoding.UTF8);
                    var compressedStream = _stream.ReadStream(localFileHeader.CompressedSize);
                    var extraFieldStream = _stream.ReadStream(localFileHeader.ExtraFieldLength);

                    Files.Add(name, new File
                    {
                        ZipArchive = this,
                        LocalFileHeader = localFileHeader,
                        Name = name,
                        CompressedStream = compressedStream,
                        ExtraFieldStream = extraFieldStream
                    });
                }
                    break;
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
                    throw new NotImplementedException($"Unknown {structType:X}");
            }
        }
    }
}