namespace CSharpUtils.Ext.Compression.Lz
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RleMatcher
    {
        /// <summary>
        /// 
        /// </summary>
        public byte Byte;

        /// <summary>
        /// 
        /// </summary>
        public int Length;

        private byte[] Data;
        private int _offset;

        /// <summary>
        /// 
        /// </summary>
        public int Offset
        {
            get => _offset;
            set
            {
                Length = 0;
                _offset = value;
                Skip(0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public RleMatcher(byte[] data, int offset = 0)
        {
            Data = data;
            Offset = offset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skip"></param>
        public unsafe void Skip(int skip = 1)
        {
            _offset += skip;

            if (_offset >= Data.Length)
            {
                Length = 0;
                return;
            }

            if (skip >= Length)
            {
                Byte = Data[_offset];
                Length = 0;

                fixed (byte* start = &Data[_offset])
                fixed (byte* end = &Data[Data.Length - 1])
                {
                    var maxLen = (int) (end - start + 1);
                    Length = PointerUtils.FindLargestMatchByte(start, start[0], maxLen);
                }
            }
            else
            {
                Length -= skip;
            }
        }
    }
}