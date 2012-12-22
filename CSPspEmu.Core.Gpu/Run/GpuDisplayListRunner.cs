using System;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.Run
{
	public unsafe sealed partial class GpuDisplayListRunner
	{
		static Logger Logger = Logger.GetLogger("GpuDisplayListRunner");

		public GlobalGpuState GlobalGpuState;
		public GpuDisplayList GpuDisplayList;
		public GpuOpCodes OpCode;
		public uint Params24;
		public static uint PC;

		public ushort Param16(int Offset)
		{
			return (ushort)(Params24 >> Offset);
		}
		public byte Param8(int Offset)
		{
			return (byte)(Params24 >> Offset);
		}

		public uint Extract(int Offset, int Count)
		{
			return BitUtils.Extract(Params24, Offset, Count);
		}

		public TType Extract<TType>(int Offset, int Count)
		{
			return (TType)(object)BitUtils.Extract(Params24, Offset, Count);
		}


		public GpuStateStruct* GpuState
		{
			get
			{
				return GpuDisplayList.GpuStateStructPointer;
			}
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
			if (GpuDisplayList.GpuProcessor.GpuConfig.NoticeUnimplementedGpuCommands)
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
