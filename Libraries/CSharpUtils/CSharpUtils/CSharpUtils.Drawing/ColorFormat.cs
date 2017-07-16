namespace CSharpUtils
{
    public struct ColorFormat
    {
        public struct Component
        {
            public int Offset;
            public int Size;

            public uint Mask
            {
                get { return (uint) ((1 << Size) - 1); }
            }

            public void Insert(ref uint Base, uint Value)
            {
                var MaskValue = this.Mask;

                Base =
                    (Base & ~(Mask << Offset))
                    | ((Value & MaskValue) << Offset)
                    ;
            }

            public void InsertFromByte(ref uint Base, byte Value)
            {
                Insert(ref Base, (uint) (Value * Mask / 255));
            }

            public uint Extract(uint Value)
            {
                return (Value >> Offset) & Mask;
            }

            public byte ExtractToByte(uint Value)
            {
                return (byte) ((Extract(Value) * 255) / Mask);
            }

            public float ExtractToFloat(uint Value)
            {
                return (float) Extract(Value) / (float) Mask;
            }
        }

        public int TotalBytes;
        public Component Red;
        public Component Green;
        public Component Blue;
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

        public int[] Offsets
        {
            get { return new[] {Red.Offset, Green.Offset, Blue.Offset, Alpha.Offset}; }
            set
            {
                Red.Offset = value[0];
                Green.Offset = value[1];
                Blue.Offset = value[2];
                Alpha.Offset = value[3];
            }
        }

        public int[] Sizes
        {
            get { return new[] {Red.Size, Green.Size, Blue.Size, Alpha.Size}; }
            set
            {
                Red.Size = value[0];
                Green.Size = value[1];
                Blue.Size = value[2];
                Alpha.Size = value[3];
            }
        }

        public uint Encode(byte R, byte G, byte B, byte A)
        {
            uint Result = 0;
            Red.InsertFromByte(ref Result, R);
            Green.InsertFromByte(ref Result, G);
            Blue.InsertFromByte(ref Result, B);
            Alpha.InsertFromByte(ref Result, A);
            return Result;
        }

        public void Decode(uint Data, out byte R, out byte G, out byte B, out byte A)
        {
            R = Red.ExtractToByte(Data);
            G = Green.ExtractToByte(Data);
            B = Blue.ExtractToByte(Data);
            A = Alpha.ExtractToByte(Data);
        }
    }
}