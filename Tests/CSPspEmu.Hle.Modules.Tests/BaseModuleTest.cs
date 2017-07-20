using System;
using System.Reflection;
using CSharpUtils.Extensions;
using NUnit.Framework;

namespace CSPspEmu.Hle.Modules.Tests
{
    [TestFixture]
    public class BaseModuleTest
    {
        [SetUp]
        public void SetUp()
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