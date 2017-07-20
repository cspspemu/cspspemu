using System;

namespace CSPspEmu.Core.Cpu
{
    public class InvalidAddressException : Exception
    {
        public InvalidAddressException(string message) : base(message)
        {
        }

        public InvalidAddressException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidAddressException(ulong address) : base($"Invalid Address : 0x{address:X8}")
        {
        }

        public InvalidAddressException(ulong address, Exception innerException) : base(
            $"Invalid Address : 0x{address:X8}", innerException)
        {
        }
    }
}