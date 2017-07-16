using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

public static class StructExtensions
{
    public static string
        ToStringDefault<T>(this T Struct, bool SimplifyBool = false, Type StructType = null) //where T : struct
    {
        if (StructType == null) StructType = typeof(T);

        var Ret = "";

        //Console.WriteLine("{0}", StructType);

        if (StructType.IsArray)
        {
            var ElementType = StructType.GetElementType();
            foreach (var Item in Struct as Array)
            {
                if (Ret.Length > 0) Ret += ", ";
                Ret += Item.ToStringDefault(SimplifyBool, ElementType);
            }
            return "[" + Ret + "]";
        }
        else if (StructType == typeof(string))
        {
            return "\"" + (Struct as String).EscapeString() + "\"";
        }
        else if (StructType.IsEnum)
        {
            // @TODO: Check Flags
            return Enum.GetName(StructType, Struct);
        }
        else if (StructType.IsPrimitive)
        {
            if (StructType == typeof(uint))
            {
                return String.Format("0x{0:X}", Struct);
            }

            return String.Format("{0}", Struct);
        }
        else if (StructType.IsClass || StructType.IsValueType)
        {
            Ret += StructType.Name;
            Ret += "(";
            var MemberCount = 0;
            bool AddedItem = false;

            //FieldInfo fi;
            //PropertyInfo pi;
            foreach (var MemberInfo in StructType.GetMembers())
            {
                bool ValueSet = false;
                object Value = null;

                try
                {
                    if (MemberInfo is FieldInfo)
                    {
                        ValueSet = true;
                        Value = (MemberInfo as FieldInfo).GetValue(Struct);
                    }
                    else if (MemberInfo is PropertyInfo)
                    {
                        ValueSet = true;
                        Value = (MemberInfo as PropertyInfo).GetValue(Struct, null);
                    }
                }
                catch
                {
                    ValueSet = false;
                    Value = null;
                }

                if (ValueSet)
                {
                    if (AddedItem)
                    {
                        Ret += ",";
                        AddedItem = false;
                    }

                    if (SimplifyBool && (Value is bool))
                    {
                        if (((bool) Value) == true)
                        {
                            Ret += MemberInfo.Name;
                            MemberCount++;
                            AddedItem = true;
                        }
                    }
                    else
                    {
                        Ret += MemberInfo.Name;
                        Ret += "=";

                        var ValueType = Value.GetType();

                        if (Value is uint)
                        {
                            Ret += String.Format("0x{0:X}", Value);
                        }
                        else
                        {
                            Ret += Value.ToStringDefault(SimplifyBool, ValueType);
                        }
                        MemberCount++;
                        AddedItem = true;
                    }
                }
            }
            Ret += ")";
            return Ret;
        }
        else
        {
            return StructType.ToString();
        }
    }
}