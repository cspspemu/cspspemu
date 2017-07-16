using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading;
using CSharpUtils.Streams;
using System.Diagnostics;
using CSharpUtils;
using CSharpUtils.Extensions;

/// <summary>
/// 
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <returns></returns>
    static public bool Eof(this Stream Stream)
    {
        return Stream.Available() <= 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TStream"></typeparam>
    /// <param name="Stream"></param>
    /// <param name="Position"></param>
    /// <param name="Callback"></param>
    /// <returns></returns>
    static public TStream PreservePositionAndLock<TStream>(this TStream Stream, long Position, Action Callback)
        where TStream : Stream
    {
        return Stream.PreservePositionAndLock(() =>
        {
            Stream.Position = Position;
            Callback();
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TStream"></typeparam>
    /// <param name="Stream"></param>
    /// <param name="Callback"></param>
    /// <returns></returns>
    static public TStream PreservePositionAndLock<TStream>(this TStream Stream, Action Callback) where TStream : Stream
    {
        return Stream.PreservePositionAndLock((_Stream) => { Callback(); });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TStream"></typeparam>
    /// <param name="Stream"></param>
    /// <param name="Callback"></param>
    /// <returns></returns>
    static public TStream PreservePositionAndLock<TStream>(this TStream Stream, Action<Stream> Callback)
        where TStream : Stream
    {
        if (!Stream.CanSeek)
        {
            throw(new NotImplementedException("Stream can't seek"));
        }

        lock (Stream)
        {
            var OldPosition = Stream.Position;
            {
                Callback(Stream);
            }
            Stream.Position = OldPosition;
        }
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <returns></returns>
    static public long Available(this Stream Stream)
    {
        return Stream.Length - Stream.Position;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Start"></param>
    /// <param name="Length"></param>
    /// <returns></returns>
    static public byte[] ReadChunk(this Stream Stream, int Start, int Length)
    {
        byte[] Chunk = new byte[Length];
        Stream.PreservePositionAndLock(() =>
        {
            Stream.Position = Start;
            Stream.Read(Chunk, 0, Length);
        });
        return Chunk;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="ExpectedByte"></param>
    /// <param name="IncludeExpectedByte"></param>
    /// <returns></returns>
    static public byte[] ReadUntil(this Stream Stream, byte ExpectedByte, bool IncludeExpectedByte = false)
    {
        bool Found = false;
        var Buffer = new MemoryStream();
        while (!Found)
        {
            int b = Stream.ReadByte();
            if (b == -1) throw (new Exception("End Of Stream"));

            if (b == ExpectedByte)
            {
                Found = true;
                if (!IncludeExpectedByte) break;
            }

            Buffer.WriteByte((byte) b);
        }
        return Buffer.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="ExpectedByte"></param>
    /// <param name="Encoding"></param>
    /// <param name="IncludeExpectedByte"></param>
    /// <returns></returns>
    static public String ReadUntilString(this Stream Stream, byte ExpectedByte, Encoding Encoding,
        bool IncludeExpectedByte = false)
    {
        return Encoding.GetString(Stream.ReadUntil(ExpectedByte, IncludeExpectedByte));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Encoding"></param>
    /// <param name="FromStart"></param>
    /// <returns></returns>
    static public String ReadAllContentsAsString(this Stream Stream, Encoding Encoding = null, bool FromStart = true)
    {
        if (Encoding == null) Encoding = Encoding.UTF8;
        var Data = Stream.ReadAll(FromStart);
        if (Encoding == Encoding.UTF8)
        {
            if (Data.Length >= 3 && Data[0] == 0xEF && Data[1] == 0xBB && Data[2] == 0xBF)
            {
                Data = Data.Skip(3).ToArray();
            }
        }
        return Encoding.GetString(Data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="FromStart"></param>
    /// <param name="Dispose"></param>
    /// <returns></returns>
    static public byte[] ReadAll(this Stream Stream, bool FromStart = true, bool Dispose = false)
    {
        try
        {
            var MemoryStream = new MemoryStream();

            if (FromStart && Stream.CanSeek)
            {
                //if (!Stream.CanSeek) throw (new NotImplementedException("Can't use 'FromStream' on Stream that can't seek"));
                Stream.PreservePositionAndLock(() =>
                {
                    Stream.Position = 0;
                    Stream.CopyTo(MemoryStream);
                });
            }
            else
            {
                Stream.CopyTo(MemoryStream);
            }

            return MemoryStream.ToArray();
        }
        finally
        {
            if (Dispose) Stream.Dispose();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="ToRead"></param>
    /// <returns></returns>
    static public SliceStream ReadStream(this Stream Stream, long ToRead = -1)
    {
        if (ToRead == -1) ToRead = Stream.Available();
        var ReadedStream = SliceStream.CreateWithLength(Stream, Stream.Position, ToRead);
        Stream.Skip(ToRead);
        return ReadedStream;
    }

    static public MemoryStream ReadStreamCopy(this Stream Stream, long ToRead = -1)
    {
        if (ToRead == -1) ToRead = Stream.Available();
        return new MemoryStream(Stream.ReadBytes((int) ToRead));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="ToRead"></param>
    /// <returns></returns>
    static public byte[] ReadBytes(this Stream Stream, int ToRead)
    {
        if (ToRead == 0) return new byte[0];
        var Buffer = new byte[ToRead];
        int Readed = 0;
        while (Readed < ToRead)
        {
            int ReadedNow = Stream.Read(Buffer, Readed, ToRead - Readed);
            if (ReadedNow <= 0) throw (new Exception("Unable to read " + ToRead + " bytes, readed " + Readed + "."));
            Readed += ReadedNow;
        }
        return Buffer;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="ToReadAsMax"></param>
    /// <returns></returns>
    static public byte[] ReadBytesUpTo(this Stream Stream, int ToReadAsMax)
    {
        if (ToReadAsMax == 0) return new byte[0];
        var Buffer = new byte[ToReadAsMax];
        int Readed = 0;
        while (Readed < ToReadAsMax)
        {
            int ReadedNow = Stream.Read(Buffer, Readed, ToReadAsMax - Readed);
            if (ReadedNow <= 0)
            {
                break;
            }
            Readed += ReadedNow;
        }
        return Buffer.Slice(0, Readed);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Bytes"></param>
    /// <returns></returns>
    static public TStream WriteBytes<TStream>(this TStream Stream, byte[] Bytes) where TStream : Stream
    {
        Stream.Write(Bytes, 0, Bytes.Length);
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Byte"></param>
    /// <param name="RepeatCount"></param>
    /// <returns></returns>
    static public Stream WriteBytes(this Stream Stream, byte Byte, int RepeatCount)
    {
        var Bytes = Byte.Repeat(RepeatCount);
        Stream.Write(Bytes, 0, Bytes.Length);
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="ToRead"></param>
    /// <param name="Encoding"></param>
    /// <returns></returns>
    static public String ReadString(this Stream Stream, int ToRead, Encoding Encoding = null)
    {
        if (Encoding == null) Encoding = Encoding.UTF8;
        return Stream.ReadBytes(ToRead).GetString(Encoding);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Offset"></param>
    /// <param name="Encoding"></param>
    /// <returns></returns>
    static public String ReadStringzAt(this Stream Stream, long Offset, Encoding Encoding = null)
    {
        String Return = null;
        Stream.PreservePositionAndLock(Offset, () => { Return = Stream.ReadStringz(-1, Encoding); });
        return Return;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="ToRead"></param>
    /// <param name="AlignTo4"></param>
    /// <returns></returns>
    static public int CountStringzBytes(this Stream Stream, int ToRead = -1, bool AlignTo4 = false,
        int AlignPosition = 0, bool KeepStreamPosition = true)
    {
        if (AlignTo4 == true) throw(new NotImplementedException());
        if (KeepStreamPosition) Stream = Stream.SliceWithLength(Stream.Position);
        if (Stream.Eof()) return 0;
        bool ContinueReading = false;

        if (ToRead == -1)
        {
            ToRead = 0x100;
            ContinueReading = true;
        }

        var StartPosition = Stream.Position;
        var Bytes = Stream.ReadBytesUpTo(ToRead);
        int ZeroIndex = Array.IndexOf(Bytes, (byte) 0x00);
        if (ZeroIndex == -1)
        {
            if (ContinueReading)
            {
                return Bytes.Length + CountStringzBytes(Stream, ToRead, AlignTo4, KeepStreamPosition: false);
            }
            else
            {
                return Bytes.Length;
            }
        }
        else
        {
            if (AlignTo4)
            {
                var Bytes2 = Stream.SliceWithLength(StartPosition + ZeroIndex, 5).ReadBytesUpTo(5);
                int n = 0;
                for (; n < Bytes2.Length; n++) if (Bytes2[n] != 0) break;
                return (ZeroIndex + n);
            }
            else
            {
                return (ZeroIndex + 1);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="ToRead"></param>
    /// <param name="Encoding"></param>
    /// <param name="AllowEndOfStream"></param>
    /// <returns></returns>
    static public String ReadStringz(this Stream Stream, int ToRead = -1, Encoding Encoding = null,
        bool AllowEndOfStream = true)
    {
        if (Encoding == null) Encoding = Encoding.ASCII;
        if (ToRead == -1)
        {
            var Temp = new MemoryStream();
            while (!Stream.Eof())
            {
                int Readed = Stream.ReadByte();
                //if (Readed < 0) break;
                if (Readed < 0)
                {
                    if (AllowEndOfStream) break;
                    throw (new EndOfStreamException(
                        "ReadStringz reached the end of the stream without finding a \\0 character at Position=" +
                        Stream.Position + "."));
                }
                if (Readed == 0) break;
                Temp.WriteByte((byte) Readed);
            }
            return Encoding.GetString(Temp.ToArray());
        }
        else
        {
            var Str = Encoding.GetString(Stream.ReadBytes(ToRead));
            var ZeroIndex = Str.IndexOf('\0');
            if (ZeroIndex == -1) ZeroIndex = Str.Length;
            return Str.Substring(0, ZeroIndex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Value1"></param>
    /// <param name="Value2"></param>
    /// <param name="Encoding"></param>
    /// <returns></returns>
    static public Stream WriteStringzPair(this Stream Stream, String Value1, String Value2, Encoding Encoding = null)
    {
        Stream.WriteStringz(Value1, -1, Encoding);
        Stream.WriteStringz(Value2, -1, Encoding);
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Value"></param>
    /// <param name="Encoding"></param>
    /// <returns></returns>
    static public Stream WriteString(this Stream Stream, String Value, Encoding Encoding = null)
    {
        if (Encoding == null) Encoding = Encoding.ASCII;
        Stream.WriteBytes(Encoding.GetBytes(Value));
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Value"></param>
    /// <param name="ToWrite"></param>
    /// <param name="Encoding"></param>
    /// <returns></returns>
    static public Stream WriteStringz(this Stream Stream, String Value, int ToWrite = -1, Encoding Encoding = null)
    {
        if (Encoding == null) Encoding = Encoding.ASCII;
        if (ToWrite == -1)
        {
            Stream.WriteBytes(Value.GetStringzBytes(Encoding));
        }
        else
        {
            byte[] Bytes = Encoding.GetBytes(Value);
            if (Bytes.Length > ToWrite) throw(new Exception("String too long"));
            Stream.WriteBytes(Bytes);
            Stream.WriteZeroBytes(ToWrite - Bytes.Length);
        }

        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Count"></param>
    /// <returns></returns>
    static public Stream WriteZeroBytes(this Stream Stream, int Count)
    {
        if (Count < 0)
        {
            Console.Error.WriteLine("Can't Write Negative Zero Bytes '" + Count + "'.");
            //throw (new Exception("Can't Write Negative Zero Bytes '" + Count + "'."));
        }
        else if (Count > 0)
        {
            var Bytes = new byte[Count];
            Stream.WriteBytes(Bytes);
        }
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Align"></param>
    /// <returns></returns>
    static public Stream WriteZeroToAlign(this Stream Stream, int Align)
    {
        Stream.WriteZeroBytes((int) (MathUtils.Align(Stream.Position, Align) - Stream.Position));
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Offset"></param>
    /// <returns></returns>
    static public Stream WriteZeroToOffset(this Stream Stream, long Offset)
    {
        Stream.WriteZeroBytes((int) (Offset - Stream.Position));
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <param name="Stream"></param>
    /// <returns></returns>
    public static TType ReadManagedStruct<TType>(this Stream Stream) where TType : struct
    {
        var Struct = new TType();
        var BinaryReader = new BinaryReader(Stream);
        foreach (var Field in typeof(TType).GetFields())
        {
            if (Field.FieldType == typeof(int))
            {
                Field.SetValueDirect(__makeref(Struct), BinaryReader.ReadInt32());
            }
            else if (Field.FieldType == typeof(uint))
            {
                Field.SetValueDirect(__makeref(Struct), BinaryReader.ReadUInt32());
            }
            else if (Field.FieldType == typeof(string))
            {
                Field.SetValueDirect(__makeref(Struct), Stream.ReadStringz());
            }
            else
            {
                throw(new NotImplementedException());
            }
        }
        return Struct;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Stream"></param>
    /// <returns></returns>
    public static T ReadStruct<T>(this Stream Stream) where T : struct
    {
        var Size = Marshal.SizeOf(typeof(T));
        var Buffer = Stream.ReadBytes(Size);
        return StructUtils.BytesToStruct<T>(Buffer);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Stream"></param>
    /// <returns></returns>
    public static T ReadStructPartially<T>(this Stream Stream) where T : struct
    {
        var Size = Marshal.SizeOf(typeof(T));
        var BufferPartial = Stream.ReadBytes(Math.Min((int) Stream.Available(), Size));
        byte[] Buffer;
        if (BufferPartial.Length < Size)
        {
            Buffer = new byte[Size];
            BufferPartial.CopyTo(Buffer, 0);
        }
        else
        {
            Buffer = BufferPartial;
        }
        return StructUtils.BytesToStruct<T>(Buffer);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <param name="Stream"></param>
    /// <param name="Offset"></param>
    /// <param name="Count"></param>
    /// <param name="EntrySize"></param>
    /// <returns></returns>
    public static TType[] ReadStructVectorAt<TType>(this Stream Stream, long Offset, uint Count, int EntrySize = -1)
        where TType : struct
    {
        TType[] Value = null;
        Stream.PreservePositionAndLock(() =>
        {
            Stream.Position = Offset;
            Value = Stream.ReadStructVector<TType>(Count, EntrySize);
        });
        return Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <param name="Stream"></param>
    /// <param name="Vector"></param>
    /// <param name="Count"></param>
    /// <param name="EntrySize"></param>
    /// <returns></returns>
    public static TType[] ReadStructVector<TType>(this Stream Stream, ref TType[] Vector, uint Count,
        int EntrySize = -1) where TType : struct
    {
        Vector = Stream.ReadStructVector<TType>(Count, EntrySize);
        return Vector;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <param name="Stream"></param>
    /// <param name="Count"></param>
    /// <param name="EntrySize"></param>
    /// <param name="AllowReadLess"></param>
    /// <returns></returns>
    public static TType[] ReadStructVector<TType>(this Stream Stream, uint Count, int EntrySize = -1,
        bool AllowReadLess = false) where TType : struct
    {
        if (Count == 0) return new TType[0];

        var ItemSize = Marshal.SizeOf(typeof(TType));
        var SkipSize = (EntrySize == -1) ? (0) : (EntrySize - ItemSize);

        var MaxCount = (uint) (Stream.Length / (ItemSize + SkipSize));
        if (AllowReadLess)
        {
            Count = Math.Min(MaxCount, Count);
        }

        if (SkipSize < 0)
        {
            throw (new Exception("Invalid Size"));
        }
        else if (SkipSize == 0)
        {
            return StructUtils.BytesToStructArray<TType>(Stream.ReadBytes((int) (ItemSize * Count)));
        }
        else
        {
            TType[] Vector = new TType[Count];

            for (int n = 0; n < Count; n++)
            {
                Vector[n] = ReadStruct<TType>(Stream);
                Stream.Skip(SkipSize);
            }

            return Vector;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Stream"></param>
    /// <returns></returns>
    public static T[] ReadStructVectorUntilTheEndOfStream<T>(this Stream Stream) where T : struct
    {
        var EntrySize = Marshal.SizeOf(typeof(T));
        var BytesAvailable = Stream.Available();
        //Console.WriteLine("BytesAvailable={0}/EntrySize={1}", BytesAvailable, EntrySize);
        return Stream.ReadStructVector<T>((uint) (BytesAvailable / EntrySize));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Stream"></param>
    /// <param name="Struct"></param>
    /// <returns></returns>
    public static Stream WriteStruct<T>(this Stream Stream, T Struct) where T : struct
    {
        byte[] Bytes = StructUtils.StructToBytes(Struct);
        Stream.Write(Bytes, 0, Bytes.Length);
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Stream"></param>
    /// <param name="Structs"></param>
    /// <returns></returns>
    public static TStream WriteStructVector<T, TStream>(this TStream Stream, T[] Structs, int Count = -1)
        where T : struct
        where TStream : Stream
    {
        Stream.WriteBytes(StructUtils.StructArrayToBytes(Structs, Count));
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Align"></param>
    /// <returns></returns>
    public static Stream Align(this Stream Stream, int Align)
    {
        Stream.Position = MathUtils.Align(Stream.Position, Align);
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Count"></param>
    /// <returns></returns>
    public static Stream Skip(this Stream Stream, long Count)
    {
        if (Count != 0)
        {
            if (!Stream.CanSeek)
            {
                if (Count < 0) throw(new NotImplementedException("Can't go back"));
                Stream.ReadBytes((int) Count);
            }
            else
            {
                Stream.Seek(Count, SeekOrigin.Current);
            }
        }
        return Stream;
    }

#if false
	static ThreadLocal<byte[]> PerThreadBuffer = new ThreadLocal<byte[]>(() =>
	{
		return new byte[2 * 1024 * 1024];
	});

	public static void CopyToFast(this Stream FromStream, Stream ToStream)
	{
		//var SliceFromStream = new SliceStream(FromStream);
		var SliceFromStream = FromStream;
		while (true)
		{
			int ReadedBytesCount = SliceFromStream.Read(PerThreadBuffer.Value, 0, PerThreadBuffer.Value.Length);
			Console.WriteLine(ReadedBytesCount);
			ToStream.Write(PerThreadBuffer.Value, 0, ReadedBytesCount);
			if (ReadedBytesCount < PerThreadBuffer.Value.Length) break;
		}
		//SliceFromStream.Dispose();
	}
#else
    /// <summary>
    /// 
    /// </summary>
    /// <param name="FromStream"></param>
    /// <param name="ToStream"></param>
    public static void CopyToFast(this Stream FromStream, Stream ToStream, Action<long, long> ActionReport = null)
    {
        var BufferSize = (int) Math.Min((long) FromStream.Length, (long) (2 * 1024 * 1024));
        var Buffer = new byte[BufferSize];
        CopyToFast(FromStream, ToStream, Buffer, ActionReport);
        Buffer = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="FromStream"></param>
    /// <param name="ToStream"></param>
    public static void CopyToFast(this Stream FromStream, Stream ToStream, byte[] Buffer,
        Action<long, long> ActionReport = null)
    {
        /// ::TODO: Create a buffer and reuse it once for each thread.
        if (ActionReport == null) ActionReport = (Current, Max) => { };
        long TotalBytes = FromStream.Available();
        long CurrentBytes = 0;
        ActionReport(CurrentBytes, TotalBytes);
        while (true)
        {
            int Readed = FromStream.Read(Buffer, 0, Buffer.Length);
            if (Readed <= 0) break;
            ToStream.Write(Buffer, 0, Readed);
            CurrentBytes += Readed;
            ActionReport(CurrentBytes, TotalBytes);
        }
        Buffer = null;
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="FileName"></param>
    /// <returns></returns>
    public static Stream CopyToFile(this Stream Stream, String FileName)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FileName));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        using (var OutputFile = File.Open(FileName, FileMode.Create, FileAccess.Write))
        {
            Stream.CopyToFast(OutputFile);
        }
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ToStream"></param>
    /// <param name="FromStream"></param>
    public static Stream WriteStream(this Stream ToStream, Stream FromStream, Action<long, long> ActionReport = null)
    {
        FromStream.CopyToFast(ToStream, ActionReport);
        return ToStream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Position"></param>
    /// <returns></returns>
    public static Stream SetPosition(this Stream Stream, long Position)
    {
        Stream.Position = Position;
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Byte"></param>
    /// <returns></returns>
    unsafe public static Stream FillStreamWithByte(this Stream Stream, byte Byte)
    {
        Stream.WriteByteRepeated(Byte, (int) (Stream.Length - Stream.Position));
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Byte"></param>
    /// <param name="TotalCount"></param>
    /// <returns></returns>
    unsafe public static Stream WriteByteRepeated(this Stream Stream, byte Byte, long TotalCount,
        Action<long, long> Progress = null)
    {
        if (TotalCount > 0)
        {
            long MaxBuffer = 2 * 1024 * 1024;
            var Bytes = new byte[Math.Min(MaxBuffer, TotalCount)];
            PointerUtils.Memset(Bytes, Byte, Bytes.Length);
            long Left = TotalCount;
            long Current = 0;
            if (Progress == null) Progress = (_Current, _Max) => { };

            Progress(0, TotalCount);

            while (Left > 0)
            {
                var ToWrite = Math.Min(Bytes.Length, Left);
                Stream.Write(Bytes, 0, (int) ToWrite);
                Left -= ToWrite;
                Current += ToWrite;
                Progress(Current, TotalCount);
            }
        }

        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Byte"></param>
    /// <param name="PositionStop"></param>
    /// <returns></returns>
    unsafe public static Stream WriteByteRepeatedTo(this Stream Stream, byte Byte, long PositionStop,
        Action<long, long> Progress = null)
    {
        return WriteByteRepeated(Stream, Byte, (int) (PositionStop - Stream.Position), Progress);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    static public Stream WriteVariableUintBit8Extends(this Stream Stream, uint Value)
    {
        do
        {
            byte Byte = (byte) (Value & 0x7F);
            Value >>= 7;
            if (Value != 0) Byte |= 0x80;
            Stream.WriteByte(Byte);
        } while (Value != 0);
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    static public Stream WriteVariableUlongBit8Extends(this Stream Stream, ulong Value)
    {
        do
        {
            byte Byte = (byte) (Value & 0x7F);
            Value >>= 7;
            if (Value != 0) Byte |= 0x80;
            Stream.WriteByte(Byte);
        } while (Value != 0);
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Values"></param>
    /// <returns></returns>
    static public Stream WriteVariableUintBit8ExtendsArray(this Stream Stream, params uint[] Values)
    {
        foreach (var Value in Values)
        {
            Stream.WriteVariableUintBit8Extends(Value);
        }
        return Stream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <returns></returns>
    static public uint ReadVariableUintBit8Extends(this Stream Stream)
    {
        int c;
        uint v = 0;
        int shift = 0;
        do
        {
            c = Stream.ReadByte();
            if (c == -1) throw (new Exception("Incomplete VariableUintBit8Extends"));
            v |= (uint) (((uint) c & 0x7F) << shift);
            shift += 7;
        } while ((c & 0x80) != 0);
        return v;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <returns></returns>
    static public ulong ReadVariableUlongBit8Extends(this Stream Stream)
    {
        int c;
        ulong v = 0;
        int shift = 0;
        do
        {
            c = Stream.ReadByte();
            if (c == -1) throw (new Exception("Incomplete VariableUintBit8Extends"));
            v |= (ulong) (((ulong) c & 0x7F) << shift);
            shift += 7;
        } while ((c & 0x80) != 0);
        return v;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Count"></param>
    /// <returns></returns>
    static public uint[] ReadVariableUintBit8ExtendsArray(this Stream Stream, int Count)
    {
        uint[] Array = new uint[Count];
        for (int n = 0; n < Count; n++) Array[n] = Stream.ReadVariableUintBit8Extends();
        return Array;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <returns></returns>
    static public IEnumerable<byte> AsByteEnumerable(this Stream Stream)
    {
        lock (Stream)
        {
            var OldPosition = Stream.Position;
            try
            {
                while (true)
                {
                    int Value = Stream.ReadByte();
                    if (Value == -1)
                    {
                        break;
                    }
                    yield return (byte) Value;
                }
            }
            finally
            {
                Stream.Position = OldPosition;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="BaseStream"></param>
    /// <returns></returns>
    static public SliceStream Slice(this Stream BaseStream)
    {
        return SliceStream.CreateWithLength(BaseStream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="BaseStream"></param>
    /// <param name="ThisStart"></param>
    /// <param name="ThisLength"></param>
    /// <param name="CanWrite"></param>
    /// <returns></returns>
    static public SliceStream SliceWithLength(this Stream BaseStream, long ThisStart = 0, long ThisLength = -1,
        bool? CanWrite = null)
    {
        return SliceStream.CreateWithLength(BaseStream, ThisStart, ThisLength, CanWrite);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="BaseStream"></param>
    /// <param name="LowerBound"></param>
    /// <param name="UpperBound"></param>
    /// <param name="CanWrite"></param>
    /// <returns></returns>
    static public SliceStream SliceWithBounds(this Stream BaseStream, long LowerBound, long UpperBound,
        bool? CanWrite = null)
    {
        return SliceStream.CreateWithBounds(BaseStream, LowerBound, UpperBound, CanWrite);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="BaseStream"></param>
    /// <param name="NextStream"></param>
    /// <returns></returns>
    static public ConcatStream Concat(this Stream BaseStream, Stream NextStream)
    {
        return new ConcatStream(BaseStream, NextStream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <param name="BaseStream"></param>
    /// <returns></returns>
    static public StreamStructArrayWrapper<TType> ConvertToStreamStructArrayWrapper<TType>(this Stream BaseStream)
        where TType : struct
    {
        return new StreamStructArrayWrapper<TType>(BaseStream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <param name="BaseStream"></param>
    /// <param name="BufferCount"></param>
    /// <returns></returns>
    static public StreamStructCachedArrayWrapper<TType> ConvertToStreamStructCachedArrayWrapper<TType>(
        this Stream BaseStream, int BufferCount) where TType : struct
    {
        return new StreamStructCachedArrayWrapper<TType>(BufferCount, BaseStream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="Pointer"></param>
    /// <param name="Count"></param>
    /// <returns></returns>
    unsafe static public int ReadToPointer(this Stream Stream, byte* Pointer, int Count)
    {
        var Data = new byte[Count];
        int Result = Stream.Read(Data, 0, Count);
        Marshal.Copy(Data, 0, new IntPtr(Pointer), Count);
        return Result;
    }
}