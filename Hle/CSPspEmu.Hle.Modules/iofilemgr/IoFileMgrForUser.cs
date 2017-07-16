using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core;
using CSharpUtils;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public partial class IoFileMgrForUser : HleModuleHost
    {
        static Logger Logger = Logger.GetLogger("IoFileMgrForUser");

        [Inject] protected HleIoManager HleIoManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileHandle"></param>
        /// <returns></returns>
        public HleIoDrvFileArg GetFileArgFromHandle(SceUID FileHandle)
        {
            return HleIoManager.HleIoDrvFileArgPool.Get(FileHandle);
        }

        public override void Dispose()
        {
            HleIoManager.HleIoDrvFileArgPool.RemoveAll();
            base.Dispose();
        }
    }
}