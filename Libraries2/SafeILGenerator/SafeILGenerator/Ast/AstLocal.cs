using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast
{
	sealed public class AstLocal
	{
		public readonly string Name;
		public readonly Type Type;

		private AstLocal(Type Type, string Name)
		{
			this.Name = Name;
			this.Type = Type;
		}

		static public AstLocal Create(Type Type, string Name = "<Unknown>")
		{
			return new AstLocal(Type, Name);
		}

		static public AstLocal Create<TType>(string Name = "<Unknown>")
		{
			return Create(typeof(TType), Name);
		}
	}
}
