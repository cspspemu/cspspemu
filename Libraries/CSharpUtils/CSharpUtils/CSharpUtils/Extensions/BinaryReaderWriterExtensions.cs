using System;
using System.IO;
using CSharpUtils;

public static class BinaryReaderWriterExtensions
{
    public static byte[] ReverseBytes(this byte[] Bytes)
    {
        var ReversedBytes = new byte[Bytes.Length];
        for (int From = Bytes.Length - 1, To = 0; From >= 0; From--, To++)
        {
            ReversedBytes[To] = Bytes[From];
        }
        return ReversedBytes;
    }

    public static uint ReadUint32Endian(this BinaryReader BinaryReader, Endianness Endian)
    {
        byte[] Bytes = BinaryReader.ReadBytes(4);
        if (Endian == Endianness.BigEndian) Bytes = Bytes.ReverseBytes();
        return BitConverter.ToUInt32(Bytes, 0);
    }

    public static float ReadSingleEndian(this BinaryReader BinaryReader, Endianness Endian)
    {
        byte[] Bytes = BinaryReader.ReadBytes(4);
        if (Endian == Endianness.BigEndian) Bytes = Bytes.ReverseBytes();
        return BitConverter.ToSingle(Bytes, 0);
    }

    public static ushort ReadUint16Endian(this BinaryReader BinaryReader, Endianness Endian)
    {
        byte[] Bytes = BinaryReader.ReadBytes(2);
        if (Endian == Endianness.BigEndian) Bytes = Bytes.ReverseBytes();
        return BitConverter.ToUInt16(Bytes, 0);
    }

    public static void WriteEndian(this BinaryWriter BinaryWriter, ushort Value, Endianness Endian)
    {
        BinaryWriter.Write((byte) ((Value >> 8) & 0xFF));
        BinaryWriter.Write((byte) ((Value >> 0) & 0xFF));
    }

    public static void WriteEndian(this BinaryWriter BinaryWriter, uint Value, Endianness Endian)
    {
        BinaryWriter.Write((byte) ((Value >> 24) & 0xFF));
        BinaryWriter.Write((byte) ((Value >> 16) & 0xFF));
        BinaryWriter.Write((byte) ((Value >> 8) & 0xFF));
        BinaryWriter.Write((byte) ((Value >> 0) & 0xFF));
    }
}