using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.Run
{
	unsafe sealed public partial class GpuDisplayListRunner
	{
		public GpuDisplayList GpuDisplayList;
		public GpuOpCodes OpCode;
		public uint Params;

		public ushort Param16(int Offset)
		{
			return (ushort)(Params >> Offset);
		}
		public byte Param8(int Offset)
		{
			return (byte)(Params >> Offset);
		}
	}
}
