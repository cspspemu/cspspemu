using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSharpUtils.Templates.TemplateProvider
{
	public class TemplateProviderMemory : ITemplateProvider
    {
        Dictionary<String, Stream> Map;

        public TemplateProviderMemory()
        {
            Map = new Dictionary<string, Stream>();
        }

        public void Add(String Name, Stream Data)
        {
            Map[Name] = Data;
        }

        public void Add(String Name, String Data, Encoding Encoding = null)
        {
            if (Encoding == null) Encoding = Encoding.UTF8;
            Add(Name, new MemoryStream(Data.GetBytes(Encoding)));
        }

        public Stream GetTemplate(string Name)
        {
            if (!Map.ContainsKey(Name)) throw(new Exception(String.Format("Not Mapped File '{0}'", Name)));
            return Map[Name];
        }
    }
}
