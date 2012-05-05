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

static public class StreamExtensions
{
	static public bool Eof(this Stream Stream)
	{
		return Stream.Available() <= 0;
	}

	static public TStream PreservePositionAndLock<TStream>(this TStream Stream, long Position, Action Callback) where TStream : Stream
	{
		return Stream.PreservePositionAndLock(() =>
		{
			Stream.Position = Position;
			Callback();
		});
	}

	static public TStream PreservePositionAndLock<TStream>(this TStream Stream, Action Callback) where TStream : Stream
	{
		return Stream.PreservePositionAndLock((_Stream) =>
		{
			Callback();
		});
	}

	static public TStream PreservePositionAndLock<TStream>(this TStream Stream, Action<Stream> Callback) where TStream : Stream
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

	static public long Available(this Stream Stream)
	{
		return Stream.Length - Stream.Position;
	}

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

			Buffer.WriteByte((byte)b);
		}
		return Buffer.ToArray();
	}

	static public String ReadUntilString(this Stream Stream, byte ExpectedByte, Encoding Encoding, bool IncludeExpectedByte = false)
	{
		return Encoding.GetString(Stream.ReadUntil(ExpectedByte, IncludeExpectedByte));
	}

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

	static public Stream ReadStream(this Stream Stream, long ToRead = -1)
	{
		if (ToRead == -1) ToRead = Stream.Available();
		var ReadedStream = SliceStream.CreateWithLength(Stream, Stream.Position, ToRead);
		Stream.Skip(ToRead);
		return ReadedStream;
	}

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

	static public Stream WriteBytes(this Stream Stream, byte[] Bytes)
	{
		Stream.Write(Bytes, 0, Bytes.Length);
		return Stream;
	}

	static public Stream WriteBytes(this Stream Stream, byte Byte, int RepeatCount)
	{
		var Bytes = Byte.Repeat(RepeatCount);
		Stream.Write(Bytes, 0, Bytes.Length);
		return Stream;
	}

	static public String ReadString(this Stream Stream, int ToRead, Encoding Encoding = null)
	{
		if (Encoding == null) Encoding = Encoding.UTF8;
		return Stream.ReadBytes(ToRead).GetString(Encoding);
	}

	static public String ReadStringzAt(this Stream Stream, long Offset, Encoding Encoding = null)
	{
		String Return = null;
		Stream.PreservePositionAndLock(Offset, () =>
		{
			Return = Stream.ReadStringz(-1, Encoding);
		});
		return Return;
	}

	static public String ReadStringz(this Stream Stream, int ToRead = -1, Encoding Encoding = null, bool AllowEndOfStream = true)
	{
		if (Encoding == null) Encoding = Encoding.ASCII;
		if (ToRead == -1)
		{
			var Temp = new MemoryStream();
			while (true)
			{
				int Readed = Stream.ReadByte();
				//if (Readed < 0) break;
				if (Readed < 0)
				{
					if (AllowEndOfStream) break;
					throw (new EndOfStreamException("ReadStringz reached the end of the stream without finding a \\0 character at Position=" + Stream.Position + "."));
				}
				if (Readed == 0) break;
				Temp.WriteByte((byte)Readed);
			}
			return Encoding.GetString(Temp.ToArray());
		}
		else
		{
			return Encoding.GetString(Stream.ReadBytes(ToRead)).TrimEnd('\0');
		}
	}

	static public Stream WriteStringzPair(this Stream Stream, String Value1, String Value2, Encoding Encoding = null)
	{
		Stream.WriteStringz(Value1, -1, Encoding);
		Stream.WriteStringz(Value2, -1, Encoding);
		return Stream;
	}

	static public Stream WriteString(this Stream Stream, String Value, Encoding Encoding = null)
	{
		if (Encoding == null) Encoding = Encoding.ASCII;
		Stream.WriteBytes(Encoding.GetBytes(Value));
		return Stream;
	}

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

	static public Stream WriteZeroToAlign(this Stream Stream, int Align)
	{
		Stream.WriteZeroBytes((int)(MathUtils.Align(Stream.Position, Align) - Stream.Position));
		return Stream;
	}

	static public Stream WriteZeroToOffset(this Stream Stream, long Offset)
	{
		Stream.WriteZeroBytes((int)(Offset - Stream.Position));
		return Stream;
	}

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

	public static T ReadStruct<T>(this Stream Stream) where T : struct
	{
		var Size = Marshal.SizeOf(typeof(T));
		var Buffer = Stream.ReadBytes(Size);
		return StructUtils.BytesToStruct<T>(Buffer);
	}

	public static T ReadStructPartially<T>(this Stream Stream) where T : struct
	{
		var Size = Marshal.SizeOf(typeof(T));
		var BufferPartial = Stream.ReadBytes(Math.Min((int)Stream.Available(), Size));
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

	public static TType[] ReadStructVectorAt<TType>(this Stream Stream, long Offset, uint Count, int EntrySize = -1) where TType : struct
	{
		TType[] Value = null;
		Stream.PreservePositionAndLock(() =>
		{
			Stream.Position = Offset;
			Value = Stream.ReadStructVector<TType>(Count, EntrySize);
		});
		return Value;
	}

	public static TType[] ReadStructVector<TType>(this Stream Stream, ref TType[] Vector, uint Count, int EntrySize = -1) where TType : struct
	{
		Vector = Stream.ReadStructVector<TType>(Count, EntrySize);
		return Vector;
	}

	public static TType[] ReadStructVector<TType>(this Stream Stream, uint Count, int EntrySize = -1) where TType : struct
	{
		TType[] Vector = new TType[Count];
		int SkipSize;
		if (EntrySize == -1)
		{
			SkipSize = 0;
		}
		else
		{
			SkipSize = EntrySize - Marshal.SizeOf(typeof(TType));
		}

		if (SkipSize < 0)
		{
			throw (new Exception("Invalid Size"));
		}

		for (int n = 0; n < Count; n++)
		{
			Vector[n] = ReadStruct<TType>(Stream);
			Stream.Skip(SkipSize);
		}

		return Vector;
	}

	public static T[] ReadStructVectorUntilTheEndOfStream<T>(this Stream Stream) where T : struct
	{
		var EntrySize = Marshal.SizeOf(typeof(T));
		var BytesAvailable = Stream.Available();
		//Console.WriteLine("BytesAvailable={0}/EntrySize={1}", BytesAvailable, EntrySize);
		return Stream.ReadStructVector<T>((uint)(BytesAvailable / EntrySize));
	}

	public static Stream WriteStruct<T>(this Stream Stream, T Struct) where T : struct
	{
		byte[] Bytes = StructUtils.StructToBytes(Struct);
		Stream.Write(Bytes, 0, Bytes.Length);
		return Stream;
	}

	public static Stream WriteStructVector<T>(this Stream Stream, T[] Structs) where T : struct
	{
		foreach (var Struct in Structs)
		{
			Stream.WriteStruct(Struct);
		}
		return Stream;
	}

	public static Stream Align(this Stream Stream, int Align)
	{
		Stream.Position = MathUtils.Align(Stream.Position, Align);
		return Stream;
	}

	public static Stream Skip(this Stream Stream, long Count)
	{
		Stream.Seek(Count, SeekOrigin.Current);
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
	public static void CopyToFast(this Stream FromStream, Stream ToStream)
	{
		/// ::TODO: Create a buffer and reuse it once for each thread.
		var BufferSize = Math.Min((int)FromStream.Length, 2 * 1024 * 1024);
		if (BufferSize > 0)
		{
			FromStream.CopyTo(ToStream, BufferSize);
		}
		else
		{
			FromStream.CopyTo(ToStream);
		}
	}
#endif

	public static Stream CopyToFile(this Stream Stream, String FileName)
	{
		using (var OutputFile = File.Open(FileName, FileMode.Create, FileAccess.Write))
		{
			Stream.CopyToFast(OutputFile);
		}
		return Stream;
	}

	public static void WriteStream(this Stream ToStream, Stream FromStream)
	{
		FromStream.CopyToFast(ToStream);
	}

	public static Stream SetPosition(this Stream Stream, long Position)
	{
		Stream.Position = Position;
		return Stream;
	}

	unsafe public static Stream WriteByteRepeated(this Stream Stream, byte Byte, int Count = 1)
	{
		if (Count > 0)
		{
			var Bytes = new byte[Count];
			fixed (byte* BytesPtr = &Bytes[0])
			{
				PointerUtils.Memset(BytesPtr, Byte, Count);
			}

			Stream.WriteBytes(Bytes);
		}

		return Stream;
	}

	static public Stream WriteVariableUintBit8Extends(this Stream Stream, uint Value)
	{
		do
		{
			byte Byte = (byte)(Value & 0x7F);
			Value >>= 7;
			if (Value != 0) Byte |= 0x80;
			Stream.WriteByte(Byte);
		} while (Value != 0);
		return Stream;
	}

	static public Stream WriteVariableUintBit8ExtendsArray(this Stream Stream, params uint[] Values)
	{
		foreach (var Value in Values)
		{
			Stream.WriteVariableUintBit8Extends(Value);
		}
		return Stream;
	}

	static public uint ReadVariableUintBit8Extends(this Stream Stream)
	{
		int c;
		uint v = 0;
		int shift = 0;
		do
		{
			c = Stream.ReadByte();
			if (c == -1) throw (new Exception("Incomplete VariableUintBit8Extends"));
			v |= (uint)(((uint)c & 0x7F) << shift);
			shift += 7;
		} while ((c & 0x80) != 0);
		return v;
	}

	static public uint[] ReadVariableUintBit8ExtendsArray(this Stream Stream, int Count)
	{
		uint[] Array = new uint[Count];
		for (int n = 0; n < Count; n++) Array[n] = Stream.ReadVariableUintBit8Extends();
		return Array;
	}

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
					yield return (byte)Value;
				}
			}
			finally
			{
				Stream.Position = OldPosition;
			}
		}
	}

	static public SliceStream Slice(this Stream BaseStream)
	{
		return SliceStream.CreateWithLength(BaseStream);
	}

	static public SliceStream SliceWithLength(this Stream BaseStream, long ThisStart = 0, long ThisLength = -1, bool? CanWrite = null)
	{
		return SliceStream.CreateWithLength(BaseStream, ThisStart, ThisLength, CanWrite);
	}

	static public SliceStream SliceWithBounds(this Stream BaseStream, long LowerBound, long UpperBound, bool? CanWrite = null)
	{
		return SliceStream.CreateWithBounds(BaseStream, LowerBound, UpperBound, CanWrite);
	}

	static public ConcatStream Concat(this Stream BaseStream, Stream NextStream)
	{
		return new ConcatStream(BaseStream, NextStream);
	}

	static public StreamStructArrayWrapper<TType> ConvertToStreamStructArrayWrapper<TType>(this Stream BaseStream) where TType : struct
	{
		return new StreamStructArrayWrapper<TType>(BaseStream);
	}

	static public StreamStructCachedArrayWrapper<TType> ConvertToStreamStructCachedArrayWrapper<TType>(this Stream BaseStream, int BufferCount) where TType : struct
	{
		return new StreamStructCachedArrayWrapper<TType>(BufferCount, BaseStream);
	}

	unsafe static public int ReadToPointer(this Stream Stream, byte* Pointer, int Count)
	{
		var Data = new byte[Count];
		int Result = Stream.Read(Data, 0, Count);
		PointerUtils.Memcpy(Pointer, Data, Count);
		return Result;
	}
}
