using System.IO;

namespace CSharpUtils.Streams
{
    public class StreamBitReader
    {
        protected Stream Stream;

        public int ByteAvailableBits;
        public uint ByteData;

        public StreamBitReader(Stream Stream)
        {
            this.Stream = Stream;
        }

        public uint ReadBits(int Count)
        {
            if (ByteAvailableBits == 0)
            {
                PrepareData();
            }

            if (Count > ByteAvailableBits)
            {
                var LeftBits = Count - ByteAvailableBits;
                var CurrentData = ReadBits(ByteAvailableBits);
                var LeftData = ReadBits(LeftBits);
                return (CurrentData << LeftBits) | LeftData;
            }
            else
            {
                try
                {
                    return (uint) (ByteData & ((1 << Count) - 1));
                }
                finally
                {
                    ByteAvailableBits -= Count;
                    ByteData >>= Count;
                }
            }
        }

        protected void PrepareData()
        {
            ByteData = (uint) Stream.ReadByte();
            ByteAvailableBits = 8;
        }
    }
}