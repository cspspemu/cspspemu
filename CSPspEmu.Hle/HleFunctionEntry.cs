using CSPspEmu.Core.Cpu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Hle
{
	public struct HleFunctionEntry
	{
		public uint NID;
		public String Name;
		public String Description;
		public HleModuleHost Module;
		public string ModuleName;
		public Action<CpuThreadState> Delegate;

		public override string ToString()
		{
			return String.Format("FunctionEntry(NID=0x{0:X}, Name='{1}', Description='{2}', Module='{3}')", NID, Name, Description, Module);
		}
	}
}
