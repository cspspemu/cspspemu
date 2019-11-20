using System;
using System.Runtime.InteropServices;

namespace SafeILGenerator.Ast
{
    public unsafe class AstUtils
    {
        public static bool IsIntegerType(Type type)
        {
            if (type == typeof(sbyte) || type == typeof(byte)) return true;
            if (type == typeof(short) || type == typeof(ushort)) return true;
            if (type == typeof(int) || type == typeof(uint)) return true;
            if (type == typeof(long) || type == typeof(ulong)) return true;
            return false;
        }

        public static int GetTypeSize(Type type)
        {
            type = GetSignedType(type);
            if (type == typeof(sbyte)) return sizeof(sbyte);
            if (type == typeof(short)) return sizeof(short);
            if (type == typeof(int)) return sizeof(int);
            if (type == typeof(long)) return sizeof(long);
            if (type == typeof(float)) return sizeof(float);
            if (type == typeof(double)) return sizeof(double);
            if (type == typeof(IntPtr) || type.IsPointer || type.IsByRef) return sizeof(void*);
            if (type.IsAnsiClass) return sizeof(IntPtr);
            Console.Error.WriteLine("Warning. Trying to get size for {0}", type);
            return Marshal.SizeOf(typeof(IntPtr));
        }

        public static Type GetSignedType(Type type)
        {
            if (type == typeof(byte)) return typeof(sbyte);
            if (type == typeof(ushort)) return typeof(short);
            if (type == typeof(uint)) return typeof(int);
            if (type == typeof(ulong)) return typeof(long);
            return type;
        }

        public static Type GetUnsignedType(Type type)
        {
            if (type == typeof(sbyte)) return typeof(byte);
            if (type == typeof(short)) return typeof(ushort);
            if (type == typeof(int)) return typeof(uint);
            if (type == typeof(long)) return typeof(ulong);
            return type;
        }

        public static bool IsTypeFloat(Type type)
        {
            return type == typeof(float) || type == typeof(double);
        }

        public static bool IsTypeSigned(Type type)
        {
            if (!type.IsPrimitive) return false;
            return type == typeof(sbyte) ||
                   type == typeof(short) ||
                   type == typeof(int) ||
                   type == typeof(long) ||
                   type == typeof(float) ||
                   type == typeof(double) ||
                   type == typeof(decimal);
        }

        public static TType CastType<TType>(object value) => (TType) CastType(value, typeof(TType));

        public static object CastType(object value, Type castType)
        {
            if (castType == typeof(sbyte)) return (sbyte) Convert.ToInt64(value);
            if (castType == typeof(short)) return (short) Convert.ToInt64(value);
            if (castType == typeof(int)) return (int) Convert.ToInt64(value);
            if (castType == typeof(long)) return Convert.ToInt64(value);

            if (castType == typeof(byte)) return (byte) Convert.ToInt64(value);
            if (castType == typeof(ushort)) return (ushort) Convert.ToInt64(value);
            if (castType == typeof(uint)) return (uint) Convert.ToInt64(value);
            if (castType == typeof(ulong)) return (ulong) Convert.ToInt64(value);

            if (castType == typeof(float)) return Convert.ToSingle(value);
            if (castType == typeof(double)) return Convert.ToDouble(value);

            if (castType.IsEnum) return Enum.ToObject(castType, value);

            throw new NotImplementedException("CastType for type '" + castType + "'");
        }

        public static object Negate(object value)
        {
            var valueType = value.GetType();
            if (valueType == typeof(sbyte)) return -(sbyte) value;
            if (valueType == typeof(short)) return -(short) value;
            if (valueType == typeof(int)) return -(int) value;
            if (valueType == typeof(long)) return -(long) value;
            if (valueType == typeof(float)) return -(float) value;
            if (valueType == typeof(double)) return -(double) value;

            throw new NotImplementedException("Negate for type '" + valueType + "'");
        }
    }
}