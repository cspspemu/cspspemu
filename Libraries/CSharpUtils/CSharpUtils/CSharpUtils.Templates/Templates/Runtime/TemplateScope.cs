using System;
using System.Collections.Generic;

namespace CSharpUtils.Templates.Runtime
{
	public sealed class TemplateScope
	{
		public TemplateScope ParentScope;
		public Dictionary<String, dynamic> Items;

		public TemplateScope(Dictionary<String, dynamic> Items, TemplateScope ParentScope = null)
		{
			this.ParentScope = ParentScope;
			this.Items = Items;
		}

		public TemplateScope(TemplateScope ParentScope = null)
		{
			this.ParentScope = ParentScope;
			this.Items = new Dictionary<String, dynamic>();
		}

		public dynamic this[String Index]
		{
			set
			{
				Items[Index] = value;
			}
			get
			{
				if (Items.ContainsKey(Index))
				{
					return Items[Index];
				}

				if (ParentScope != null)
				{
					return ParentScope[Index];
				}
				return null;
			}
		}
	}
}
