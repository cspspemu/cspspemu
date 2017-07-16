namespace CSPspEmu.Hle.Vfs
{
    public static class HleIoDriverExtensions
    {
        public static IHleIoDriver AsReadonlyHleIoDriver(this IHleIoDriver HleIoDriver)
        {
            return new ReadonlyHleIoDriver(HleIoDriver);
        }
    }
}