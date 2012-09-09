namespace CSPspEmu.Hle.Vfs
{
	static public class HleIoDriverExtensions
	{
		static public IHleIoDriver AsReadonlyHleIoDriver(this IHleIoDriver HleIoDriver)
		{
			return new ReadonlyHleIoDriver(HleIoDriver);
		}
	}
}
