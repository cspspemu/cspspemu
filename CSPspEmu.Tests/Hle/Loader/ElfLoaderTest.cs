using CSPspEmu.Hle.Loader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class ElfLoaderTest
	{
		[TestMethod]
		public void ElfLoaderConstructorTest()
		{
			var PspConfig = new PspConfig();
			var PspEmulatorContext = new PspEmulatorContext(PspConfig);
			PspEmulatorContext.SetInstanceType<PspMemory, LazyPspMemory>();
			var Memory = PspEmulatorContext.GetInstance<PspMemory>();
			var MemoryStream = new PspMemoryStream(Memory);
			var MemoryPartition = new MemoryPartition(PspMemory.MainOffset, PspMemory.MainOffset + PspMemory.MainSize);

			var ElfLoader = new ElfLoader();

			ElfLoader.Load(File.OpenRead("../../../TestInput/minifire.elf"));
			ElfLoader.AllocateAndWrite(MemoryStream, MemoryPartition);
			Assert.AreEqual(1, ElfLoader.ProgramHeaders.Length);
			Assert.AreEqual(3, ElfLoader.SectionHeaders.Length);

			Assert.AreEqual(
				"['','.rodata.sceModuleInfo']".Replace('\'', '"'),
				ElfLoader.SectionHeadersByName.Keys.ToJson()
			);

			//ElfLoader.LoadAllocateMemory(MemoryPartition);
			//ElfLoader.LoadWriteToMemory(MemoryStream);

			//var ModuleInfo = ElfLoader.ModuleInfo;

			var PC = ElfLoader.Header.EntryPoint;
			//var GP = ModuleInfo.GP;

			Assert.AreEqual(0x08900008, (int)PC);
			//Assert.AreEqual(0x00004821, (int)GP);
		}
	}
}
