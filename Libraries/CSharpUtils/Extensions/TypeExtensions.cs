using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

static public class TypeExtensions
{
	static public T GetCustomAttribute<T>(this Type Type, bool inherit)// if (T is Attribute)
	{
		return Type.GetCustomAttributes<T>(inherit).FirstOrDefault();
	}

	static public IEnumerable<T> GetCustomAttributes<T>(this Type Type, bool inherit)// if (T is Attribute)
	{
		return Type.GetCustomAttributes(typeof(T), inherit).Cast<T>();
	}
}
