namespace CSPspEmu.Core.Cpu.VFpu
{
    public enum VfpuControlRegistersEnum
    {
        /// <summary>Source prefix stack</summary>
        VfpuPfxs = 128,

        /// <summary>
        /// Target prefix stack
        /// </summary>
        VfpuPfxt = 129,

        /// <summary>
        /// Destination prefix stack
        /// </summary>
        VfpuPfxd = 130,

        /// <summary>
        /// Condition information
        /// </summary>
        VfpuCc = 131,

        /// <summary>
        /// VFPU internal information 4
        /// </summary>
        VfpuInf4 = 132,

        /// <summary>
        /// Not used (reserved)
        /// </summary>
        VfpuRsv5 = 133,

        /// <summary>
        /// Not used (reserved)
        /// </summary>
        VfpuRsv6 = 134,

        /// <summary>
        /// VFPU revision information
        /// </summary>
        VfpuRev = 135,

        /// <summary>
        /// Pseudorandom number generator information 0
        /// </summary>
        VfpuRcx0 = 136,

        /// <summary>
        /// Pseudorandom number generator information 1
        /// </summary>
        VfpuRcx1 = 137,

        /// <summary>
        /// Pseudorandom number generator information 2
        /// </summary>
        VfpuRcx2 = 138,

        /// <summary>
        /// Pseudorandom number generator information 3
        /// </summary>
        VfpuRcx3 = 139,

        /// <summary>
        /// Pseudorandom number generator information 4
        /// </summary>
        VfpuRcx4 = 140,

        /// <summary>
        /// Pseudorandom number generator information 5
        /// </summary>
        VfpuRcx5 = 141,

        /// <summary>
        /// Pseudorandom number generator information 6
        /// </summary>
        VfpuRcx6 = 142,

        /// <summary>
        /// Pseudorandom number generator information 7
        /// </summary>
        VfpuRcx7 = 143,
    }
}