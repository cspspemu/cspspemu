namespace CSPspEmu.Core.Cpu
{
	public class SyscallInfo
	{
		public const ushort NativeCallSyscallCode = 0x1234;

		public static uint NativeCallSyscallOpCode
		{
			get
			{
				return (uint)(0x0000000C | (SyscallInfo.NativeCallSyscallCode << 6));
			}
		}
	}
}
