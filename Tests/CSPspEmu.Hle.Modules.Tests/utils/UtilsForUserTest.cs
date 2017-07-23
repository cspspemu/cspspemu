
using CSPspEmu.Hle.Modules.utils;
using Xunit;

namespace CSPspEmu.Hle.Modules.Tests.utils
{
    
    public class UtilsForUserTest : BaseModuleTest
    {
        [Inject] protected UtilsForUser UtilsForUser = null;

        [Fact(Skip = "check. Different values")]
        public void TestMt19937()
        {
            SceKernelUtilsMt19937Context sceKernelUtilsMt19937Context;

            UtilsForUser.sceKernelUtilsMt19937Init(out sceKernelUtilsMt19937Context, 0x00000000);
            Assert.Equal(0x39747020U, UtilsForUser.sceKernelUtilsMt19937UInt(ref sceKernelUtilsMt19937Context));

            UtilsForUser.sceKernelUtilsMt19937Init(out sceKernelUtilsMt19937Context, 0x76543210);

            Assert.Equal(0x1BDC5797U, UtilsForUser.sceKernelUtilsMt19937UInt(ref sceKernelUtilsMt19937Context));

            for (int n = 0; n < 1234; n++)
            {
                UtilsForUser.sceKernelUtilsMt19937UInt(ref sceKernelUtilsMt19937Context);
            }

            Assert.Equal(0xE5051779, UtilsForUser.sceKernelUtilsMt19937UInt(ref sceKernelUtilsMt19937Context));
        }
    }
}