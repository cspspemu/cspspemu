using System;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using System.Runtime.CompilerServices;
using System.Runtime;

namespace CSPspEmu.Core.Gpu.Run
{
	public unsafe sealed partial class GpuDisplayListRunner
	{
		public static readonly GpuDisplayListRunner Methods = new GpuDisplayListRunner();

		private static readonly Logger Logger = Logger.GetLogger("GpuDisplayListRunner");

		public GlobalGpuState GlobalGpuState;
		public GpuDisplayList GpuDisplayList;
		public GpuOpCodes OpCode;
		public uint Params24;
		public uint PC;


		private GpuDisplayListRunner()
		{
		}

		public GpuDisplayListRunner(GpuDisplayList GpuDisplayList, State.GlobalGpuState GlobalGpuState)
		{
			this.GpuDisplayList = GpuDisplayList;
			this.GlobalGpuState = GlobalGpuState;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public ushort Param16(int Offset)
		{
			return (ushort)(Params24 >> Offset);
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public byte Param8(int Offset)
		{
			return (byte)(Params24 >> Offset);
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public uint Extract(int Offset, int Count)
		{
			return BitUtils.Extract(Params24, Offset, Count);
		}

		////[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		//public TType Extract<TType>(int Offset, int Count)
		//{
		//	return (TType)BitUtils.Extract(Params24, Offset, Count);
		//}

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
				Console.Error.WriteLineColored(ConsoleColor.Red, "Unimplemented GpuOpCode: {0} : {1:X}", OpCode, Params24);
			}
		}

		public void OP_UNKNOWN()
		{
			//NoticeUnimplementedGpuCommands
			Console.WriteLine("Unhandled GpuOpCode: {0} : {1:X}", OpCode, Params24);
		}
	}
}
