using System;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace CSharpUtils
{
	/// <summary>
	/// 
	/// </summary>
	/// <see cref="http://blogs.microsoft.co.il/blogs/sasha/archive/2012/01/20/aggressive-inlining-in-the-clr-4-5-jit.aspx"/>
	public static class BitUtils
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Size"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static uint CreateMask(int Size)
		{
			return (Size == 0) ? 0 : (uint)((1 << Size) - 1);
		}

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="Value"></param>
	    /// <param name="Offset"> </param>
	    /// <param name="Count"> </param>
	    /// <param name="ValueToInsert"> </param>
	    /// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static void Insert(ref uint Value, int Offset, int Count, uint ValueToInsert)
		{
			Value = Insert(Value, Offset, Count, ValueToInsert);
		}

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="InitialValue"> </param>
	    /// <param name="Offset"> </param>
	    /// <param name="Count"> </param>
	    /// <param name="ValueToInsert"> </param>
	    /// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static uint Insert(uint InitialValue, int Offset, int Count, uint ValueToInsert)
		{
			return InsertWithMask(InitialValue, Offset, CreateMask(Count), ValueToInsert);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InitialValue"> </param>
		/// <param name="Offset"> </param>
		/// <param name="Count"> </param>
		/// <param name="ValueToInsert"> </param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static void InsertScaled(ref uint InitialValue, int Offset, int Count, uint ValueToInsert, uint MaxValue)
		{
			InitialValue = InsertScaled(InitialValue, Offset, Count, ValueToInsert, MaxValue);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InitialValue"> </param>
		/// <param name="Offset"> </param>
		/// <param name="Count"> </param>
		/// <param name="ValueToInsert"> </param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static uint InsertScaled(uint InitialValue, int Offset, int Count, uint ValueToInsert, uint MaxValue)
		{
			return InsertWithMask(InitialValue, Offset, CreateMask(Count), ValueToInsert * CreateMask(Count) / MaxValue);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InitialValue"></param>
		/// <param name="Offset"></param>
		/// <param name="Mask"></param>
		/// <param name="ValueToInsert"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		private static uint InsertWithMask(uint InitialValue, int Offset, uint Mask, uint ValueToInsert)
		{
			return ((InitialValue & ~(Mask << Offset)) | ((ValueToInsert & Mask) << Offset));
		}

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="InitialValue"> </param>
	    /// <param name="Offset"> </param>
	    /// <param name="Count"> </param>
	    /// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static uint Extract(uint InitialValue, int Offset, int Count)
		{
			return (uint)((InitialValue >> Offset) & CreateMask(Count));
		}

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="InitialValue"> </param>
	    /// <param name="Offset"> </param>
	    /// <param name="Count"> </param>
	    /// <param name="Scale"> </param>
	    /// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static uint ExtractScaled(uint InitialValue, int Offset, int Count, int Scale)
		{
			return (uint)((Extract(InitialValue, Offset, Count) * Scale) / CreateMask(Count));
		}

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="InitialValue"> </param>
	    /// <param name="Offset"> </param>
	    /// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool ExtractBool(uint InitialValue, int Offset)
		{
			return Extract(InitialValue, Offset, 1) != 0;
		}

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="InitialValue"> </param>
	    /// <param name="Offset"> </param>
	    /// <param name="Count"> </param>
	    /// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int ExtractSigned(uint InitialValue, int Offset, int Count)
		{
			var Mask = CreateMask(Count);
			uint SignBit = (uint)(1 << (Offset + (Count - 1)));
			uint _Value = (uint)((InitialValue >> Offset) & Mask);
			if ((_Value & SignBit) != 0)
			{
				_Value |= ~Mask;
			}
			return (int)_Value;
		}

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="Value"> </param>
	    /// <param name="Offset"> </param>
	    /// <param name="Count"> </param>
	    /// <param name="Scale"> </param>
	    /// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ExtractUnsignedScaled(uint Value, int Offset, int Count, float Scale = 1.0f)
		{
			return ((float)Extract(Value, Offset, Count) / (float)CreateMask(Count)) * Scale;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Arrays"></param>
		/// <returns></returns>
		public static byte[] XorBytes(params byte[][] Arrays)
		{
			int Length = Arrays[0].Length;
			foreach (var Array in Arrays) if (Array.Length != Length) throw(new InvalidOperationException("Arrays sizes must match"));
			var Bytes = new byte[Length];
			foreach (var Array in Arrays)
			{
				for (int n = 0; n < Length; n++) Bytes[n] ^= Array[n];
			}
			return Bytes;
		}

		static readonly int[] MultiplyDeBruijnBitPosition = 
		{
			0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 
			31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
		};

		/// <summary>
		/// 
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int GetFirstBit1(uint v)
		{
			return MultiplyDeBruijnBitPosition[((uint)((v & -v) * 0x077CB531U)) >> 27];
		}
	}
}
