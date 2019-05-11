using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CSharpUtils.Extensions;

namespace CSPspEmu.Resources
{
    public class ResourceArchive
    {
        public static Assembly CurrentAssembly => typeof(ResourceArchive).GetTypeInfo().Assembly;
        //public static Assembly Assembly => System.Reflection.Assembly.GetExecutingAssembly();
        
        public static Stream GetFlash0ZipFileStream()
        {
            Console.WriteLine("CurrentAssembly.GetManifestResourceNames: " + CurrentAssembly.GetManifestResourceNames().ToList().ToStringList());

            return CurrentAssembly.GetManifestResourceStream("CSPspEmu.Resources.flash0.zip")
                   ?? throw new Exception("Not found flash0.zip");
        }

        public static Stream GetTranslationsStream()
        {
            return CurrentAssembly.GetManifestResourceStream("CSPspEmu.Resources.Translations.xml")
                   ?? throw new Exception("Not found Translations.xml");
        }
    }
}