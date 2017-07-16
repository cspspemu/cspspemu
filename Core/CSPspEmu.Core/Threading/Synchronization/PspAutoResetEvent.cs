namespace CSPspEmu.Core.Threading.Synchronization
{
	public class PspAutoResetEvent : PspResetEvent
	{
		public PspAutoResetEvent(bool initialValue)
			: base(initialValue, autoReset: true)
		{
		}
	}
}
