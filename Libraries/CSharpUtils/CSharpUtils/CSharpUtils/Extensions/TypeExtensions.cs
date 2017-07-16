using System;
using System.Collections.Generic;
using System.Linq;

public static class TypeExtensions
{
	public static T GetCustomAttribute<T>(this Type Type, bool inherit)// if (T is Attribute)
	{
		return Type.GetCustomAttributes<T>(inherit).FirstOrDefault();
	}

	public static IEnumerable<T> GetCustomAttributes<T>(this Type Type, bool inherit)// if (T is Attribute)
	{
		return Type.GetCustomAttributes(typeof(T), inherit).Cast<T>();
	}

	public static bool Implements(this Type Type, Type Interface)
	{
		return Type.GetInterfaces().Contains(Interface);
	}
}
