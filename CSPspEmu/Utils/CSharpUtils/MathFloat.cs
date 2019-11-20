using System;
using System.Runtime;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public sealed unsafe class MathFloat
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float Abs(float value)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (value == -0f) return 0f;
            return value >= 0 ? value : -value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static int Cast(float value)
        {
            if (float.IsNegativeInfinity(value)) return int.MinValue;
            if (float.IsInfinity(value) || float.IsNaN(value)) return int.MaxValue;
            return (int) value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static int Floor(float value)
        {
            if (float.IsNegativeInfinity(value)) return int.MinValue;
            if (float.IsInfinity(value) || float.IsNaN(value)) return int.MaxValue;
            return (int) Math.Floor(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static int Ceil(float value)
        {
            if (float.IsNegativeInfinity(value)) return int.MinValue;
            if (float.IsInfinity(value) || float.IsNaN(value)) return int.MaxValue;
            return (int) Math.Ceiling(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static int Round(float value)
        {
            if (float.IsNegativeInfinity(value)) return int.MinValue;
            if (float.IsInfinity(value) || float.IsNaN(value)) return int.MaxValue;
            return (int) Math.Round(value);
        }

        /// <summary>
        /// Rounds x to the nearest integer value, using the current rounding mode.
        /// If the return value is not equal to x, the FE_INEXACT exception is raised.
        /// nearbyint performs the same operation, but does not set the FE_INEXACT exception.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float Rint(float value)
        {
            if (float.IsNegativeInfinity(value)) return int.MinValue;
            if (float.IsInfinity(value) || float.IsNaN(value)) return int.MaxValue;
            return Round(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static uint ReinterpretFloatAsUInt(float value)
        {
            return *(uint*) &value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float ReinterpretUIntAsFloat(uint value)
        {
            return *(float*) &value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static int ReinterpretFloatAsInt(float value)
        {
            return *(int*) &value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float ReinterpretIntAsFloat(int value)
        {
            return *(float*) &value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        
        
        public static float Cos(float angle)
        {
            return (float) Math.Cos(angle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        
        
        public static float Sin(float angle)
        {
            return (float) Math.Sin(angle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angleV1"></param>
        /// <returns></returns>
        
        
        public static float CosV1(float angleV1)
        {
            return Cos(angleV1 * Pi2);
        }

        private const float Pi2 = (float) (Math.PI / 2f);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angleV1"></param>
        /// <returns></returns>
        
        
        public static float SinV1(float angleV1)
        {
            return Sin(angleV1 * Pi2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        
        
        public static float Clamp(float value, float min, float max)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (min == 0) return 0;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (value == -0) return 0;
            if (value < min) value = min;
            else if (value > max) value = max;
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        
        
        public static int ClampInt(int value, int min, int max)
        {
            if (value < min) value = min;
            else if (value > max) value = max;
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float Sqrt(float value)
        {
            return (float) Math.Sqrt(value);
        }

        /// <summary>
        /// Math.scalb (12.0, 3) = 96.0
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        
        
        public static float Scalb(float value, int count)
        {
            return (float) (value * Math.Pow(2.0f, count));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float Sign(float value)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (value == 0) return 0f;
            var iValue = ReinterpretFloatAsUInt(value);
            return (iValue & 0x80000000) != 0 ? -1f : +1f;
            //if (float.IsNaN(Value)) return +1f;
            //if (float.IsNaN(-Value)) return -1f;
            //if (Value > 0) return +1.0f;
            //if (Value < 0) return -1.0f;
            //return 0.0f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        
        
        public static float Min(float left, float right)
        {
            //var ILeft = MathFloat.ReinterpretFloatAsUInt(Left);
            //var IRight = MathFloat.ReinterpretFloatAsUInt(Right);
            return Math.Min(left, right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        
        
        public static float Max(float left, float right)
        {
            return Math.Max(left, right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static bool IsNan(float value)
        {
            //return float.IsNaN(Value);
            return float.IsNaN(Math.Abs(value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static bool IsInfinity(float value)
        {
            return float.IsInfinity(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float RSqrt(float value)
        {
            return 1.0f / Sqrt(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float Asin(float value)
        {
            return (float) Math.Asin(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float AsinV1(float value)
        {
            return Asin(value) / Pi2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float Vsat0(float value)
        {
            return Clamp(value, 0.0f, 1.0f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float Vsat1(float value)
        {
            return float.IsNaN(value) ? value : Clamp(value, -1.0f, 1.0f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float Log2(float value)
        {
            return (float) (Math.Log(value) / Math.Log(2.0f));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float Exp2(float value)
        {
            return (float) Math.Pow(2.0, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float NRcp(float value)
        {
            return -(1.0f / value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        
        
        public static float NSinV1(float angle)
        {
            var value = SinV1(angle);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (value == 0f) return -0f;
            return -value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static float RExp2(float value)
        {
            return (float) (1.0 / Math.Pow(2.0, value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static bool IsNanOrInfinity(float value)
        {
            return IsNan(value) || float.IsInfinity(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        
        public static bool IsZero(float value)
        {
            if (IsNan(value)) return false;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            var r1 = value == 0f;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            var r2 = value == -0f;
            return r1 || r2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        
        
        public static bool IsEquals(float left, float right)
        {
            if (IsNan(left) || IsNan(right)) return false;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return left == right;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        
        
        public static bool IsLessThan(float left, float right)
        {
            if (IsNan(left) || IsNan(right)) return false;
            return left < right;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        
        
        public static bool IsLessOrEqualsThan(float left, float right)
        {
            if (IsNan(left) || IsNan(right)) return false;
            return left <= right;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        
        
        public static bool IsGreatOrEqualsThan(float left, float right)
        {
            if (IsNan(left) || IsNan(right)) return false;
            return left >= right;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        
        
        public static float Sign2(float left, float right)
        {
            var a = left - right;
            return (0.0 < a ? 1 : 0) - (a < 0.0 ? 1 : 0);
        }
    }
}