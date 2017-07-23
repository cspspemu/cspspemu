using System.IO;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle;
using CSPspEmu.Hle.Loader;
using Xunit;

namespace Tests.CSPspEmu.Hle.Loader
{
    
    public class ElfLoaderTest
    {
        [Fact(Skip = "file not found")]
        public void ElfLoaderConstructorTest()
        {
            var injectContext = new InjectContext();
            injectContext.SetInstanceType<PspMemory, LazyPspMemory>();
            var memory = injectContext.GetInstance<PspMemory>();
            var memoryStream = new PspMemoryStream(memory);
            var memoryPartition = new MemoryPartition(injectContext, PspMemory.MainOffset,
                PspMemory.MainOffset + PspMemory.MainSize);

            var elfLoader = new ElfLoader();

            elfLoader.Load(File.OpenRead("../../../TestInput/minifire.elf"), "minifire.elf");
            elfLoader.AllocateAndWrite(memoryStream, memoryPartition);
            Assert.Equal(1, elfLoader.ProgramHeaders.Length);
            Assert.Equal(3, elfLoader.SectionHeaders.Length);

            Assert.Equal(
                "['','.rodata.sceModuleInfo']".Replace('\'', '"'),
                elfLoader.SectionHeadersByName.Keys.ToJson()
            );

            //ElfLoader.LoadAllocateMemory(MemoryPartition);
            //ElfLoader.LoadWriteToMemory(MemoryStream);

            //var ModuleInfo = ElfLoader.ModuleInfo;

            var pc = elfLoader.Header.EntryPoint;
            //var GP = ModuleInfo.GP;

            Assert.Equal(0x08900008, (int) pc);
            //Assert.Equal(0x00004821, (int)GP);
        }
    }
}