using System;
using System.Collections.Generic;
using System.Reflection;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class StructExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Struct"></param>
        /// <param name="simplifyBool"></param>
        /// <param name="structType"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string
            ToStringDefault<T>(this T Struct, bool simplifyBool = false, Type structType = null, HashSet<object> memory = null) //where T : struct
        {
            if (structType == null) structType = typeof(T);

            if (memory == null) memory = new HashSet<object>();
            if (!structType.IsPrimitive)
            {
                if (memory.Contains(Struct)) return "<recursive>";
                memory.Add(Struct);
            }

            var ret = "";

            //Console.WriteLine("{0}", StructType);

            if (structType.IsArray && Struct is Array)
            {
                var elementType = structType.GetElementType();
                foreach (var item in Struct as Array)
                {
                    if (ret.Length > 0) ret += ", ";
                    ret += item.ToStringDefault(simplifyBool, elementType, memory);
                }
                return "[" + ret + "]";
            }
            if (structType == typeof(string))
            {
                return "\"" + (Struct as string).EscapeString() + "\"";
            }
            if (structType.IsEnum)
            {
                // @TODO: Check Flags
                return Enum.GetName(structType, Struct);
            }
            if (structType.IsPrimitive)
            {
                if (structType == typeof(uint))
                {
                    return $"0x{Struct:X}";
                }

                return $"{Struct}";
            }
            if (structType.IsClass || structType.IsValueType)
            {
                ret += structType.Name;
                ret += "(";
                var addedItem = false;

                //FieldInfo fi;
                //PropertyInfo pi;
                foreach (var memberInfo in structType.GetMembers())
                {
                    var valueSet = false;
                    object value = null;

                    try
                    {
                        if (memberInfo is FieldInfo)
                        {
                            valueSet = true;
                            value = (memberInfo as FieldInfo).GetValue(Struct);
                        }
                        else if (memberInfo is PropertyInfo)
                        {
                            valueSet = true;
                            value = (memberInfo as PropertyInfo).GetValue(Struct, null);
                        }
                    }
                    catch
                    {
                        valueSet = false;
                        value = null;
                    }

                    if (valueSet)
                    {
                        if (addedItem)
                        {
                            ret += ",";
                            addedItem = false;
                        }

                        if (simplifyBool && value is bool)
                        {
                            if ((bool) value)
                            {
                                ret += memberInfo.Name;
                                //MemberCount++;
                                addedItem = true;
                            }
                        }
                        else
                        {
                            ret += memberInfo.Name;
                            ret += "=";

                            var valueType = value.GetType();

                            if (value is uint)
                            {
                                ret += $"0x{value:X}";
                            }
                            else
                            {
                                ret += value.ToStringDefault(simplifyBool, valueType, memory);
                            }
                            //MemberCount++;
                            addedItem = true;
                        }
                    }
                }
                ret += ")";
                return ret;
            }
            return structType.ToString();
        }
    }
}