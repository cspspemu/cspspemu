using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast
{
	public class AstArgument
	{
		public readonly int Index;
		public readonly Type Type;
		public readonly string Name;

		public AstArgument(int Index, Type Type, string Name = null)
		{
			this.Index = Index;
			this.Type = Type;
			this.Name = (Name == null) ? ("@ARG(" + Index + ")") : Name;
		}

		static public AstArgument Create(Type Type, int Index, string Name = null)
		{
			return new AstArgument(Index, Type, Name);
		}

		static public AstArgument Create<TType>(int Index, string Name = null)
		{
			return Create(typeof(TType), Index, Name);
		}

		static public AstArgument Create(MethodInfo MethodInfo, int Index, string Name = null)
		{
			if (Name == null) Name = MethodInfo.GetParameters()[Index].Name;
			return new AstArgument(Index, MethodInfo.GetParameters()[Index].ParameterType, Name);
		}
	}
}
