using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Globalization;

namespace CSPspEmu.Resources
{
	public class Translations
	{
		static private Dictionary<string, Dictionary<string, Dictionary<string, string>>> Dictionary;

		static private void Parse()
		{
			Dictionary = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

			try
			{
				var Document = new XmlDocument();
				Document.LoadXml(ResourceArchive.GetTranslationsStream().ReadAllContentsAsString());
				foreach (var CategoryNode in Document.SelectNodes("/translations/category").Cast<XmlNode>())
				{
					var CategoryId = CategoryNode.Attributes["id"].Value;
					if (!Dictionary.ContainsKey(CategoryId)) Dictionary[CategoryId] = new Dictionary<string, Dictionary<string, string>>();
					foreach (var TextNode in CategoryNode.SelectNodes("text").Cast<XmlNode>())
					{
						var TextId = TextNode.Attributes["id"].Value;
						if (!Dictionary[CategoryId].ContainsKey(TextId)) Dictionary[CategoryId][TextId] = new Dictionary<string, string>();
						foreach (var TranslationNode in TextNode.SelectNodes("translation").Cast<XmlNode>())
						{
							var LangId = TranslationNode.Attributes["lang"].Value;
							var Text = TranslationNode.InnerText;
							//Console.WriteLine("{0}.{1}.{2} = {3}", CategoryId, TextId, LangId, Text);
							Dictionary[CategoryId][TextId][LangId] = Text;
						}
					}
				}
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
			}
		}

		static public string GetString(string CategoryId, string TextId, CultureInfo CultureInfo = null)
		{
			if (Dictionary == null)
			{
				Parse();
			}
			if (CultureInfo == null)
			{
				CultureInfo = CultureInfo.CurrentUICulture;
			}

			var LangId = CultureInfo.TwoLetterISOLanguageName;


			dynamic Category = null;
			dynamic CategoryText = null;

			try
			{
				Category = Dictionary[CategoryId];
				CategoryText = Category[TextId];
				return CategoryText[LangId];
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine("Can't find key '{0}.{1}.{2}'", CategoryId, TextId, LangId);
				Console.Error.WriteLine(Exception);
				try
				{
					return CategoryText["en"];
				}
				catch
				{
					return String.Format("{0}.{1}", CategoryId, TextId);
				}
			}
		}
	}
}
