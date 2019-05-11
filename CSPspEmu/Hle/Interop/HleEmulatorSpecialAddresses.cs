namespace CSPspEmu.Hle.Interop
{
    public class HleEmulatorSpecialAddresses
    {
        public const uint CODE_PTR_EXIT_THREAD = 0x08000010;
        public const uint CODE_PTR_FINALIZE_CALLBACK = 0x08000020;

        public const int CODE_PTR_EXIT_THREAD_SYSCALL = 0x7777;
        public const int CODE_PTR_FINALIZE_CALLBACK_SYSCALL = 0x7778;
    }
}