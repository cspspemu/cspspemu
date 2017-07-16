using System;
using System.Collections.Generic;

namespace CSharpUtils
{
	public class TypeUtils
	{
		public static IEnumerable<Type> GetTypesExtending(Type BaseType)
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
