using System;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace CSharpUtils
{
	/// <summary>
	/// 
	/// </summary>
	public unsafe sealed class MathFloat
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Abs(float Value)
		{
			if (Value == -0f) return 0f;
			return (Value >= 0) ? Value : -Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int Cast(float Value)
		{
			if (float.IsNegativeInfinity(Value)) return int.MinValue;
			if (float.IsInfinity(Value) || float.IsNaN(Value)) return int.MaxValue;
			return (int)Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int Floor(float Value)
		{
			if (float.IsNegativeInfinity(Value)) return int.MinValue;
			if (float.IsInfinity(Value) || float.IsNaN(Value)) return int.MaxValue;
			return (int)Math.Floor((double)Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int Ceil(float Value)
		{
			if (float.IsNegativeInfinity(Value)) return int.MinValue;
			if (float.IsInfinity(Value) || float.IsNaN(Value)) return int.MaxValue;
			return (int)Math.Ceiling((double)Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int Round(float Value)
		{
			if (float.IsNegativeInfinity(Value)) return int.MinValue;
			if (float.IsInfinity(Value) || float.IsNaN(Value)) return int.MaxValue;
			return (int)Math.Round((double)Value);
		}

		/// <summary>
		/// Rounds x to the nearest integer value, using the current rounding mode.
		/// If the return value is not equal to x, the FE_INEXACT exception is raised.
		/// nearbyint performs the same operation, but does not set the FE_INEXACT exception.
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Rint(float Value)
		{
			if (float.IsNegativeInfinity(Value)) return int.MinValue;
			if (float.IsInfinity(Value) || float.IsNaN(Value)) return int.MaxValue;
			return MathFloat.Round(Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static uint ReinterpretFloatAsUInt(float Value)
		{
			return *((uint *)&Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float ReinterpretUIntAsFloat(uint Value)
		{
			return *((float*)&Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int ReinterpretFloatAsInt(float Value)
		{
			return *((int*)&Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float ReinterpretIntAsFloat(int Value)
		{
			return *((float*)&Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Angle"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Cos(float Angle)
		{
			return (float)Math.Cos(Angle);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Angle"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Sin(float Angle)
		{
			return (float)Math.Sin(Angle);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AngleV1"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float CosV1(float AngleV1)
		{
			return Cos(AngleV1 * PI_2);
		}

		private const float PI_2 = (float)(Math.PI / 2f);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AngleV1"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float SinV1(float AngleV1)
		{
			return Sin(AngleV1 * PI_2);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="Min"></param>
		/// <param name="Max"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Clamp(float Value, float Min, float Max)
		{
			if (Min == 0 && Value == -0) return 0;
			if (Value < Min) Value = Min;
			else if (Value > Max) Value = Max;
			return Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="Min"></param>
		/// <param name="Max"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int ClampInt(int Value, int Min, int Max)
		{
			if (Value < Min) Value = Min;
			else if (Value > Max) Value = Max;
			return Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Sqrt(float Value)
		{
			return (float)Math.Sqrt((double)Value);
		}

		/// <summary>
		/// Math.scalb (12.0, 3) = 96.0
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Scalb(float Value, int Count)
		{
			return (float)(Value * Math.Pow(2.0f, Count));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Sign(float Value)
		{
			if (Value == 0) return 0f;
			var IValue = MathFloat.ReinterpretFloatAsUInt(Value);
			return ((IValue & 0x80000000) != 0) ? -1f : +1f;
			//if (float.IsNaN(Value)) return +1f;
			//if (float.IsNaN(-Value)) return -1f;
			//if (Value > 0) return +1.0f;
			//if (Value < 0) return -1.0f;
			//return 0.0f;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Min(float Left, float Right)
		{
			//var ILeft = MathFloat.ReinterpretFloatAsUInt(Left);
			//var IRight = MathFloat.ReinterpretFloatAsUInt(Right);
			return Math.Min(Left, Right);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Max(float Left, float Right)
		{
			return Math.Max(Left, Right);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool IsNan(float Value)
		{
			//return float.IsNaN(Value);
			return float.IsNaN(Math.Abs(Value));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool IsInfinity(float Value)
		{
			return float.IsInfinity(Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float RSqrt(float Value)
		{
			return 1.0f / Sqrt(Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Asin(float Value)
		{
			return (float)Math.Asin((double)Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float AsinV1(float Value)
		{
			return Asin(Value) / PI_2;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Vsat0(float Value)
		{
			return MathFloat.Clamp(Value, 0.0f, 1.0f);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Vsat1(float Value)
		{
			if (float.IsNaN(Value)) return Value;
			return MathFloat.Clamp(Value, -1.0f, 1.0f);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Log2(float Value)
		{
			return (float)(Math.Log(Value) / Math.Log(2.0f));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Exp2(float Value)
		{
			return (float)Math.Pow(2.0, Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float NRcp(float Value)
		{
			return -(1.0f / Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Angle"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float NSinV1(float Angle)
		{
			var Value = SinV1(Angle);
			if (Value == 0f) return -0f;
			return -Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float RExp2(float Value)
		{
			return (float)(1.0 / Math.Pow(2.0, Value));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool IsNanOrInfinity(float Value)
		{
			return IsNan(Value) || float.IsInfinity(Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool IsZero(float Value)
		{
			if (IsNan(Value)) return false;
			return (Value == 0f || Value == -0f);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool IsEquals(float Left, float Right)
		{
			if (IsNan(Left) || IsNan(Right)) return false;
			return Left == Right;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool IsLessThan(float Left, float Right)
		{
			if (IsNan(Left) || IsNan(Right)) return false;
			return Left < Right;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool IsLessOrEqualsThan(float Left, float Right)
		{
			if (IsNan(Left) || IsNan(Right)) return false;
			return Left <= Right;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool IsGreatOrEqualsThan(float Left, float Right)
		{
			if (IsNan(Left) || IsNan(Right)) return false;
			return Left >= Right;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <returns></returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static float Sign2(float Left, float Right)
		{
			float a = Left - Right;
			return (float)(((0.0 < a) ? 1 : 0) - ((a < 0.0) ? 1 : 0));
		}
	}
}
