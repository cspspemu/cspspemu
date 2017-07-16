using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast
{
	public unsafe class AstUtils
	{
		static public bool IsIntegerType(Type Type)
		{
			if (Type == typeof(sbyte) || Type == typeof(byte)) return true;
			if (Type == typeof(short) || Type == typeof(ushort)) return true;
			if (Type == typeof(int) || Type == typeof(uint)) return true;
			if (Type == typeof(long) || Type == typeof(ulong)) return true;
			return false;
		}

		static public int GetTypeSize(Type Type)
		{
			Type = GetSignedType(Type);
			if (Type == typeof(sbyte)) return sizeof(sbyte);
			if (Type == typeof(short)) return sizeof(short);
			if (Type == typeof(int)) return sizeof(int);
			if (Type == typeof(long)) return sizeof(long);
			if (Type == typeof(float)) return sizeof(float);
			if (Type == typeof(double)) return sizeof(double);
			if (Type == typeof(IntPtr) || Type.IsPointer || Type.IsByRef) return sizeof(void*);
			if (Type.IsAnsiClass) return sizeof(IntPtr);
			Console.Error.WriteLine("Warning. Trying to get size for {0}", Type);
			return Marshal.SizeOf(typeof(IntPtr));
		}

		static public Type GetSignedType(Type Type)
		{
			if (Type == typeof(byte)) return typeof(sbyte);
			if (Type == typeof(ushort)) return typeof(short);
			if (Type == typeof(uint)) return typeof(int);
			if (Type == typeof(ulong)) return typeof(long);
			return Type;
		}

		static public Type GetUnsignedType(Type Type)
		{
			if (Type == typeof(sbyte)) return typeof(byte);
			if (Type == typeof(short)) return typeof(ushort);
			if (Type == typeof(int)) return typeof(uint);
			if (Type == typeof(long)) return typeof(ulong);
			return Type;
		}

		static public bool IsTypeFloat(Type Type)
		{
			return (Type == typeof(float)) || (Type == typeof(double));
		}

		static public bool IsTypeSigned(Type Type)
		{
			if (!Type.IsPrimitive) return false;
			return (
				Type == typeof(sbyte) ||
				Type == typeof(short) ||
				Type == typeof(int) ||
				Type == typeof(long) ||
				Type == typeof(float) ||
				Type == typeof(double) ||
				Type == typeof(decimal)
			);
		}

		static public TType CastType<TType>(object Value)
		{
			return (TType)CastType(Value, typeof(TType));
		}

		static public object CastType(object Value, Type CastType)
		{
			if (CastType == typeof(sbyte)) return (sbyte)Convert.ToInt64(Value);
			if (CastType == typeof(short)) return (short)Convert.ToInt64(Value);
			if (CastType == typeof(int)) return (int)Convert.ToInt64(Value);
			if (CastType == typeof(long)) return (long)Convert.ToInt64(Value);

			if (CastType == typeof(byte)) return (byte)Convert.ToInt64(Value);
			if (CastType == typeof(ushort)) return (ushort)Convert.ToInt64(Value);
			if (CastType == typeof(uint)) return (uint)Convert.ToInt64(Value);
			if (CastType == typeof(ulong)) return (ulong)Convert.ToInt64(Value);

			if (CastType == typeof(float)) return Convert.ToSingle(Value);
			if (CastType == typeof(double)) return Convert.ToDouble(Value);

			if (CastType.IsEnum) return Enum.ToObject(CastType, Value);
			
			throw(new NotImplementedException("CastType for type '" + CastType + "'"));
		}

		static public object Negate(object Value)
		{
			var ValueType = Value.GetType();
			if (ValueType == typeof(sbyte)) return -(sbyte)Value;
			if (ValueType == typeof(short)) return -(short)Value;
			if (ValueType == typeof(int)) return -(int)Value;
			if (ValueType == typeof(long)) return -(long)Value;
			if (ValueType == typeof(float)) return -(float)Value;
			if (ValueType == typeof(double)) return -(double)Value;

			throw (new NotImplementedException("Negate for type '" + ValueType + "'"));
		}
	}
}
