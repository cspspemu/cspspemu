using System;
using System.IO;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmuLLETest
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="http://hitmen.c02.at/files/yapspd/psp_doc/chap8.html#sec8.6"/>
    public class LleNand : ILleDma
    {
        [Flags]
        public enum EnumControlRegister : uint
        {
            CalculateEccWhenWritting = (1 << 17),
            CalculateEccWhenReading = (1 << 16),
        }

        public enum EnumCommands : uint
        {
            Reset = 0xFF,
        }

        [Flags]
        public enum EnumStatus : uint
        {
            WriteProtected = (1 << 7),
            Ready = (1 << 0), // 1 - READY | 0 - BUSY
        }

        EnumControlRegister _controlRegister;
        EnumStatus _status;
        uint _command;

        /// <summary>
        /// Physical page to access
        /// </summary>
        uint _address;

        /// <summary>
        /// 
        /// </summary>
        uint _dmaAddress;

        /// <summary>
        /// 
        /// </summary>
        public Stream NandStream;

        public byte[] NandBlock;

        public LleNand(Stream nandStream)
        {
            NandStream = nandStream;
        }


        private void Reset()
        {
            Console.WriteLine("Reset NAND controller to default state?");
        }

        public void Transfer(Dma.Direction direction, int size, DmaEnum address, ref uint value)
        {
            // Reading sector
            if ((address >= DmaEnum.NAND__DATA_PAGE_START) && (address < DmaEnum.NAND__DATA_PAGE_END))
            {
                var offset = (int) (address - DmaEnum.NAND__DATA_PAGE_START);
                //Console.WriteLine("{0:X8}", (uint)Address);
                //Console.WriteLine("Transfer {0} / {1} [{2}]", Offset, Size, NandBlock.Length);
                TransferUtils.TransferToArray(direction, NandBlock, offset, size, ref value);
                return;
            }

            if ((address >= DmaEnum.NAND__DATA_SPARE_BUF0_REG) && (address < DmaEnum.NAND__DATA_EXTRA_END))
            {
                var offset = (int) (address - DmaEnum.NAND__DATA_SPARE_BUF0_REG);
                TransferUtils.TransferToArray(direction, NandBlock, 512 + offset + 4, size, ref value);
                return;
            }

            switch (address)
            {
                case DmaEnum.NAND__CONTROL:
                    TransferUtils.Transfer(direction, ref _controlRegister, ref value);
                    break;
                case DmaEnum.NAND__STATUS:
                    TransferUtils.Transfer(direction, ref _status, ref value);
                    //Thread.Sleep(200);
                    break;
                case DmaEnum.NAND__COMMAND:
                    TransferUtils.Transfer(direction, ref _command, ref value);

                    // Reset
                    if (direction == Dma.Direction.Write)
                    {
                        switch ((EnumCommands) value)
                        {
                            case EnumCommands.Reset:
                                _status = EnumStatus.Ready;
                                break;
                        }
                    }
                    break;
                case DmaEnum.NAND__ADDRESS:
                    TransferUtils.Transfer(direction, ref address, ref value);
                    break;
                case DmaEnum.NAND__RESET:
                    if (direction == Dma.Direction.Write)
                    {
                        Reset();
                    }
                    break;
                case DmaEnum.NAND__DMA_ADDRESS:
                    TransferUtils.Transfer(direction, ref _dmaAddress, ref value);
                    break;
                case DmaEnum.NAND__DMA_CONTROL:
                    if (direction == Dma.Direction.Write)
                    {
                        if (value == 0x301)
                        {
                            //0x20000/2/512*(512+16)
                            NandStream.Position = ((_dmaAddress / 2 / 512) * (512 + 16));
                            NandBlock = NandStream.ReadBytes(512 + 16);
                            Console.WriteLine("Read from NAND: 0x{0:X8}", _dmaAddress);
                            ArrayUtils.HexDump(NandBlock);

                            //Thread.Sleep(TimeSpan.FromSeconds(0.5));
                            //Thread.Sleep(-1);
                        }
                    }
                    else
                    {
                        value = 0;
                    }
                    break;
                case DmaEnum.NAND__DMA_ERROR:
                    value = 0;
                    break;
            }
        }
    }
}