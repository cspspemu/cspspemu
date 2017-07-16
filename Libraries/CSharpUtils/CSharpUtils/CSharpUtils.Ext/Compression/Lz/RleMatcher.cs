using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpUtils.Ext.Compression.Lz
{
    sealed public class RleMatcher
    {
        public byte Byte;
        public int Length = 0;
        private byte[] Data;
        private int _Offset;

        public int Offset
        {
            get { return _Offset; }
            set
            {
                Length = 0;
                _Offset = value;
                Skip(0);
            }
        }

        public RleMatcher(byte[] Data, int Offset = 0)
        {
            this.Data = Data;
            this.Offset = Offset;
        }

        unsafe public void Skip(int Skip = 1)
        {
            _Offset += Skip;

            if (_Offset >= Data.Length)
            {
                Length = 0;
                return;
            }

            if (Skip >= Length)
            {
                Byte = Data[_Offset];
                Length = 0;

                fixed (byte* Start = &Data[_Offset])
                fixed (byte* End = &Data[Data.Length - 1])
                {
                    int MaxLen = (int) (End - Start);
                    Length = PointerUtils.FindLargestMatchByte(Start, Start[0], MaxLen);
                }
            }
            else
            {
                Length -= Skip;
            }
        }
    }
}