namespace CSharpUtils.Drawing
{
    /// <summary>
    /// 
    /// </summary>
    public struct ColorFormat
    {
        /// <summary>
        /// 
        /// </summary>
        public struct Component
        {
            /// <summary>
            /// 
            /// </summary>
            public int Offset;

            /// <summary>
            /// 
            /// </summary>
            public int Size;

            /// <summary>
            /// 
            /// </summary>
            public uint Mask => (uint) ((1 << Size) - 1);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="Base"></param>
            /// <param name="value"></param>
            public void Insert(ref uint Base, uint value)
            {
                var maskValue = Mask;

                Base =
                    (Base & ~(Mask << Offset))
                    | ((value & maskValue) << Offset)
                    ;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="Base"></param>
            /// <param name="value"></param>
            public void InsertFromByte(ref uint Base, byte value)
            {
                Insert(ref Base, value * Mask / 255);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public uint Extract(uint value) => (value >> Offset) & Mask;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public byte ExtractToByte(uint value) => (byte) (Extract(value) * 255 / Mask);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public float ExtractToFloat(uint value) => Extract(value) / (float) Mask;
        }

        /// <summary>
        /// 
        /// </summary>
        public int TotalBytes;

        /// <summary>
        /// 
        /// </summary>
        public Component Red;

        /// <summary>
        /// 
        /// </summary>
        public Component Green;

        /// <summary>
        /// 
        /// </summary>
        public Component Blue;

        /// <summary>
        /// 
        /// </summary>
        public Component Alpha;

        /*
        public int TotalUsedBits
        {
            get
            {
                return MathUtils.Max(
                    RedOffset + RedSize,
                    GreenOffset + GreenSize,
                    BlueOffset + BlueSize,
                    AlphaOffset + AlphaSize
                );
            }
        }
        */

        /// <summary>
        /// 
        /// </summary>
        public int[] Offsets
        {
            get => new[] {Red.Offset, Green.Offset, Blue.Offset, Alpha.Offset};
            set
            {
                Red.Offset = value[0];
                Green.Offset = value[1];
                Blue.Offset = value[2];
                Alpha.Offset = value[3];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int[] Sizes
        {
            get => new[] {Red.Size, Green.Size, Blue.Size, Alpha.Size};
            set
            {
                Red.Size = value[0];
                Green.Size = value[1];
                Blue.Size = value[2];
                Alpha.Size = value[3];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public uint Encode(byte r, byte g, byte b, byte a)
        {
            var result = 0U;
            Red.InsertFromByte(ref result, r);
            Green.InsertFromByte(ref result, g);
            Blue.InsertFromByte(ref result, b);
            Alpha.InsertFromByte(ref result, a);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public void Decode(uint data, out byte r, out byte g, out byte b, out byte a)
        {
            r = Red.ExtractToByte(data);
            g = Green.ExtractToByte(data);
            b = Blue.ExtractToByte(data);
            a = Alpha.ExtractToByte(data);
        }
    }
}