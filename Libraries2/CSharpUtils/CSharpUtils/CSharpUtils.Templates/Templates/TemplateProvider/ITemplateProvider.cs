using System;
using System.IO;

namespace CSharpUtils.Templates.TemplateProvider
{
	public interface ITemplateProvider
	{
		Stream GetTemplate(String Name);
	}
}
