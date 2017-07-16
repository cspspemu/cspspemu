using System.Runtime.InteropServices;

namespace CSharpUtils.Endian
{
	public struct ulong_be
	{
		private ulong _InternalValue;

		public ulong NativeValue
		{
			set
			{
				_InternalValue = MathUtils.ByteSwap(value);
			}
			get
			{
				return MathUtils.ByteSwap(_InternalValue);
			}
		}

		public static implicit operator ulong(ulong_be that)
		{
			return that.NativeValue;
		}

		public static implicit operator ulong_be(ulong that)
		{
			return new ulong_be()
			{
				NativeValue = that,
			};
		}

		public override string ToString()
		{
			return NativeValue.ToString();
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct uint48_be
	{
		private ushort _InternalValueHigh;
		private uint _InternalValueLow;

		public ulong NativeValue
		{
			set
			{
				var Value = MathUtils.ByteSwap(value);
				_InternalValueLow = (uint)(Value >> 0);
				_InternalValueHigh = (ushort)(Value >> 32);
			}
			get
			{
				var Value = ((ulong)_InternalValueLow << 32) | ((ulong)_InternalValueHigh << 16);
				return MathUtils.ByteSwap(Value);
			}
		}

		public static implicit operator ulong(uint48_be that)
		{
			return that.NativeValue;
		}

		public static implicit operator uint48_be(ulong that)
		{
			return new uint48_be()
			{
				NativeValue = that,
			};
		}

		public override string ToString()
		{
			return NativeValue.ToString();
		}
	}

	public struct uint_be
	{
		private uint _InternalValue;

		public uint NativeValue
		{
			set
			{
				_InternalValue = MathUtils.ByteSwap(value);
			}
			get
			{
				return MathUtils.ByteSwap(_InternalValue);
			}
		}

		public static implicit operator uint(uint_be that)
		{
			return that.NativeValue;
		}

		public static implicit operator uint_be(uint that)
		{
			return new uint_be()
			{
				NativeValue = that,
			};
		}

		public static implicit operator uint_be(uint_le that)
		{
			return (uint)that;
		}

		public override string ToString()
		{
			return NativeValue.ToString();
		}
	}

	public struct ushort_be
	{
		private ushort _InternalValue;

		public ushort NativeValue
		{
			set
			{
				_InternalValue = MathUtils.ByteSwap(value);
			}
			get
			{
				return MathUtils.ByteSwap(_InternalValue);
			}
		}

		public static implicit operator ushort(ushort_be that)
		{
			return that.NativeValue;
		}

		public static implicit operator ushort_be(ushort that)
		{
			return new ushort_be()
			{
				NativeValue = that,
			};
		}

		public static implicit operator ushort_be(ushort_le that)
		{
			return (ushort)that;
		}

		public override string ToString()
		{
			return NativeValue.ToString();
		}

		public byte Low { get { return (byte)(NativeValue >> 0); } }
		public byte High { get { return (byte)(NativeValue >> 8); } }
	}

	public struct uint_le
	{
		private uint _InternalValue;

		public uint NativeValue
		{
			set
			{
				//_InternalValue = MathUtils.ByteSwap(value);
				_InternalValue = value;
			}
			get
			{
				//return MathUtils.ByteSwap(_InternalValue);
				return _InternalValue;
			}
		}

		public static implicit operator uint(uint_le that)
		{
			return that.NativeValue;
		}

		public static implicit operator uint_le(uint that)
		{
			return new uint_le()
			{
				NativeValue = that,
			};
		}

		public static implicit operator uint_le(uint_be that)
		{
			return (uint)that;
		}

		public override string ToString()
		{
			return NativeValue.ToString();
		}
	}

	public struct ushort_le
	{
		private ushort _InternalValue;

		public ushort NativeValue
		{
			set
			{
				//_InternalValue = MathUtils.ByteSwap(value);
				_InternalValue = value;
			}
			get
			{
				//return MathUtils.ByteSwap(_InternalValue);
				return _InternalValue;
			}
		}

		public static implicit operator uint(ushort_le that)
		{
			return that.NativeValue;
		}

		public static implicit operator ushort_le(ushort that)
		{
			return new ushort_le()
			{
				NativeValue = that,
			};
		}

		public static implicit operator ushort_le(ushort_be that)
		{
			return (ushort)that;
		}

		public override string ToString()
		{
			return NativeValue.ToString();
		}
	}

	public struct float_be
	{
		private float _InternalValue;

		public float NativeValue
		{
			set
			{
				_InternalValue = MathUtils.ByteSwap(value);
			}
			get
			{
				return MathUtils.ByteSwap(_InternalValue);
			}
		}

		public static implicit operator float(float_be that)
		{
			return that.NativeValue;
		}

		public static implicit operator float_be(float that)
		{
			return new float_be()
			{
				NativeValue = that,
			};
		}

		public override string ToString()
		{
			return NativeValue.ToString();
		}
	}
}
