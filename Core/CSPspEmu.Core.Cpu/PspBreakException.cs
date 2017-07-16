using System;

namespace CSPspEmu.Core.Cpu
{
    public class PspBreakException : Exception
    {
        public PspBreakException(string Message) : base(Message)
        {
        }
    }
}