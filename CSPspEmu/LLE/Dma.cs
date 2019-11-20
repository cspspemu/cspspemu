using System;
using System.Diagnostics;
using CSPspEmu.Core.Cpu;

namespace CSPspEmuLLETest
{
    public class Dma
    {
        public enum Direction
        {
            Write,
            Read,
        }

        public CpuThreadState CpuThreadState;

        public LleState LleState;
        //0xBC100004

        public Dma(CpuThreadState cpuThreadState)
        {
            this.CpuThreadState = cpuThreadState;
        }
        
        bool LogDMAReads = true;

        private string GetRegisterName(uint address)
        {
            var reg = "";
            var dmaAddress = (DmaEnum) address;

            if (dmaAddress >= DmaEnum.NAND__DATA_PAGE_START && dmaAddress < DmaEnum.NAND__DATA_PAGE_END)
            {
                return $"NAND__DATA_PAGE[{dmaAddress - DmaEnum.NAND__DATA_PAGE_START}]";
            }

            if (Enum.IsDefined(typeof(DmaEnum), address))
            {
                reg = $"{(DmaEnum) address}({(uint) address:X8})";
            }
            else
            {
                reg = $"Unknown({(uint) address:X8})";
            }

            return reg;
        }

        public void LogDma(Dma.Direction direction, int size, uint address, ref uint value)
        {
            var dmaAddress = (DmaEnum) address;
            if (dmaAddress >= DmaEnum.NAND__DATA_PAGE_START && dmaAddress < DmaEnum.NAND__DATA_PAGE_END) return;
            Console.WriteLine("PC({0:X8}) {1}: {2} : 0x{3:X8}", CpuThreadState.Pc, direction, GetRegisterName(address),
                value);
        }

        byte[] _test = new byte[0x1000];

        public void TransferDma(Dma.Direction direction, int size, DmaEnum address, ref uint value)
        {
            if (false)
            {
            }
            /*
            else if ((uint)Address >= 0x1FD00000 && (uint)Address <= 0x1FD00000 + 0x1000)
            {
                TransferUtils.TransferToArray(Direction, Test, (int)(Address - 0x1FD00000), Size, ref Value);
            }
            */
            else if (address >= DmaEnum.GPIO && address <= DmaEnum.GPIO__PORT_CLEAR)
            {
                LleState.Gpio.Transfer(direction, size, (DmaEnum) address, ref value);
                return;
            }
            else if (address >= DmaEnum.NAND__CONTROL && address <= DmaEnum.NAND__READDATA)
            {
                LleState.Nand.Transfer(direction, size, (DmaEnum) address, ref value);
                return;
            }
            else if (address >= DmaEnum.NAND__DATA_PAGE_START && address <= DmaEnum.NAND__DATA_EXTRA_END)
            {
                LleState.Nand.Transfer(direction, size, (DmaEnum) address, ref value);
                return;
            }
            else if (address >= DmaEnum.KIRK_SIGNATURE && address <= DmaEnum.KIRK_UNK_50)
            {
                LleState.LleKirk.Transfer(direction, size, (DmaEnum) address, ref value);
                return;
            }
            else
            {
                Console.WriteLine("Unprocessed LLEState.Memory:{0}", LleState.Memory.MountedPreIpl);
                //Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public uint ReadDma(int size, uint address)
        {
            uint value = 0;

            TransferDma(Direction.Read, size, (DmaEnum) address, ref value);
            LogDma(Direction.Read, size, address, ref value);

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="address"></param>
        /// <param name="value"></param>
        public void WriteDma(int size, uint address, uint value)
        {
            LogDma(Direction.Write, size, address, ref value);
            TransferDma(Direction.Write, size, (DmaEnum) address, ref value);

            switch ((DmaEnum) address)
            {
                case DmaEnum.SystemConfig__RESET_ENABLE:
                    LleState.Memory.MountedPreIpl = false;
                    /*
                    if ((Value & 2) != 0) // ME
                    {
                        LLEState.Me.Reset();
                    }
                    */
                    break;
            }
        }
    }
}