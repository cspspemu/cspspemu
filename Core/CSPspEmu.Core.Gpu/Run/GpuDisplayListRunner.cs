using System;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using System.Runtime;

namespace CSPspEmu.Core.Gpu.Run
{
    public sealed unsafe partial class GpuDisplayListRunner
    {
        public static readonly GpuDisplayListRunner Methods = new GpuDisplayListRunner();

        private static readonly Logger Logger = Logger.GetLogger("GpuDisplayListRunner");

        public GlobalGpuState GlobalGpuState;
        public GpuDisplayList GpuDisplayList;
        public GpuOpCodes OpCode;
        public uint Params24;
        public uint Pc;


        private GpuDisplayListRunner()
        {
        }

        public GpuDisplayListRunner(GpuDisplayList gpuDisplayList, GlobalGpuState globalGpuState)
        {
            GpuDisplayList = gpuDisplayList;
            GlobalGpuState = globalGpuState;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public ushort Param16(int offset) => (ushort) (Params24 >> offset);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public byte Param8(int offset) => (byte) (Params24 >> offset);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public uint Extract(int offset, int count) => BitUtils.Extract(Params24, offset, count);

        ////[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //public TType Extract<TType>(int Offset, int Count)
        //{
        //	return (TType)BitUtils.Extract(Params24, Offset, Count);
        //}

        public GpuStateStruct* GpuState => GpuDisplayList.GpuStateStructPointer;
        public float Float1 => MathFloat.ReinterpretUIntAsFloat(Params24 << 8);
        public bool Bool1 => Params24 != 0;

        public void UNIMPLEMENTED_NOTICE()
        {
            if (GpuDisplayList.GpuProcessor.GpuConfig.NoticeUnimplementedGpuCommands)
            {
                Console.Error.WriteLineColored(ConsoleColor.Red, "Unimplemented GpuOpCode: {0} : {1:X}", OpCode,
                    Params24);
            }
        }

        [GpuInstructionAttribute(GpuOpCodes.UNKNOWN)]
        public void OP_UNKNOWN() => Console.WriteLine("Unhandled GpuOpCode: {0} : {1:X}", OpCode, Params24);
    }
}