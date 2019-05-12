using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Xml;
using CSharpUtils.Extensions;

namespace CSPspEmu.Resources
{
    public class Translations
    {
        private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> _dictionary;
        private static SortedSet<string> _availableLanguages;

        public static SortedSet<string> AvailableLanguages
        {
            get
            {
                if (_dictionary == null) Parse();
                return _availableLanguages;
            }
        }

        public static string DefaultLanguage;

        private static void Parse()
        {
            _dictionary = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            _availableLanguages = new SortedSet<string>();

            try
            {
                var document = new XmlDocument();
                document.LoadXml(ResourceArchive.GetTranslationsStream().ReadAllContentsAsString());
                foreach (var categoryNode in
                    document.SelectNodes("/translations/category")?.Cast<XmlNode>() ?? new XmlNode[0]
                )
                {
                    var categoryId = categoryNode?.Attributes?["id"]?.Value ?? "";
                    if (!_dictionary.ContainsKey(categoryId))
                        _dictionary[categoryId] = new Dictionary<string, Dictionary<string, string>>();

                    foreach (var textNode in categoryNode?.SelectNodes("text")?.Cast<XmlNode>() ?? new XmlNode[0])
                    {
                        var textId = textNode?.Attributes?["id"]?.Value ?? "";

                        if (!_dictionary[categoryId].ContainsKey(textId))
                            _dictionary[categoryId][textId] = new Dictionary<string, string>();

                        foreach (var translationNode in textNode?.SelectNodes("translation")?.Cast<XmlNode>() ??
                                                        new XmlNode[0])
                        {
                            var langId = translationNode?.Attributes["lang"].Value;
                            var text = translationNode.InnerText;
                            if (DefaultLanguage == null)
                            {
                                DefaultLanguage = langId;
                            }
                            if (langId != "xx")
                            {
                                AvailableLanguages.Add(langId);
                            }
                            //Console.WriteLine("{0}.{1}.{2} = {3}", CategoryId, TextId, LangId, Text);
                            _dictionary[categoryId][textId][langId] = text;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
            }
        }

        private static readonly Dictionary<string, Image> FlagCache = new Dictionary<string, Image>();

        public static Image GetLangFlagImage(string langId)
        {
            try
            {
                if (!FlagCache.ContainsKey(langId))
                {
                    var bitmapStream =
                        typeof(Translations).Assembly.GetManifestResourceStream(
                            "CSPspEmu.Resources.Images.Languages." + langId + ".png");
                    if (bitmapStream != null) FlagCache[langId] = Image.FromStream(bitmapStream);
                }
                return FlagCache[langId];
            }
            catch
            {
                return null;
            }
        }

        public static string GetString(string categoryId, string textId, string langId = null)
        {
            if (_dictionary == null) Parse();
            if (_dictionary == null) throw new NullReferenceException();
            if (langId == null) langId = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            Dictionary<string, string> categoryText = null;

            try
            {
                var category = _dictionary[categoryId];
                categoryText = category[textId];
                return categoryText.ContainsKey("xx") ? categoryText["xx"] : categoryText[langId];
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Can't find key '{0}.{1}.{2}'", categoryId, textId, langId);
                Console.Error.WriteLine(exception);
                return categoryText?[DefaultLanguage] ?? $"{categoryId}.{textId}";
            }
        }

        public static string GetString(string categoryId, string textId, CultureInfo cultureInfo)
        {
            var langId = cultureInfo.TwoLetterISOLanguageName;

            return GetString(categoryId, textId, langId);
        }
    }
}