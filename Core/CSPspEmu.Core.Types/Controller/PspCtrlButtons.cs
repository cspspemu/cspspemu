using System;

namespace CSPspEmu.Core.Types.Controller
{
    [Flags]
    public enum PspCtrlButtons : uint
    {
        None = 0x0000000,
        Select = 0x0000001, // Select button.
        Start = 0x0000008, // Start button.
        Up = 0x0000010, // Up D-Pad button.
        Right = 0x0000020, // Right D-Pad button.
        Down = 0x0000040, // Down D-Pad button.
        Left = 0x0000080, // Left D-Pad button.
        LeftTrigger = 0x0000100, // Left trigger.
        RightTrigger = 0x0000200, // Right trigger.
        Triangle = 0x0001000, // Triangle button.
        Circle = 0x0002000, // Circle button.
        Cross = 0x0004000, // Cross button.
        Square = 0x0008000, // Square button.
        Home = 0x0010000, // Home button. In user mode this bit is set if the exit dialog is visible.
        Hold = 0x0020000, // Hold button.
        WirelessLanUp = 0x0040000, // Wlan switch up.
        Remote = 0x0080000, // Remote hold position.
        VolumeUp = 0x0100000, // Volume up button.
        VolumeDown = 0x0200000, // Volume down button.
        Screen = 0x0400000, // Screen button.
        Note = 0x0800000, // Music Note button.
        DiscPresent = 0x1000000, // Disc present.
        MemoryStickPresent = 0x2000000, // Memory stick present.
    }
}