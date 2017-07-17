using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpUtils;
using Kirk = CSPspEmu.Core.Components.Crypto.Kirk;

namespace CSPspEmuLLETest
{
    unsafe public class LleKirk : ILleDma
    {
        DebugPspMemory Memory;
        Kirk Kirk;
        uint _kirkSource;
        uint _kirkDestination;
        uint _kirkCommand;
        uint _kirkResult;

        public LleKirk(DebugPspMemory memory)
        {
            this.Memory = memory;
            Kirk = new Kirk();
            Kirk.kirk_init();
        }

        public void Transfer(Dma.Direction direction, int size, DmaEnum address, ref uint value)
        {
            switch (address)
            {
                case DmaEnum.KIRK_PATTERN:
                    value = 1;
                    break;
                case DmaEnum.KIRK_COMMAND:
                    TransferUtils.Transfer(direction, ref _kirkCommand, ref value);
                    break;
                case DmaEnum.KIRK_RESULT:
                    TransferUtils.Transfer(direction, ref _kirkResult, ref value);
                    break;
                case DmaEnum.KIRK_SOURCE_ADDRESS:
                    TransferUtils.Transfer(direction, ref _kirkSource, ref value);
                    break;
                case DmaEnum.KIRK_DESTINATION_ADDRESS:
                    TransferUtils.Transfer(direction, ref _kirkDestination, ref value);
                    break;
                case DmaEnum.KIRK_START:
                    if (_kirkCommand != 1) throw(new NotImplementedException());

                    var sourcePtr = (byte*) Memory.PspAddressToPointerSafe(_kirkSource);
                    var destinationPtr = (byte*) Memory.PspAddressToPointerSafe(_kirkDestination);

                    //var Out = new byte[10000];

                    //fixed (byte* OutPtr = Out)
                {
                    //DestinationPtr = OutPtr;
                    Console.WriteLine("Input:");
                    ArrayUtils.HexDump(PointerUtils.PointerToByteArray(sourcePtr, 0x200));

                    /*
                    try
                    {
                        Kirk.kirk_CMD1(DestinationPtr, SourcePtr, -1, true);
                        this.KirkResult = (uint)Kirk.ResultEnum.OK;
                    }
                    catch (Kirk.KirkException KirkException)
                    {
                        this.KirkResult = (uint)KirkException.Result;
                        Console.Error.WriteLine("Kirk.KirkException : {0}", KirkException);
                    }
                    */

                    this._kirkResult = (uint) Kirk.SceUtilsBufferCopyWithRange(destinationPtr, -1, sourcePtr, -1, 1);

                    Console.WriteLine("Output:");
                    ArrayUtils.HexDump(PointerUtils.PointerToByteArray(destinationPtr, 0x200));
                    Console.WriteLine("LOADADDR:{0:X8}", ((uint*) destinationPtr)[0]);
                    Console.WriteLine("BLOCKSIZE:{0:X8}", ((uint*) destinationPtr)[1]);
                    Console.WriteLine("ENTRY:{0:X8}", ((uint*) destinationPtr)[2]);
                    Console.WriteLine("CHECKSUM:{0:X8}", ((uint*) destinationPtr)[3]);
                }

                    //Thread.Sleep(4);
                    break;
            }
        }
    }
}