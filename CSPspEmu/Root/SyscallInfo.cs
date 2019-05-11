namespace CSPspEmu.Core.Cpu
{
    public class SyscallInfo
    {
        public const ushort NativeCallSyscallCode = 0x1234;

        public static uint NativeCallSyscallOpCode => 0x0000000C | (NativeCallSyscallCode << 6);
    }
}