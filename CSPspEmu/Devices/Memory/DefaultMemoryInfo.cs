namespace CSPspEmu.Core.Memory
{
    public class DefaultMemoryInfo : IPspMemoryInfo
    {
        public static DefaultMemoryInfo Instance = new DefaultMemoryInfo();

        private DefaultMemoryInfo()
        {
        }

        public bool IsAddressValid(uint Address)
        {
            return PspMemory.IsAddressValid(Address);
        }
    }
}