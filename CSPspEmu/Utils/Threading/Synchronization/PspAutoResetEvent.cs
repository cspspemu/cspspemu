namespace CSPspEmu.Core.Threading.Synchronization
{
    public class PspAutoResetEvent : PspResetEvent
    {
        public PspAutoResetEvent(bool InitialValue)
            : base(InitialValue, AutoReset: true)
        {
        }
    }
}