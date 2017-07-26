//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  This material may not be duplicated in whole or in part, except for 
//  personal use, without the express written consent of the author. 
//
//  Email:  ianier@hotmail.com
//
//  Copyright (C) 1999-2003 Ianier Munoz. All Rights Reserved.

using System;
using System.IO;

namespace CSPspEmu.Core.Audio.Impl.WaveOut.WaveLib
{
    public class WaveStream : Stream
    {
        private Stream m_Stream;
        private long _mDataPos;
        private long _mLength;

        private WaveFormat _mFormat;

        public WaveFormat Format => _mFormat;

        private static string ReadChunk(BinaryReader reader)
        {
            byte[] ch = new byte[4];
            reader.Read(ch, 0, ch.Length);
            return System.Text.Encoding.ASCII.GetString(ch);
        }

        private void ReadHeader()
        {
            var reader = new BinaryReader(m_Stream);
            if (ReadChunk(reader) != "RIFF") throw new Exception("Invalid file format");

            reader.ReadInt32(); // File length minus first 8 bytes of RIFF description, we don't use it

            if (ReadChunk(reader) != "WAVE") throw new Exception("Invalid file format");
            if (ReadChunk(reader) != "fmt ") throw new Exception("Invalid file format");

            var len = reader.ReadInt32();
            if (len < 16) throw new Exception("Invalid file format"); // bad format chunk length

            _mFormat = new WaveFormat(22050, 16, 2); // initialize to any format
            _mFormat.wFormatTag = reader.ReadInt16();
            _mFormat.nChannels = reader.ReadInt16();
            _mFormat.nSamplesPerSec = reader.ReadInt32();
            _mFormat.nAvgBytesPerSec = reader.ReadInt32();
            _mFormat.nBlockAlign = reader.ReadInt16();
            _mFormat.wBitsPerSample = reader.ReadInt16();

            // advance in the stream to skip the wave format block 
            len -= 16; // minimum format size
            while (len > 0)
            {
                reader.ReadByte();
                len--;
            }

            // assume the data chunk is aligned
            while (m_Stream.Position < m_Stream.Length && ReadChunk(reader) != "data")
            {
            }

            if (m_Stream.Position >= m_Stream.Length) throw new Exception("Invalid file format");

            _mLength = reader.ReadInt32();
            _mDataPos = m_Stream.Position;

            Position = 0;
        }

        public WaveStream(string fileName) : this(new FileStream(fileName, FileMode.Open))
        {
        }

        public WaveStream(Stream s)
        {
            m_Stream = s;
            ReadHeader();
        }

        ~WaveStream()
        {
            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_Stream?.Close();
            GC.SuppressFinalize(this);
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _mLength;

        public override long Position
        {
            get => m_Stream.Position - _mDataPos;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override void Close() => Dispose();

        public override void Flush()
        {
        }

        public override void SetLength(long len) => throw new InvalidOperationException();

        public override long Seek(long pos, SeekOrigin o)
        {
            switch (o)
            {
                case SeekOrigin.Begin:
                    m_Stream.Position = pos + _mDataPos;
                    break;
                case SeekOrigin.Current:
                    m_Stream.Seek(pos, SeekOrigin.Current);
                    break;
                case SeekOrigin.End:
                    m_Stream.Position = _mDataPos + _mLength - pos;
                    break;
            }
            return Position;
        }

        public override int Read(byte[] buf, int ofs, int count)
        {
            var toread = (int) Math.Min(count, _mLength - Position);
            return m_Stream.Read(buf, ofs, toread);
        }

        public override void Write(byte[] buf, int ofs, int count) => throw new InvalidOperationException();
    }
}