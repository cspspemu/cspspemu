using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Gpu.Run
{
	unsafe sealed public partial class GpuDisplayListRunner
	{
		public GpuDisplayList GpuDisplayList;
		public GpuOpCodes OpCode;
		public uint Params24;

		public ushort Param16(int Offset)
		{
			return (ushort)(Params24 >> Offset);
		}
		public byte Param8(int Offset)
		{
			return (byte)(Params24 >> Offset);
		}

		public float Float1
		{
			get
			{
				return MathFloat.ReinterpretUIntAsFloat(Params24 << 8);
			}
		}

		public bool Bool1
		{
			get
			{
				return Params24 != 0;
			}
		}

		public void UNIMPLEMENTED_NOTICE()
		{
			if (GpuDisplayList.GpuProcessor.PspConfig.NoticeUnimplementedGpuCommands)
			{
				Console.WriteLine("Unimplemented GpuOpCode: {0} : {1:X}", OpCode, Params24);
			}
		}

		public void OP_UNKNOWN()
		{
			//NoticeUnimplementedGpuCommands
			Console.WriteLine("Unhandled GpuOpCode: {0} : {1:X}", OpCode, Params24);
		}
	}
}
