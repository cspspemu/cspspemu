namespace CSPspEmu.Core.Threading.Synchronization
{
    public class PspManualResetEvent : PspResetEvent
    {
        public PspManualResetEvent(bool InitialValue)
            : base(InitialValue, AutoReset: false)
        {
        }
    }
}