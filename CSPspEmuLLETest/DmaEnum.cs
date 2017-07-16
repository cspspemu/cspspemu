using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmuLLETest
{
    public enum DmaEnum : uint
    {
        SystemConfig__RESET_ENABLE = 0xBC10004C,
        SystemConfig__BUS_CLOCK_ENABLE = 0xBC100050,
        SystemConfig__DEVKIT = 0xBC100068, // Set to 0xFFFF0000 for devkit.
        SystemConfig__IO_ENABLE = 0xBC100078,
        SystemConfig__GPIO_IO_ENABLE = 0xBC10007C,

        GPIO = 0xBE240000,
        GPIO__PORT_READ = 0xBE240004,
        GPIO__PORT_WRITE = 0xBE240008,
        GPIO__PORT_CLEAR = 0xBE24000C,

        MSTICK__COMMAND = 0xBD200030,
        MSTICK__DATA = 0xBD200034,
        MSTICK__STATUS = 0xBD200038,
        MSTICK__SYS = 0xBD20003C,

        NAND__CONTROL = 0xBD101000,

        //NAND__MODE = 0xBD101000,
        NAND__STATUS = 0xBD101004,
        NAND__COMMAND = 0xBD101008,
        NAND__ADDRESS = 0xBD10100C,
        NAND__RESET = 0xBD101014,

        NAND__DMA_ADDRESS = 0xBD101020,
        NAND__DMA_CONTROL = 0xBD101024,
        NAND__DMA_ERROR = 0xBD101028,

        NAND__READDATA = 0xBD101300,
        //NAND__DAT = 0xBD101300,

        NAND__DATA_PAGE_START = 0xBFF00000,
        NAND__DATA_PAGE_END = 0xBFF00200,
        NAND__DATA_SPARE_BUF0_REG = 0xBFF00900,
        NAND__DATA_SPARE_BUF1_REG = 0xBFF00904,
        NAND__DATA_SPARE_BUF2_REG = 0xBFF00908,
        NAND__DATA_EXTRA_END = NAND__DATA_SPARE_BUF2_REG + 4,

        KIRK_SIGNATURE = 0xBDE00000, // 'K', 'I', 'R', 'K'
        KIRK_VERSION = 0xBDE00004, // 0, 0, 1, 0
        KIRK_ERROR = 0xBDE00008, // set to 1 on incomplete processing
        KIRK_START = 0xBDE0000C, // set this to 1 or 2 to start phase 1/2 of the processing
        KIRK_COMMAND = 0xBDE00010,
        KIRK_RESULT = 0xBDE00014, // result of semaphore_XXXXXXXX functions (exported)
        KIRK_UNK_18 = 0xBDE00018, // ???
        KIRK_PATTERN = 0xBDE0001C, // pattern to check status of processing
        KIRK_ASYNC_PATTERN = 0xBDE00020, // pattern set before starting an async processing
        KIRK_ASYNC_PATTERN_END = 0xBDE00024, // value of asyncPattern after processing
        KIRK_PATTERN_END = 0xBDE00028, // value of pattern after processing
        KIRK_SOURCE_ADDRESS = 0xBDE0002C, // physical address of source buffer
        KIRK_DESTINATION_ADDRESS = 0xBDE00030, // physical address of destination buffer
        KIRK_UNK_4C = 0xBDE0004C,
        KIRK_UNK_50 = 0xBDE00050,
    }
}