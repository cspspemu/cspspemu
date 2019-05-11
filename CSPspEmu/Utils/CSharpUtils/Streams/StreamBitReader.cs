using System.IO;

namespace CSharpUtils.Streams
{
    /// <summary>
    /// 
    /// </summary>
    public class StreamBitReader
    {
        protected Stream Stream;

        /// <summary>
        /// 
        /// </summary>
        public int ByteAvailableBits;

        /// <summary>
        /// 
        /// </summary>
        public uint ByteData;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public StreamBitReader(Stream stream)
        {
            Stream = stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public uint ReadBits(int count)
        {
            if (ByteAvailableBits == 0)
            {
                PrepareData();
            }

            if (count > ByteAvailableBits)
            {
                var leftBits = count - ByteAvailableBits;
                var currentData = ReadBits(ByteAvailableBits);
                var leftData = ReadBits(leftBits);
                return (currentData << leftBits) | leftData;
            }
            try
            {
                return (uint) (ByteData & ((1 << count) - 1));
            }
            finally
            {
                ByteAvailableBits -= count;
                ByteData >>= count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void PrepareData()
        {
            ByteData = (uint) Stream.ReadByte();
            ByteAvailableBits = 8;
        }
    }
}