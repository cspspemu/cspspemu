using System;
using System.IO;

namespace CSharpUtils.Streams
{
    public class StreamChunker
    {
        Stream InputStream;
        byte[] TempBuffer;
        ProduceConsumeBuffer<byte> Buffer;

        public bool Eof
        {
            get { return (Buffer.ConsumeRemaining == 0) && (InputStream.Eof()); }
        }

        public void MakeBytesAvailable(int NumberOfBytes)
        {
            int BytesToRead = NumberOfBytes - this.Buffer.ConsumeRemaining;
            if (BytesToRead > TempBuffer.Length)
                throw (new Exception(
                    "Can't Peek More Bytes Than BufferSize. " + BytesToRead + " > " + TempBuffer.Length));
            if (BytesToRead > 0)
            {
                this.Buffer.Produce(TempBuffer, 0, InputStream.Read(TempBuffer, 0, BytesToRead));
            }
        }

        public byte[] PeekBytes(int NumberOfBytes)
        {
            MakeBytesAvailable(NumberOfBytes);
            return Buffer.ConsumePeek(NumberOfBytes);
        }

        public void SkipBytes(int NumberOfBytes)
        {
            Buffer.Consume(NumberOfBytes);
        }

        public StreamChunker(Stream InputStream, int BufferSize = 4096)
        {
            this.InputStream = InputStream;
            this.TempBuffer = new byte[BufferSize];
            this.Buffer = new ProduceConsumeBuffer<byte>();
        }

        public long SkipUpToSequence(byte[] Sequence, long MaxReaded = long.MaxValue)
        {
            return CopyUpToSequence(null, Sequence, MaxReaded);
        }

        public byte[] GetUpToSequence(byte[] Sequence, long MaxReaded = long.MaxValue)
        {
            var MemoryStream = new MemoryStream();
            CopyUpToSequence(MemoryStream, Sequence, MaxReaded);
            return MemoryStream.ToArray();
        }

        public long CopyUpToSequence(Stream OutputStream, byte[] Sequence, long MaxReaded = long.MaxValue)
        {
            int TempBufferReaded;
            byte[] ConsumedBytes;
            long TotalCopied = 0;

            Action<int> Consume = delegate(int Length)
            {
                if (Length > 0)
                {
                    ConsumedBytes = this.Buffer.Consume(Length);
                    if (OutputStream != null)
                    {
                        OutputStream.Write(ConsumedBytes, 0, ConsumedBytes.Length);
                    }
                    TotalCopied += ConsumedBytes.Length;
                }
            };

            while (true)
            {
                TempBufferReaded = InputStream.Read(TempBuffer, 0,
                    (int) Math.Min(TempBuffer.Length, MaxReaded - TotalCopied));
                if (TempBufferReaded > 0)
                {
                    this.Buffer.Produce(TempBuffer, 0, TempBufferReaded);
                }

                if (this.Buffer.ConsumeRemaining <= Sequence.Length) break;

                int FoundIndex = this.Buffer.IndexOf(Sequence);

                // Not Found
                if (FoundIndex == -1)
                {
                    Consume(this.Buffer.ConsumeRemaining - Sequence.Length);
                }
                // Found!
                else
                {
                    Consume(FoundIndex);
                    // Remove Sequence.
                    this.Buffer.Consume(Sequence.Length);

                    return TotalCopied;
                }
            }

            Consume(this.Buffer.ConsumeRemaining);

            return TotalCopied;
        }
    }
}