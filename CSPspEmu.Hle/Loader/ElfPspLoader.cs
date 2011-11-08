using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Formats;
using CSharpUtils.Extensions;
using System.IO;

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
			base.Load(FileStream);
			this.ModuleInfo = SliceStreamForSectionHeader(SectionHeadersByName[".rodata.sceModuleInfo"]).ReadStruct<ElfPsp.ModuleInfo>();;
			this.InitInfo.PC = this.Header.EntryPoint;
			this.InitInfo.GP = this.ModuleInfo.GP;
		}
	}
}
