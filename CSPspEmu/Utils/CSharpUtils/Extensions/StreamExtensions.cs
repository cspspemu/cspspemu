using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils.Streams;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool Eof(this Stream stream)
        {
            return stream.Available() <= 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TStream"></typeparam>
        /// <param name="stream"></param>
        /// <param name="position"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static TStream PreservePositionAndLock<TStream>(this TStream stream, long position, Action callback)
            where TStream : Stream
        {
            return stream.PreservePositionAndLock(() =>
            {
                stream.Position = position;
                callback();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TStream"></typeparam>
        /// <param name="stream"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static TStream PreservePositionAndLock<TStream>(this TStream stream, Action callback)
            where TStream : Stream
        {
            return stream.PreservePositionAndLock(str => { callback(); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TStream"></typeparam>
        /// <param name="stream"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static TStream PreservePositionAndLock<TStream>(this TStream stream, Action<Stream> callback)
            where TStream : Stream
        {
            if (!stream.CanSeek)
            {
                throw new NotImplementedException("Stream can't seek");
            }

            lock (stream)
            {
                var oldPosition = stream.Position;
                {
                    callback(stream);
                }
                stream.Position = oldPosition;
            }
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static long Available(this Stream stream)
        {
            return stream.Length - stream.Position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] ReadChunk(this Stream stream, int start, int length)
        {
            var chunk = new byte[length];
            stream.PreservePositionAndLock(() =>
            {
                stream.Position = start;
                stream.Read(chunk, 0, length);
            });
            return chunk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="expectedByte"></param>
        /// <param name="includeExpectedByte"></param>
        /// <returns></returns>
        public static byte[] ReadUntil(this Stream stream, byte expectedByte, bool includeExpectedByte = false)
        {
            var found = false;
            var buffer = new MemoryStream();
            while (!found)
            {
                var b = stream.ReadByte();
                if (b == -1) throw new Exception("End Of Stream");

                if (b == expectedByte)
                {
                    found = true;
                    if (!includeExpectedByte) break;
                }

                buffer.WriteByte((byte) b);
            }
            return buffer.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="expectedByte"></param>
        /// <param name="encoding"></param>
        /// <param name="includeExpectedByte"></param>
        /// <returns></returns>
        public static string ReadUntilString(this Stream stream, byte expectedByte, Encoding encoding,
            bool includeExpectedByte = false)
        {
            return encoding.GetString(stream.ReadUntil(expectedByte, includeExpectedByte));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <param name="fromStart"></param>
        /// <returns></returns>
        public static string ReadAllContentsAsString(this Stream stream, Encoding encoding = null,
            bool fromStart = true)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var data = stream.ReadAll(fromStart);
            if (Equals(encoding, Encoding.UTF8))
            {
                if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
                {
                    data = data.Skip(3).ToArray();
                }
            }
            return encoding.GetString(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fromStart"></param>
        /// <param name="dispose"></param>
        /// <returns></returns>
        public static byte[] ReadAll(this Stream stream, bool fromStart = true, bool dispose = false)
        {
            try
            {
                var memoryStream = new MemoryStream();

                if (fromStart && stream.CanSeek)
                {
                    //if (!Stream.CanSeek) throw (new NotImplementedException("Can't use 'FromStream' on Stream that can't seek"));
                    stream.PreservePositionAndLock(() =>
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        stream.Position = 0;
                        // ReSharper disable once AccessToDisposedClosure
                        stream.CopyTo(memoryStream);
                    });
                }
                else
                {
                    stream.CopyTo(memoryStream);
                }

                return memoryStream.ToArray();
            }
            finally
            {
                if (dispose) stream.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="toRead"></param>
        /// <returns></returns>
        public static SliceStream ReadStream(this Stream stream, long toRead = -1)
        {
            if (toRead == -1) toRead = stream.Available();
            var readedStream = SliceStream.CreateWithLength(stream, stream.Position, toRead);
            stream.Skip(toRead);
            return readedStream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="toRead"></param>
        /// <returns></returns>
        public static MemoryStream ReadStreamCopy(this Stream stream, long toRead = -1)
        {
            if (toRead == -1) toRead = stream.Available();
            return new MemoryStream(stream.ReadBytes((int) toRead));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="toRead"></param>
        /// <returns></returns>
        public static byte[] ReadBytes(this Stream stream, int toRead)
        {
            if (toRead == 0) return new byte[0];
            var buffer = new byte[toRead];
            var readed = 0;
            while (readed < toRead)
            {
                var readedNow = stream.Read(buffer, readed, toRead - readed);
                if (readedNow <= 0)
                    throw new Exception("Unable to read " + toRead + " bytes, readed " + readed + ".");
                readed += readedNow;
            }
            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="toReadAsMax"></param>
        /// <returns></returns>
        public static byte[] ReadBytesUpTo(this Stream stream, int toReadAsMax)
        {
            if (toReadAsMax == 0) return new byte[0];
            var buffer = new byte[toReadAsMax];
            var readed = 0;
            while (readed < toReadAsMax)
            {
                var readedNow = stream.Read(buffer, readed, toReadAsMax - readed);
                if (readedNow <= 0)
                {
                    break;
                }
                readed += readedNow;
            }
            return buffer.Slice(0, readed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static TStream WriteBytes<TStream>(this TStream stream, byte[] bytes) where TStream : Stream
        {
            stream.Write(bytes, 0, bytes.Length);
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="Byte"></param>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public static Stream WriteBytes(this Stream stream, byte Byte, int repeatCount)
        {
            var bytes = Byte.Repeat(repeatCount);
            stream.Write(bytes, 0, bytes.Length);
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="toRead"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ReadString(this Stream stream, int toRead, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return stream.ReadBytes(toRead).GetString(encoding);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ReadStringzAt(this Stream stream, long offset, Encoding encoding = null)
        {
            string Return = null;
            stream.PreservePositionAndLock(offset, () => { Return = stream.ReadStringz(-1, encoding); });
            return Return;
        }

        /// <summary>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="toRead"></param>
        /// <param name="alignTo4"></param>
        /// <param name="alignPosition"></param>
        /// <param name="keepStreamPosition"></param>
        /// <returns></returns>
        public static int CountStringzBytes(this Stream stream, int toRead = -1, bool alignTo4 = false,
            int alignPosition = 0, bool keepStreamPosition = true)
        {
            if (keepStreamPosition) stream = stream.SliceWithLength(stream.Position);
            if (stream.Eof()) return 0;
            var continueReading = false;

            if (toRead == -1)
            {
                toRead = 0x100;
                continueReading = true;
            }

            var startPosition = stream.Position;
            var bytes = stream.ReadBytesUpTo(toRead);
            var zeroIndex = Array.IndexOf(bytes, (byte) 0x00);
            if (zeroIndex == -1)
            {
                if (continueReading)
                {
                    return bytes.Length + CountStringzBytes(stream, toRead, alignTo4, keepStreamPosition: false);
                }
                return bytes.Length;
            }

            if (alignTo4)
            {
                var bytes2 = stream.SliceWithLength(startPosition + zeroIndex, 5).ReadBytesUpTo(5);
                var n = 0;
                for (; n < bytes2.Length; n++) if (bytes2[n] != 0) break;
                return zeroIndex + n;
            }
            return zeroIndex + 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="toRead"></param>
        /// <param name="encoding"></param>
        /// <param name="allowEndOfStream"></param>
        /// <returns></returns>
        public static string ReadStringz(this Stream stream, int toRead = -1, Encoding encoding = null,
            bool allowEndOfStream = true)
        {
            if (encoding == null) encoding = Encoding.ASCII;
            if (toRead == -1)
            {
                var temp = new MemoryStream();
                while (!stream.Eof())
                {
                    var readed = stream.ReadByte();
                    //if (Readed < 0) break;
                    if (readed < 0)
                    {
                        if (allowEndOfStream) break;
                        throw new EndOfStreamException(
                            "ReadStringz reached the end of the stream without finding a \\0 character at Position=" +
                            stream.Position + ".");
                    }
                    if (readed == 0) break;
                    temp.WriteByte((byte) readed);
                }
                return encoding.GetString(temp.ToArray());
            }

            var str = encoding.GetString(stream.ReadBytes(toRead));
            var zeroIndex = str.IndexOf('\0');
            if (zeroIndex == -1) zeroIndex = str.Length;
            return str.Substring(0, zeroIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Stream WriteStringzPair(this Stream stream, string value1, string value2,
            Encoding encoding = null)
        {
            stream.WriteStringz(value1, -1, encoding);
            stream.WriteStringz(value2, -1, encoding);
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Stream WriteString(this Stream stream, string value, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.ASCII;
            stream.WriteBytes(encoding.GetBytes(value));
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="toWrite"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Stream WriteStringz(this Stream stream, string value, int toWrite = -1, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.ASCII;
            if (toWrite == -1)
            {
                stream.WriteBytes(value.GetStringzBytes(encoding));
            }
            else
            {
                var bytes = encoding.GetBytes(value);
                if (bytes.Length > toWrite) throw new Exception("String too long");
                stream.WriteBytes(bytes);
                stream.WriteZeroBytes(toWrite - bytes.Length);
            }

            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Stream WriteZeroBytes(this Stream stream, int count)
        {
            if (count < 0)
            {
                Console.Error.WriteLine("Can't Write Negative Zero Bytes '" + count + "'.");
                //throw (new Exception("Can't Write Negative Zero Bytes '" + Count + "'."));
            }
            else if (count > 0)
            {
                var bytes = new byte[count];
                stream.WriteBytes(bytes);
            }
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        public static Stream WriteZeroToAlign(this Stream stream, int align)
        {
            stream.WriteZeroBytes((int) (MathUtils.Align(stream.Position, align) - stream.Position));
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Stream WriteZeroToOffset(this Stream stream, long offset)
        {
            stream.WriteZeroBytes((int) (offset - stream.Position));
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static TType ReadManagedStruct<TType>(this Stream stream) where TType : struct
        {
            var Struct = new TType();
            var binaryReader = new BinaryReader(stream);
            foreach (var field in typeof(TType).GetFields())
            {
                if (field.FieldType == typeof(int))
                {
                    field.SetValueDirect(__makeref(Struct), binaryReader.ReadInt32());
                }
                else if (field.FieldType == typeof(uint))
                {
                    field.SetValueDirect(__makeref(Struct), binaryReader.ReadUInt32());
                }
                else if (field.FieldType == typeof(string))
                {
                    field.SetValueDirect(__makeref(Struct), stream.ReadStringz());
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return Struct;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T ReadStruct<T>(this Stream stream) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var buffer = stream.ReadBytes(size);
            return StructUtils.BytesToStruct<T>(buffer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T ReadStructPartially<T>(this Stream stream) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var bufferPartial = stream.ReadBytes(Math.Min((int) stream.Available(), size));
            byte[] buffer;
            if (bufferPartial.Length < size)
            {
                buffer = new byte[size];
                bufferPartial.CopyTo(buffer, 0);
            }
            else
            {
                buffer = bufferPartial;
            }
            return StructUtils.BytesToStruct<T>(buffer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="entrySize"></param>
        /// <returns></returns>
        public static TType[] ReadStructVectorAt<TType>(this Stream stream, long offset, uint count, int entrySize = -1)
            where TType : struct
        {
            TType[] value = null;
            stream.PreservePositionAndLock(() =>
            {
                stream.Position = offset;
                value = stream.ReadStructVector<TType>(count, entrySize);
            });
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="stream"></param>
        /// <param name="vector"></param>
        /// <param name="count"></param>
        /// <param name="entrySize"></param>
        /// <returns></returns>
        public static TType[] ReadStructVector<TType>(this Stream stream, out TType[] vector, uint count,
            int entrySize = -1) where TType : struct
        {
            vector = stream.ReadStructVector<TType>(count, entrySize);
            return vector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="stream"></param>
        /// <param name="count"></param>
        /// <param name="entrySize"></param>
        /// <param name="allowReadLess"></param>
        /// <returns></returns>
        public static TType[] ReadStructVector<TType>(this Stream stream, uint count, int entrySize = -1,
            bool allowReadLess = false) where TType : struct
        {
            if (count == 0) return new TType[0];

            var itemSize = Marshal.SizeOf(typeof(TType));
            var skipSize = entrySize == -1 ? 0 : entrySize - itemSize;

            var maxCount = (uint) (stream.Length / (itemSize + skipSize));
            if (allowReadLess)
            {
                count = Math.Min(maxCount, count);
            }

            if (skipSize < 0)
                throw new Exception("Invalid Size");
            if (skipSize == 0)
                return StructUtils.BytesToStructArray<TType>(stream.ReadBytes((int) (itemSize * count)));
            var vector = new TType[count];

            for (var n = 0; n < count; n++)
            {
                vector[n] = ReadStruct<TType>(stream);
                stream.Skip(skipSize);
            }

            return vector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T[] ReadStructVectorUntilTheEndOfStream<T>(this Stream stream) where T : struct
        {
            var entrySize = Marshal.SizeOf(typeof(T));
            var bytesAvailable = stream.Available();
            //Console.WriteLine("BytesAvailable={0}/EntrySize={1}", BytesAvailable, EntrySize);
            return stream.ReadStructVector<T>((uint) (bytesAvailable / entrySize));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="Struct"></param>
        /// <returns></returns>
        public static Stream WriteStruct<T>(this Stream stream, T Struct) where T : struct
        {
            var bytes = StructUtils.StructToBytes(Struct);
            stream.Write(bytes, 0, bytes.Length);
            return stream;
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TStream"></typeparam>
        /// <param name="stream"></param>
        /// <param name="structs"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TStream WriteStructVector<T, TStream>(this TStream stream, T[] structs, int count = -1)
            where T : struct
            where TStream : Stream
        {
            stream.WriteBytes(StructUtils.StructArrayToBytes(structs, count));
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        public static Stream Align(this Stream stream, int align)
        {
            stream.Position = MathUtils.Align(stream.Position, align);
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Stream Skip(this Stream stream, long count)
        {
            if (count != 0)
            {
                if (!stream.CanSeek)
                {
                    if (count < 0) throw new NotImplementedException("Can't go back");
                    stream.ReadBytes((int) count);
                }
                else
                {
                    stream.Seek(count, SeekOrigin.Current);
                }
            }
            return stream;
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
        /// </summary>
        /// <param name="fromStream"></param>
        /// <param name="toStream"></param>
        /// <param name="actionReport"></param>
        public static void CopyToFast(this Stream fromStream, Stream toStream, Action<long, long> actionReport = null)
        {
            var bufferSize = (int) Math.Min(fromStream.Length, 2 * 1024 * 1024);
            var buffer = new byte[bufferSize];
            CopyToFast(fromStream, toStream, buffer, actionReport);
            //buffer = null;
        }

        /// <summary>
        /// </summary>
        /// <param name="fromStream"></param>
        /// <param name="toStream"></param>
        /// <param name="buffer"></param>
        /// <param name="actionReport"></param>
        public static void CopyToFast(this Stream fromStream, Stream toStream, byte[] buffer,
            Action<long, long> actionReport = null)
        {
            /// ::TODO: Create a buffer and reuse it once for each thread.
            if (actionReport == null) actionReport = (current, max) => { };
            var totalBytes = fromStream.Available();
            var currentBytes = 0;
            actionReport(currentBytes, totalBytes);
            while (true)
            {
                var readed = fromStream.Read(buffer, 0, buffer.Length);
                if (readed <= 0) break;
                toStream.Write(buffer, 0, readed);
                currentBytes += readed;
                actionReport(currentBytes, totalBytes);
            }
            //buffer = null;
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Stream CopyToFile(this Stream stream, string fileName)
        {
            try
            {
                var directoryName = Path.GetDirectoryName(fileName);
                if (directoryName != null) Directory.CreateDirectory(directoryName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            using (var outputFile = File.Open(fileName, FileMode.Create, FileAccess.Write))
            {
                stream.CopyToFast(outputFile);
            }
            return stream;
        }

        /// <summary>
        /// </summary>
        /// <param name="toStream"></param>
        /// <param name="fromStream"></param>
        /// <param name="actionReport"></param>
        public static Stream WriteStream(this Stream toStream, Stream fromStream,
            Action<long, long> actionReport = null)
        {
            fromStream.CopyToFast(toStream, actionReport);
            return toStream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Stream SetPosition(this Stream stream, long position)
        {
            stream.Position = position;
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="Byte"></param>
        /// <returns></returns>
        public static Stream FillStreamWithByte(this Stream stream, byte Byte)
        {
            stream.WriteByteRepeated(Byte, (int) (stream.Length - stream.Position));
            return stream;
        }

        /// <summary>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="Byte"></param>
        /// <param name="totalCount"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static Stream WriteByteRepeated(this Stream stream, byte Byte, long totalCount,
            Action<long, long> progress = null)
        {
            if (totalCount > 0)
            {
                long maxBuffer = 2 * 1024 * 1024;
                var bytes = new byte[Math.Min(maxBuffer, totalCount)];
                PointerUtils.Memset(bytes, Byte, bytes.Length);
                var left = totalCount;
                long current = 0;
                if (progress == null) progress = (cur, max) => { };

                progress(0, totalCount);

                while (left > 0)
                {
                    var toWrite = Math.Min(bytes.Length, left);
                    stream.Write(bytes, 0, (int) toWrite);
                    left -= toWrite;
                    current += toWrite;
                    progress(current, totalCount);
                }
            }

            return stream;
        }

        /// <summary>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="Byte"></param>
        /// <param name="positionStop"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static Stream WriteByteRepeatedTo(this Stream stream, byte Byte, long positionStop,
            Action<long, long> progress = null)
        {
            return WriteByteRepeated(stream, Byte, (int) (positionStop - stream.Position), progress);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Stream WriteVariableUintBit8Extends(this Stream stream, uint value)
        {
            do
            {
                byte Byte = (byte) (value & 0x7F);
                value >>= 7;
                if (value != 0) Byte |= 0x80;
                stream.WriteByte(Byte);
            } while (value != 0);
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Stream WriteVariableUlongBit8Extends(this Stream stream, ulong value)
        {
            do
            {
                byte Byte = (byte) (value & 0x7F);
                value >>= 7;
                if (value != 0) Byte |= 0x80;
                stream.WriteByte(Byte);
            } while (value != 0);
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Stream WriteVariableUintBit8ExtendsArray(this Stream stream, params uint[] values)
        {
            foreach (var value in values)
            {
                stream.WriteVariableUintBit8Extends(value);
            }
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static uint ReadVariableUintBit8Extends(this Stream stream)
        {
            int c;
            uint v = 0;
            int shift = 0;
            do
            {
                c = stream.ReadByte();
                if (c == -1) throw new Exception("Incomplete VariableUintBit8Extends");
                v |= ((uint) c & 0x7F) << shift;
                shift += 7;
            } while ((c & 0x80) != 0);
            return v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static ulong ReadVariableUlongBit8Extends(this Stream stream)
        {
            int c;
            ulong v = 0;
            int shift = 0;
            do
            {
                c = stream.ReadByte();
                if (c == -1) throw new Exception("Incomplete VariableUintBit8Extends");
                v |= ((ulong) c & 0x7F) << shift;
                shift += 7;
            } while ((c & 0x80) != 0);
            return v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static uint[] ReadVariableUintBit8ExtendsArray(this Stream stream, int count)
        {
            var array = new uint[count];
            for (var n = 0; n < count; n++) array[n] = stream.ReadVariableUintBit8Extends();
            return array;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IEnumerable<byte> AsByteEnumerable(this Stream stream)
        {
            lock (stream)
            {
                var oldPosition = stream.Position;
                try
                {
                    while (true)
                    {
                        var value = stream.ReadByte();
                        if (value == -1)
                        {
                            break;
                        }
                        yield return (byte) value;
                    }
                }
                finally
                {
                    stream.Position = oldPosition;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseStream"></param>
        /// <returns></returns>
        public static SliceStream Slice(this Stream baseStream)
        {
            return SliceStream.CreateWithLength(baseStream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseStream"></param>
        /// <param name="thisStart"></param>
        /// <param name="thisLength"></param>
        /// <param name="canWrite"></param>
        /// <returns></returns>
        public static SliceStream SliceWithLength(this Stream baseStream, long thisStart = 0, long thisLength = -1,
            bool? canWrite = null)
        {
            return SliceStream.CreateWithLength(baseStream, thisStart, thisLength, canWrite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseStream"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="canWrite"></param>
        /// <returns></returns>
        public static SliceStream SliceWithBounds(this Stream baseStream, long lowerBound, long upperBound,
            bool? canWrite = null)
        {
            return SliceStream.CreateWithBounds(baseStream, lowerBound, upperBound, canWrite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseStream"></param>
        /// <param name="nextStream"></param>
        /// <returns></returns>
        public static ConcatStream Concat(this Stream baseStream, Stream nextStream)
        {
            return new ConcatStream(baseStream, nextStream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="baseStream"></param>
        /// <returns></returns>
        public static StreamStructArrayWrapper<TType> ConvertToStreamStructArrayWrapper<TType>(this Stream baseStream)
            where TType : struct
        {
            return new StreamStructArrayWrapper<TType>(baseStream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="baseStream"></param>
        /// <param name="bufferCount"></param>
        /// <returns></returns>
        public static StreamStructCachedArrayWrapper<TType> ConvertToStreamStructCachedArrayWrapper<TType>(
            this Stream baseStream, int bufferCount) where TType : struct
        {
            return new StreamStructCachedArrayWrapper<TType>(bufferCount, baseStream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pointer"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static unsafe int ReadToPointer(this Stream stream, byte* pointer, int count)
        {
            var data = new byte[count];
            var result = stream.Read(data, 0, count);
            Marshal.Copy(data, 0, new IntPtr(pointer), count);
            return result;
        }
    }
}