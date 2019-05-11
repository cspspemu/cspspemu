using System;
using CSharpUtils;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Utils;

namespace CSPspEmu.Hle.Modules.sysmem
{
    [HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
    public class KDebugForKernel : HleModuleHost
    {
        public enum PspDebugKprintfHandler : uint
        {
        }

        /// <summary>
        /// Install a Kprintf handler into the system.
        /// </summary>
        /// <param name="Handler">Function pointer to the handler.</param>
        /// <returns>less than 0 on error.</returns>
        [HlePspFunction(NID = 0x7CEB2C09, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceKernelRegisterKprintfHandler(PspDebugKprintfHandler Handler)
        {
            return 0;
            //throw(new NotImplementedException());
            /*
            Logger.log(Logger.Level.WARNING, "KDebugForKernel", "Not implemented sceKernelRegisterKprintfHandler");
            return -1;
            */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Format"></param>
        /// <param name="CpuThreadState"></param>
        [HlePspFunction(NID = 0x84F370BC, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public void Kprintf(string Format, CpuThreadState CpuThreadState)
        {
            var Arguments = new ArgumentReader(CpuThreadState);
            Arguments.LoadInteger(); // Skips format

            ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Blue,
                () => { Console.Error.Write("{0}", CStringFormater.Sprintf(Format, Arguments)); });
        }
    }
}