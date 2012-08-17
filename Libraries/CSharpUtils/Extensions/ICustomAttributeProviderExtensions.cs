using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

static public class MethodInfoExtensions
{
	static public T GetSingleAttribute<T>(this ICustomAttributeProvider MethodInfo) where T : Attribute
	{
		return MethodInfo.GetCustomAttributes(typeof(T), true).ElementAtOrDefault(0) as T;
	}

	static public IEnumerable<T> GetAttribute<T>(this ICustomAttributeProvider MethodInfo) where T : Attribute
	{
		return MethodInfo.GetCustomAttributes(typeof(T), true).Cast<T>();
	}
}
