using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast
{
	public class AstLabel
	{
		public string Name;

		protected AstLabel(string Name)
		{
			this.Name = Name;
		}

		static public AstLabel CreateLabel(string Name = "<Unknown>")
		{
			return new AstLabel(Name);
		}

		//static public AstLabel CreateNewLabelFromILGenerator(ILGenerator ILGenerator, string Name = "<Unknown>")
		//{
		//	return new AstLabel((ILGenerator != null) ? ILGenerator.DefineLabel() : default(Label), Name);
		//}

		public override string ToString()
		{
			return String.Format("AstLabel({0})", Name);
		}
	}
}
