namespace CSPspEmu.Hle.Vfs.MemoryStick
{
    public interface IMemoryStickEventHandler
    {
        void ScheduleCallback(int CallbackId, int Arg1, int Arg2);
    }
}