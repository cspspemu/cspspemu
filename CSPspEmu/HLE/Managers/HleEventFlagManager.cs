using CSPspEmu.Hle.Threading.EventFlags;

namespace CSPspEmu.Hle.Managers
{
    public enum EventFlagId
    {
    }

    public class HleEventFlagManager
    {
        public HleUidPoolSpecial<HleEventFlag, EventFlagId> EventFlags =
            new HleUidPoolSpecial<HleEventFlag, EventFlagId>();
    }
}