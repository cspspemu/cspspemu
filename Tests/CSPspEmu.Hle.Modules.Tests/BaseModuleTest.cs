using System;
using System.Reflection;
using CSharpUtils.Extensions;


namespace CSPspEmu.Hle.Modules.Tests
{
    
    public class BaseModuleTest
    {
        public BaseModuleTest()
        {
            TestHleUtils.CreateInjectContext(this);
        }

        public static byte[] ReadResourceBytes(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream(name)
                                 ?? throw new Exception($"Can\'t find resource {name}");
            return resourceStream.ReadAll();
        }
    }
}