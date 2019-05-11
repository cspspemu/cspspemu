namespace CSPspEmu.Core.Cpu.VFpu
{
    public enum VfpuControlRegistersEnum
    {
        VfpuPfxs = 128, // Source prefix stack
        VfpuPfxt = 129, // Target prefix stack
        VfpuPfxd = 130, // Destination prefix stack
        VfpuCc = 131, // Condition information
        VfpuInf4 = 132, // VFPU internal information 4
        VfpuRsv5 = 133, // Not used (reserved)
        VfpuRsv6 = 134, // Not used (reserved)
        VfpuRev = 135, // VFPU revision information
        VfpuRcx0 = 136, // Pseudorandom number generator information 0
        VfpuRcx1 = 137, // Pseudorandom number generator information 1
        VfpuRcx2 = 138, // Pseudorandom number generator information 2
        VfpuRcx3 = 139, // Pseudorandom number generator information 3
        VfpuRcx4 = 140, // Pseudorandom number generator information 4
        VfpuRcx5 = 141, // Pseudorandom number generator information 5
        VfpuRcx6 = 142, // Pseudorandom number generator information 6
        VfpuRcx7 = 143, // Pseudorandom number generator information 7
    }
}