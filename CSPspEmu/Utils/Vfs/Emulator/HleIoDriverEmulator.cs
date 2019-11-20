using System;
//using System.Drawing.Imaging;
using System.Text;
using CSPspEmu.Core;
using CSPspEmu.Core.Components.Display;
using System.Drawing.Imaging;

namespace CSPspEmu.Hle.Vfs.Emulator
{
    /// <summary>
    /// 
    /// </summary>
    public enum EmulatorDevclEnum : int
    {
        GetHasDisplay = 0x00000001,
        SendOutput = 0x00000002,
        IsEmulator = 0x00000003,
        SendCtrlData = 0x00000010,
        EmitScreenshot = 0x00000020,
    }

    public class HleIoDriverEmulator : AbstractHleIoDriver, IHleIoDriver
    {
        [Inject] PspDisplay PspDisplay;

        [Inject] DisplayConfig DisplayConfig;

        [Inject] HleOutputHandler HleOutputHandler;

        [Inject] PspHleRunningConfig PspHleRunningConfig;

        public unsafe int IoInit()
        {
            return 0;
        }

        public unsafe int IoExit()
        {
            return 0;
        }

        public unsafe int IoOpen(HleIoDrvFileArg HleIoDrvFileArg, string FileName, HleIoFlags Flags, SceMode Mode)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoClose(HleIoDrvFileArg HleIoDrvFileArg)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoRead(HleIoDrvFileArg HleIoDrvFileArg, byte* OutputPointer, int OutputLength)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoWrite(HleIoDrvFileArg HleIoDrvFileArg, byte* InputPointer, int InputLength)
        {
            throw new NotImplementedException();
        }

        public unsafe long IoLseek(HleIoDrvFileArg HleIoDrvFileArg, long Offset, SeekAnchor Whence)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoIoctl(HleIoDrvFileArg HleIoDrvFileArg, uint Command, Span<byte> Input, Span<byte> Output)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoRemove(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoMkdir(HleIoDrvFileArg HleIoDrvFileArg, string Name, SceMode Mode)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoRmdir(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoDopen(HleIoDrvFileArg HleIoDrvFileArg, string Name)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoDclose(HleIoDrvFileArg HleIoDrvFileArg)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoDread(HleIoDrvFileArg HleIoDrvFileArg, HleIoDirent* dir)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoGetstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* Stat)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoChstat(HleIoDrvFileArg HleIoDrvFileArg, string FileName, SceIoStat* stat, int bits)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoRename(HleIoDrvFileArg HleIoDrvFileArg, string OldFileName, string NewFileName)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoChdir(HleIoDrvFileArg HleIoDrvFileArg, string DirectoryName)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoMount(HleIoDrvFileArg HleIoDrvFileArg)
        {
            throw new NotImplementedException();
        }

        public unsafe int IoUmount(HleIoDrvFileArg HleIoDrvFileArg)
        {
            throw new NotImplementedException();
        }

        int ScreenShotCount = 0;

        public unsafe int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command, Span<byte> Input, Span<byte> Output)
        {
            switch (DeviceName)
            {
                case "emulator:": break;
                case "kemulator:": break;
                default: throw new InvalidOperationException();
            }

            //Console.Error.WriteLine("     {0}", (EmulatorDevclEnum)Command);
            switch ((EmulatorDevclEnum) Command)
            {
                case EmulatorDevclEnum.GetHasDisplay:
                    ReinterpretSpan<int>(Output)[0] = DisplayConfig.Enabled ? 1 : 0;
                    break;
                case EmulatorDevclEnum.SendOutput:
                    var OutputString = Encoding.ASCII.GetString(ReinterpretSpan<byte>(Input));
                    Console.WriteLine($"HleOutputHandler :: {HleOutputHandler.GetType()}");
                    this.HleOutputHandler.Output(OutputString);
                    //Console.Error.WriteLine("{0}", OutputString);
                    break;
                case EmulatorDevclEnum.IsEmulator:
                    return 0;
                case EmulatorDevclEnum.EmitScreenshot:
                    if (PspHleRunningConfig.FileNameBase == null || PspHleRunningConfig.FileNameBase == "")
                        throw new Exception("PspHleRunningConfig.FileNameBase is empty");
                    this.PspDisplay.TakeScreenshot()
                        .Save(
                            $"{PspHleRunningConfig.FileNameBase}.lastoutput.{ScreenShotCount++}.png", ImageFormat.Png);
                    break;
                default:
                    Console.Error.WriteLine("Unknown emulator command '{0}':0x{1:X} <- {2}", DeviceName, Command,
                        (EmulatorDevclEnum) Command);
                    return -1;
            }
            return -1;
        }

        public unsafe int IoUnk21(HleIoDrvFileArg HleIoDrvFileArg)
        {
            throw new NotImplementedException();
        }
    }
}