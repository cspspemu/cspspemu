namespace CSPspEmu.Core.Threading.Synchronization
{
	public class PspManualResetEvent : PspResetEvent
	{
		public PspManualResetEvent(bool initialValue)
			: base(initialValue, autoReset: false)
		{
		}
	}
}
