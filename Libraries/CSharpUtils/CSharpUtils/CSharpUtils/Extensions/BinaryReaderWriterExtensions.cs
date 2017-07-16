using System;
using System.IO;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class BinaryReaderWriterExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] ReverseBytes(this byte[] bytes)
        {
            var reversedBytes = new byte[bytes.Length];
            for (int from = bytes.Length - 1, to = 0; from >= 0; from--, to++)
            {
                reversedBytes[to] = bytes[from];
            }
            return reversedBytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="endian"></param>
        /// <returns></returns>
        public static uint ReadUint32Endian(this BinaryReader binaryReader, Endianness endian)
        {
            var bytes = binaryReader.ReadBytes(4);
            if (endian == Endianness.BigEndian) bytes = bytes.ReverseBytes();
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="endian"></param>
        /// <returns></returns>
        public static float ReadSingleEndian(this BinaryReader binaryReader, Endianness endian)
        {
            var bytes = binaryReader.ReadBytes(4);
            if (endian == Endianness.BigEndian) bytes = bytes.ReverseBytes();
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="endian"></param>
        /// <returns></returns>
        public static ushort ReadUint16Endian(this BinaryReader binaryReader, Endianness endian)
        {
            var bytes = binaryReader.ReadBytes(2);
            if (endian == Endianness.BigEndian) bytes = bytes.ReverseBytes();
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryWriter"></param>
        /// <param name="value"></param>
        /// <param name="endian"></param>
        public static void WriteEndian(this BinaryWriter binaryWriter, ushort value, Endianness endian)
        {
            binaryWriter.Write((byte) ((value >> 8) & 0xFF));
            binaryWriter.Write((byte) ((value >> 0) & 0xFF));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryWriter"></param>
        /// <param name="value"></param>
        /// <param name="endian"></param>
        public static void WriteEndian(this BinaryWriter binaryWriter, uint value, Endianness endian)
        {
            binaryWriter.Write((byte) ((value >> 24) & 0xFF));
            binaryWriter.Write((byte) ((value >> 16) & 0xFF));
            binaryWriter.Write((byte) ((value >> 8) & 0xFF));
            binaryWriter.Write((byte) ((value >> 0) & 0xFF));
        }
    }
}