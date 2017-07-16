using System;
using CSharpUtils.Templates;
using CSharpUtils.Templates.TemplateProvider;

namespace CSharpUtilsTemplateTest
{
	class Program
	{
		static void Main()
		{
			//TemplateProvider TemplateProvider = new TemplateProviderVirtualFileSystem(new LocalFileSystem(FileUtils.GetExecutableDirectoryPath(), false));
			TemplateProviderMemory TemplateProvider = new TemplateProviderMemory();
			TemplateFactory TemplateFactory = new TemplateFactory(TemplateProvider);

			TemplateProvider.Add("Base.html", "Test{% block Body %}Base{% endblock %}Test");
			TemplateProvider.Add("Test.html", "{% extends 'Base.html' %}{% block Body %}Ex{% endblock %}");
			//TemplateProvider.Add("Test.html", "{% block Body %}Ex{% endblock %}");

			Console.WriteLine(TemplateFactory.GetTemplateCodeByFile("Test.html").RenderToString());

			Console.ReadKey();
		}
	}
}
