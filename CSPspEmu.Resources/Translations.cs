using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace CSPspEmu.Resources
{
	public class Translations
	{
		private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> Dictionary;
		private static SortedSet<string> _AvailableLanguages;
		public static SortedSet<string> AvailableLanguages
		{
			get
			{
				if (Dictionary == null) Parse();
				return _AvailableLanguages;
			}
		}

		public static string DefaultLanguage = null;

		private static void Parse()
		{

			Dictionary = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
			_AvailableLanguages = new SortedSet<string>();

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
							if (DefaultLanguage == null)
							{
								DefaultLanguage = LangId;
							}
							AvailableLanguages.Add(LangId);
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

		private static Dictionary<string, Image> FlagCache = new Dictionary<string, Image>();

		public static Image GetLangFlagImage(string LangId)
		{
			try
			{
				if (!FlagCache.ContainsKey(LangId))
				{
					var BitmapStream = typeof(Translations).Assembly.GetManifestResourceStream("CSPspEmu.Resources.Images.Languages." + LangId + ".png");
					FlagCache[LangId] = Image.FromStream(BitmapStream);
				}
				return FlagCache[LangId];
			}
			catch
			{
				return null;
			}
		}

		public static string GetString(string CategoryId, string TextId, string LangId = null)
		{
			if (Dictionary == null) Parse();
			if (LangId == null) LangId = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

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
					return CategoryText[DefaultLanguage];
				}
				catch
				{
					return String.Format("{0}.{1}", CategoryId, TextId);
				}
			}
		}

		public static string GetString(string CategoryId, string TextId, CultureInfo CultureInfo)
		{
			var LangId = CultureInfo.TwoLetterISOLanguageName;

			return GetString(CategoryId, TextId, LangId);
		}
	}
}
