using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils
{
	unsafe public class MathFloat
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static float Abs(float Value)
		{
			return (Value > 0) ? Value : -Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static int Cast(float Value)
		{
			return (int)Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static int Floor(float Value)
		{
			return (int)Math.Floor((double)Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static int Ceil(float Value)
		{
			return (int)Math.Ceiling((double)Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static int Round(float Value)
		{
			return (int)Math.Round((double)Value);
		}

		/// <summary>
		/// Rounds x to the nearest integer value, using the current rounding mode.
		/// If the return value is not equal to x, the FE_INEXACT exception is raised.
		/// nearbyint performs the same operation, but does not set the FE_INEXACT exception.
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static float Rint(float Value)
		{
			return MathFloat.Round(Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static uint ReinterpretFloatAsUInt(float Value)
		{
			//Console.WriteLine("ReinterpretFloatAsUInt:{0}", Value);
			var Values = new float[1];
			Values[0] = Value;
			fixed (float *ValuePtr = &Values[0])
			{
				return *((uint*)ValuePtr);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static float ReinterpretUIntAsFloat(uint Value)
		{
			//Console.WriteLine("ReinterpretUIntAsFloat:{0}", Value);
			var Values = new uint[1];
			Values[0] = Value;
			fixed (uint* ValuePtr = &Values[0])
			{
				return *((float*)ValuePtr);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static int ReinterpretFloatAsInt(float Value)
		{
			//Console.WriteLine("ReinterpretFloatAsUInt:{0}", Value);
			var Values = new float[1];
			Values[0] = Value;
			fixed (float* ValuePtr = &Values[0])
			{
				return *((int*)ValuePtr);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static float ReinterpretIntAsFloat(int Value)
		{
			//Console.WriteLine("ReinterpretUIntAsFloat:{0}", Value);
			var Values = new int[1];
			Values[0] = Value;
			fixed (int* ValuePtr = &Values[0])
			{
				return *((float*)ValuePtr);
			}
		}

		static public float Cos(float Angle)
		{
			return (float)Math.Cos(Angle);
		}

		static public float Sin(float Angle)
		{
			return (float)Math.Sin(Angle);
		}

		static public float CosV1(float AngleV1)
		{
			return Cos((float)(AngleV1 * Math.PI * 0.5f));
		}

		static public float SinV1(float AngleV1)
		{
			//Console.Error.WriteLine("aaaaaaaaaaaaaa");
			return Sin((float)(AngleV1 * Math.PI * 0.5f));
		}

		static public float Clamp(float Value, float Min, float Max)
		{
			if (Value < Min) Value = Min;
			else if (Value > Max) Value = Max;
			return Value;
		}

		static public int ClampInt(int Value, int Min, int Max)
		{
			if (Value < Min) Value = Min;
			else if (Value > Max) Value = Max;
			return Value;
		}
		

		static public float Sqrt(float Value)
		{
			return (float)Math.Sqrt((double)Value);
		}

		/// <summary>
		/// Math.scalb (12.0, 3) = 96.0
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		static public float Scalb(float Value, int Count)
		{
			return (float)(Value * Math.Pow(2.0f, Count));
		}

		static public float Sign(float Value)
		{
			if (Value > 0) return +1.0f;
			if (Value < 0) return -1.0f;
			return 0.0f;
		}

		static public float Min(float Left, float Right)
		{
			return Math.Min(Left, Right);
		}

		static public float Max(float Left, float Right)
		{
			return Math.Max(Left, Right);
		}

		static public bool IsNan(float Value)
		{
			return float.IsNaN(Value);
		}

		static public bool IsInfinity(float Value)
		{
			return float.IsInfinity(Value);
		}

		public static float RSqrt(float Value)
		{
			return 1.0f / Sqrt(Value);
		}

		public static float Asin(float Value)
		{
			return (float)Math.Asin((double)Value);
		}

		public static float AsinV1(float Value)
		{
			return Asin(Value) / (float)(Math.PI * 0.5f);
		}

		public static float Vsat0(float Value)
		{
			return MathFloat.Clamp(Value, 0.0f, 1.0f);
		}

		public static float Vsat1(float Value)
		{
			return MathFloat.Clamp(Value, -1.0f, 1.0f);
		}

		public static float Log2(float Value)
		{
			return (float)(Math.Log(Value) / Math.Log(2.0f));
		}

		public static float Exp2(float Value)
		{
			return (float)Math.Pow(2.0, Value);
		}
	}
}
