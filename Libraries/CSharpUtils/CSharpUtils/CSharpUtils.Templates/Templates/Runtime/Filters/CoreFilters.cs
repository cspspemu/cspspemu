using System;
using CSharpUtils.Html;

namespace CSharpUtils.Templates.Runtime.Filters
{
	public class CoreFilters
	{
		[TemplateFilter(Name="format")]
		public static string Format(String Format, params Object[] Params)
		{
			return String.Format(Format, Params);
		}

		[TemplateFilter(Name = "raw")]
		public static RawWrapper Raw(dynamic Object)
		{
			return RawWrapper.Get(Object);
		}

		[TemplateFilter(Name = "escape")]
		public static RawWrapper Escape(dynamic Object)
		{
			return RawWrapper.Get(HtmlUtils.EscapeHtmlCharacters("" + Object));
		}

		[TemplateFilter(Name = "e")]
		public static RawWrapper E(dynamic Object)
		{
			return Escape(Object);
		}
	}
}
