using CSPspEmu.Hle.Loader;

using System.IO;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle;
using CSharpUtils.Extensions;
using Xunit;

namespace CSPspEmu.Core.Tests
{
    
    public class ElfLoaderTest
    {
        [Fact(Skip = "file not found")]
        public void ElfLoaderConstructorTest()
        {
            var InjectContext = new InjectContext();
            InjectContext.SetInstanceType<PspMemory, LazyPspMemory>();
            var Memory = InjectContext.GetInstance<PspMemory>();
            var MemoryStream = new PspMemoryStream(Memory);
            var MemoryPartition = new MemoryPartition(InjectContext, PspMemory.MainOffset,
                PspMemory.MainOffset + PspMemory.MainSize);

            var ElfLoader = new ElfLoader();

            ElfLoader.Load(File.OpenRead("../../../TestInput/minifire.elf"), "minifire.elf");
            ElfLoader.AllocateAndWrite(MemoryStream, MemoryPartition);
            Assert.Equal(1, ElfLoader.ProgramHeaders.Length);
            Assert.Equal(3, ElfLoader.SectionHeaders.Length);

            Assert.Equal(
                "['','.rodata.sceModuleInfo']".Replace('\'', '"'),
                ElfLoader.SectionHeadersByName.Keys.ToJson()
            );

            //ElfLoader.LoadAllocateMemory(MemoryPartition);
            //ElfLoader.LoadWriteToMemory(MemoryStream);

            //var ModuleInfo = ElfLoader.ModuleInfo;

            var PC = ElfLoader.Header.EntryPoint;
            //var GP = ModuleInfo.GP;

            Assert.Equal(0x08900008, (int) PC);
            //Assert.Equal(0x00004821, (int)GP);
        }
    }
}