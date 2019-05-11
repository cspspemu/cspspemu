using System;

namespace CSPspEmu.Core.Cpu
{
    public class PspBreakException : Exception
    {
        public PspBreakException(string message) : base(message)
        {
        }
    }
}