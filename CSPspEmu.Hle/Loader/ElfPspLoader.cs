using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Formats;
using CSharpUtils.Extensions;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Cpu.Cpu.Emiter;

namespace CSPspEmu.Hle.Loader
{
	unsafe public class ElfPspLoader : ElfLoader
	{
		public struct InitInfoStruct
		{
			public uint PC;
			public uint GP;
		}

		public ElfPsp.ModuleInfo ModuleInfo { get; protected set; }
		public InitInfoStruct InitInfo;

		override public void Load(Stream FileStream)
		{
			//ElfPsp.ModuleImport
			base.Load(FileStream);
			this.ModuleInfo = SliceStreamForSectionHeader(SectionHeadersByName[".rodata.sceModuleInfo"]).ReadStruct<ElfPsp.ModuleInfo>();;

			this.InitInfo.PC = this.Header.EntryPoint;
			this.InitInfo.GP = this.ModuleInfo.GP;
		}

		override public void WriteToMemory(Stream MemoryStream)
		{
			base.WriteToMemory(MemoryStream);
		}

		public void UpdateModuleImports(Stream MemoryStream, HleModuleManager ModuleManager)
		{
			var ImportsStream = MemoryStream.SliceWithBounds(ModuleInfo.ImportsStart, ModuleInfo.ImportsEnd);
			var ExportsStream = MemoryStream.SliceWithBounds(ModuleInfo.ImportsStart, ModuleInfo.ImportsEnd);

			var ModuleImports = ImportsStream.ReadStructVectorUntilTheEndOfStream<ElfPsp.ModuleImport>();
			var ModuleExports = ExportsStream.ReadStructVectorUntilTheEndOfStream<ElfPsp.ModuleImport>();

			foreach (var SectionHeader in SectionHeaders)
			{
				//Console.WriteLine("{0}: {1}", NameAt(SectionHeader.Name), SectionHeader);
			}

			foreach (var ModuleImport in ModuleImports)
			{
				MemoryStream.Position = ModuleImport.Name;

				var ModuleImportName = MemoryStream.ReadStringzAt(ModuleImport.Name);

				var NidStreamReader = new BinaryReader(MemoryStream.SliceWithLength(ModuleImport.NidAddress, ModuleImport.FunctionCount * Marshal.SizeOf(typeof(uint))));
				var CallStreamWriter = new BinaryWriter(MemoryStream.SliceWithLength(ModuleImport.CallAddress, ModuleImport.FunctionCount * Marshal.SizeOf(typeof(uint)) * 2));

				var Module = ModuleManager.GetModuleByName(ModuleImportName);

				Console.WriteLine("{0:X}:'{1}'", ModuleImport.Name, ModuleImportName);
				for (int n = 0; n < ModuleImport.FunctionCount; n++)
				{
					uint NID = NidStreamReader.ReadUInt32();
					var NIDName = Module.NamesByNID.GetOrDefault(NID, String.Format("<unknown:{0:X}>", NID));
					//var Delegate = Module.DelegatesByNID.GetOrDefault(NID, null);
					CallStreamWriter.Write((uint)(0x0000000C | (FunctionGenerator.NativeCallSyscallCode << 6))); // syscall 0x2307
					CallStreamWriter.Write(
						(uint)ModuleManager.AllocDelegateSlot(
							Module.DelegatesByNID.GetOrDefault(NID, null),
							String.Format("{0}:{1}", ModuleImportName, NIDName)
						)
					);

					Console.WriteLine(
						"    CODE_ADDR({0:X})  :  NID(0x{1,8:X}) : {2}",
						ModuleImport.CallAddress + n * 8,
						NID,
						Module.NamesByNID.GetOrDefault(NID, "<unknown>")
					);
				}
			}

			//Console.ReadKey();
		}
	}
}
