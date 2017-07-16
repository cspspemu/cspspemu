using System.Runtime.InteropServices;

namespace CSharpUtils.Endian
{
    /// <summary>
    /// 
    /// </summary>
    public struct UlongBe
    {
        private ulong _internalValue;

        /// <summary>
        /// 
        /// </summary>
        public ulong NativeValue
        {
            set => _internalValue = MathUtils.ByteSwap(value);
            get => MathUtils.ByteSwap(_internalValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator ulong(UlongBe that) => that.NativeValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator UlongBe(ulong that) => new UlongBe()
        {
            NativeValue = that,
        };

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => NativeValue.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct Uint48Be
    {
        private ushort _InternalValueHigh;
        private uint _InternalValueLow;

        /// <summary>
        /// 
        /// </summary>
        public ulong NativeValue
        {
            set
            {
                var v = MathUtils.ByteSwap(value);
                _InternalValueLow = (uint) (v >> 0);
                _InternalValueHigh = (ushort) (v >> 32);
            }
            get
            {
                var value = ((ulong) _InternalValueLow << 32) | ((ulong) _InternalValueHigh << 16);
                return MathUtils.ByteSwap(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator ulong(Uint48Be that)
        {
            return that.NativeValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator Uint48Be(ulong that)
        {
            return new Uint48Be()
            {
                NativeValue = that,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return NativeValue.ToString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct UintBe
    {
        private uint _internalValue;

        /// <summary>
        /// 
        /// </summary>
        public uint NativeValue
        {
            set => _internalValue = MathUtils.ByteSwap(value);
            get => MathUtils.ByteSwap(_internalValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator uint(UintBe that)
        {
            return that.NativeValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator UintBe(uint that) => new UintBe
        {
            NativeValue = that,
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator UintBe(UintLe that) => (uint) that;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => NativeValue.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    public struct UshortBe
    {
        private ushort _internalValue;

        /// <summary>
        /// 
        /// </summary>
        public ushort NativeValue
        {
            set => _internalValue = MathUtils.ByteSwap(value);
            get => MathUtils.ByteSwap(_internalValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator ushort(UshortBe that) => that.NativeValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator UshortBe(ushort that) => new UshortBe
        {
            NativeValue = that,
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator UshortBe(UshortLe that) => (ushort) that;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => NativeValue.ToString();

        /// <summary>
        /// 
        /// </summary>
        public byte Low => (byte) (NativeValue >> 0);

        /// <summary>
        /// 
        /// </summary>
        public byte High => (byte) (NativeValue >> 8);
    }

    /// <summary>
    /// 
    /// </summary>
    public struct UintLe
    {
        /// <summary>
        /// 
        /// </summary>
        public uint NativeValue { set; get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator uint(UintLe that) => that.NativeValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator UintLe(uint that) => new UintLe
        {
            NativeValue = that,
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator UintLe(UintBe that) => (uint) that;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => NativeValue.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    public struct UshortLe
    {
        /// <summary>
        /// 
        /// </summary>
        public ushort NativeValue { set; get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator uint(UshortLe that)
        {
            return that.NativeValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator UshortLe(ushort that)
        {
            return new UshortLe()
            {
                NativeValue = that,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator UshortLe(UshortBe that)
        {
            return (ushort) that;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return NativeValue.ToString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct FloatBe
    {
        private float _internalValue;

        /// <summary>
        /// 
        /// </summary>
        public float NativeValue
        {
            set => _internalValue = MathUtils.ByteSwap(value);
            get => MathUtils.ByteSwap(_internalValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator float(FloatBe that) => that.NativeValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator FloatBe(float that) => new FloatBe
        {
            NativeValue = that,
        };

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once SpecifyACultureInStringConversionExplicitly
        public override string ToString() => NativeValue.ToString();
    }
}