using CSPspEmu.Core.Cpu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Hle
{
	public struct DelegateInfo
	{
		public int CallIndex;
		public uint PC;
		public uint RA;
		public string ModuleImportName;
		public HleFunctionEntry FunctionEntry;
		public Action<CpuThreadState> Action;
		public HleThread Thread;

		public override string ToString()
		{
			try
			{
				return String.Format(
					"{0}: PC=0x{3:X}, RA=0x{4:X} => '{5}' : {1}::{2}",
					CallIndex, ModuleImportName, FunctionEntry.Name, PC, RA, (Thread != null) ? Thread.Name : "-");
			}
			catch (Exception Exception)
			{
				return String.Format("Invalid DelegateInfo : " + Exception);
			}
			//return this.ToStringDefault();
		}
	}
}
