using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class InjectMapAttribute : Attribute
{
	public Type From { get; set; }
	public Type To { get; set; }

	public InjectMapAttribute(Type @from, Type to)
	{
		this.From = @from;
		this.To = to;
	}
}