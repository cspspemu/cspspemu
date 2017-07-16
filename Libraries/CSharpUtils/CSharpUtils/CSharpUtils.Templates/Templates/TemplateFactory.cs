using System;
using System.Collections.Generic;
using System.Text;
using CSharpUtils.Templates.Runtime;
using CSharpUtils.Templates.TemplateProvider;

namespace CSharpUtils.Templates
{
	public class TemplateFactory
	{
		public Encoding Encoding;
		public ITemplateProvider TemplateProvider;
		public Dictionary<String, Type> CachedTemplatesByFile = new Dictionary<string, Type>();
		protected bool OutputGeneratedCode;

		public TemplateFactory(ITemplateProvider TemplateProvider = null, Encoding Encoding = null, bool OutputGeneratedCode = false)
		{
			this.Encoding = Encoding;
			this.TemplateProvider = TemplateProvider;
			this.OutputGeneratedCode = OutputGeneratedCode;
		}

		protected Type GetTemplateCodeTypeByString(String TemplateString)
		{
			var TemplateCodeGen = new TemplateCodeGen(TemplateString, this);
			TemplateCodeGen.OutputGeneratedCode = OutputGeneratedCode;

			return TemplateCodeGen.GetTemplateCodeType();
			//return new TemplateCodeGenRoslyn(TemplateString, this).GetTemplateCodeType();
		}

		protected Type GetTemplateCodeTypeByFile(String Name)
		{
			if (TemplateProvider == null) throw(new Exception("No specified TemplateProvider"));
			lock (CachedTemplatesByFile)
			{
				if (!CachedTemplatesByFile.ContainsKey(Name))
				{
					using (var TemplateStream = TemplateProvider.GetTemplate(Name))
					{
						return CachedTemplatesByFile[Name] = GetTemplateCodeTypeByString(TemplateStream.ReadAllContentsAsString(Encoding));
					}
				}

				return CachedTemplatesByFile[Name];
			}
		}

		protected TemplateCode CreateInstanceByType(Type Type)
		{
			return (TemplateCode)Activator.CreateInstance(Type, this);
		}

		public TemplateCode GetTemplateCodeByString(String TemplateString)
		{
			return CreateInstanceByType(GetTemplateCodeTypeByString(TemplateString));
		}

		public TemplateCode GetTemplateCodeByFile(String Name)
		{
			return CreateInstanceByType(GetTemplateCodeTypeByFile(Name));
		}
	}
}
