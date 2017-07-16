using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class MethodInfoExtensions
{
	public static T GetSingleAttribute<T>(this ICustomAttributeProvider MethodInfo) where T : Attribute
	{
		return MethodInfo.GetCustomAttributes(typeof(T), true).ElementAtOrDefault(0) as T;
	}

	public static IEnumerable<T> GetAttribute<T>(this ICustomAttributeProvider MethodInfo) where T : Attribute
	{
		return MethodInfo.GetCustomAttributes(typeof(T), true).Cast<T>();
	}
}
