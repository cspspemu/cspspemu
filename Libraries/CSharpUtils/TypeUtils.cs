using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CSharpUtils
{
	public class TypeUtils
	{
		static public IEnumerable<Type> GetTypesExtending(Type BaseType)
		{
			throw(new NotImplementedException());
			/*
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(Assembly => Assembly.GetTypes())
				.Where(Type => BaseType.IsAssignableFrom(Type) && (Type != BaseType)).ToArray()
			;
			*/
		}
	}
}
