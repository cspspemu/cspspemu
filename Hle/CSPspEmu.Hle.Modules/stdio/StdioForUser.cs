using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Modules.iofilemgr;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Modules.stdio
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public class StdioForUser : HleModuleHost
    {
        [Inject] HleModuleManager ModuleManager;

        [Inject] IoFileMgrForUser IoFileMgrForUser;

        // ::TODO Faked STD. We should properly open streams.
        public enum StdHandle : int
        {
            /*
            In  = -1,
            Out = -2,
            Error = -3
            */
            In = 10000001,
            Out = 10000002,
            Error = 10000003,
        }

        public enum SceMode : uint
        {
        }

        StdHandle StdIn = StdHandle.In;
        StdHandle StdOut = StdHandle.Out;
        StdHandle StdError = StdHandle.Error;

        /// <summary>
        /// Function to get the current standard in file no
        /// </summary>
        /// <returns>The stdin fileno</returns>
        [HlePspFunction(NID = 0x172D316E, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public StdHandle sceKernelStdin()
        {
            return StdIn;
        }

        /// <summary>
        /// Function to get the current standard out file no
        /// </summary>
        /// <returns>The stdout fileno</returns>
        [HlePspFunction(NID = 0xA6BAB2E9, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public StdHandle sceKernelStdout()
        {
            return StdOut;
        }

        /// <summary>
        /// Function to get the current standard err file no
        /// </summary>
        /// <returns>The stderr fileno</returns>
        [HlePspFunction(NID = 0xF78BA90A, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public StdHandle sceKernelStderr()
        {
            return StdError;
        }

        /// <summary>
        /// Function reopen the stdout file handle to a new file
        /// </summary>
        /// <param name="File">The file to open.</param>
        /// <param name="Flags">The open flags </param>
        /// <param name="Mode">The file mode</param>
        /// <returns>&lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x98220F3E, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public StdHandle sceKernelStdoutReopen(string File, HleIoFlags Flags, Vfs.SceMode Mode)
        {
            StdOut = (StdHandle) IoFileMgrForUser.sceIoOpen(File, Flags, Mode);
            //Console.WriteLine("StdOut: {0}", StdOut);
            return StdOut;
        }

        /// <summary>
        /// Function reopen the stderr file handle to a new file
        /// </summary>
        /// <param name="File">The file to open.</param>
        /// <param name="Flags">The open flags </param>
        /// <param name="Mode">The file mode</param>
        /// <returns>&lt; 0 on error.</returns>
        [HlePspFunction(NID = 0xFB5380C5, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public StdHandle sceKernelStderrReopen(string File, HleIoFlags Flags, Vfs.SceMode Mode)
        {
            StdError = (StdHandle) IoFileMgrForUser.sceIoOpen(File, Flags, Mode);
            return StdError;
        }
    }
}